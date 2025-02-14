﻿using AbstractionsAndModels.Abstract;
using AbstractionsAndModels.Facade;
using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.Models;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
using Algo.Registry;
using Algo.Simpled;
using F23.StringSimilarity;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Algo.Algotithms
{
    public class CosineSimAlgo : SimilarityCalculator
    {
        private const double epsilon = 1e-9;
        private IENSHandler eNSHandler;
        private ProgressStrategy progressStrategy;

        private ENSHandlerRegistry handlerRegistry;
        private OrderService orderService;
        //private Cosine cosine;
        private CosineSimpled cosineSimpled;
        public CosineSimAlgo
            (IENSHandler eNSHandler, 
            ProgressStrategy progressStrategy,
            ENSHandlerRegistry handlerRegistry,
            OrderService orderService,
            /*Cosine cosine*/CosineSimpled cosineSimpled)
        {            
            this.eNSHandler = eNSHandler;
            this.progressStrategy = progressStrategy;
            this.handlerRegistry = handlerRegistry;
            this.orderService = orderService;
            //this.cosine = cosine;
            this.cosineSimpled = cosineSimpled;
        }
        //основной алгоритм в данном классе
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<TGarbageData, (Dictionary<TStandart, double>, string)> mid, Dictionary<TGarbageData, Dictionary<TStandart, double>> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
        {
            currentProgress = 0;

            ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing = new();

            ConcurrentDictionary<(TGarbageData, TStandart), double> worstBag = new();
            ConcurrentDictionary<TGarbageData, (Dictionary<TStandart, double>, string)> midBag = new();
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag = new();

            var matchedData = algoResult.MatchedData; //грязные позиции, для которых нашлось сопоставление с группами эталонов
            var processedStandarts = algoResult.ProcessedStandards;//все обработанные на этапе AlgoWrap стандарты
            var unmatchedGarbageData = algoResult.UnmatchedGarbageData;//грязные позиции без сопоставления

            //первый прогон алгоритма для сопоставленных грязных позиций
            MainRun(matchedData, dataForPostProcessing, midBag, bestBag);
            currentProgress = 0;

            //добавляем в коллекцию грязных данных для дефолтного прогона позиции, для которых не было сопоставлено ни одной группы из эталонов.
            ProcessPostProcessingData(unmatchedGarbageData, dataForPostProcessing);
            
            //дополнительный прогон по позициям с для которых не были найдены подходящие стандарты
            var allProcessedStandarts = new ConcurrentDictionary<TStandart, string>(processedStandarts.GroupStandarts.Values.SelectMany(innerDict => innerDict));
            DefaultRun(dataForPostProcessing, allProcessedStandarts, worstBag, midBag, bestBag);
            
            var (worst, mid, best) = TransferData(worstBag, midBag, bestBag);

            progressStrategy.UpdateProgress(new AbstractionsAndModels.Models.Progress { Step = "Алгоритм завершил свою работу. Ожидайте записи результатов обработки в файл", CurrentProgress = 100 });
            
            return (worst,mid,best);
        }


        public string SelectHandler(string groupClassificationName, ProcessingContext processingContext/*string baseProcessedGarbageName*/) //метод для выбора обработчика в соответствие с классификатором ЕНС.
        {
            var handler = handlerRegistry.GetHandler(groupClassificationName);
            if (handler is not null)
            {                
                return handler(/*baseProcessedGarbageName*/processingContext);
            }
            return processingContext.Input;
        }

        public void ProcessPostProcessingData<TGarbageData>(ConcurrentBag<(TGarbageData, HashSet<string>)> unmatchedGarbageData, ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing)
        where TGarbageData : IGarbageData
        {
            Parallel.ForEach(unmatchedGarbageData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                var (garbageData, gosts) = item;
                dataForPostProcessing.Add((garbageData, eNSHandler.BaseStringHandle(garbageData.ShortName), gosts));
            });
        }

        public void MainRun<TStandart, TGarbageData>
            (ConcurrentBag<MatchedResult<TStandart, TGarbageData>> matchedData,
            ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing,
            ConcurrentDictionary<TGarbageData, (Dictionary<TStandart, double>, string)> midBag,
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag)
            where TStandart : IStandart
            where TGarbageData : IGarbageData
        {
            Parallel.ForEach(matchedData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                ConcurrentDictionary<TStandart, (double, double/*,double*/)> bestStandart = new();//необходимо для запоминания лучших сопоставленных эталонов для каждой грязной позиции. В значении первый double - уровень сопоставления. Второй - общее количество числовых элементов с эталоном. (Необходимо для дополнительной сортировки, так как существуют одинаковые наименования, но с разными ГОСТ)
                int commonElementsCount = 0;//количество общих чисел у грязной позиции и эталона
                double similarityCoeff = -1;

                var garbageItem = item.GarbageItem;
                var garbageDataHandeledName = garbageItem.ProcessedName;//наименование грязной позиции БЕЗ ГОСТов
                var garbageDataItem = garbageItem.Data; //TGarbageData
                var garbageDataGosts = garbageItem.ProcessedGosts; //вытянутые из наименования грязной позиции ГОСТы

                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataHandeledName);//дефолтная обработка наименования грязной позиции, так же, как и для эталона
                var tokensFromGosts = /*GetTokensFromName(baseProcessedGarbageName, garbageDataGosts);*/GetTokensFromGosts(garbageDataGosts);
                //var tokensFromName = GetTokensFromName(baseProcessedGarbageName).Count;

                string improvedProcessedGarbageName = "";

                var standartStuff = item.Matches; //сопоставленные группы эталонов для грязной позиции по ГОСТам

                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией. Чаще всего количество сопоставленных групп - 1.
                {
                    var groupClassificationName = standartGroups.Key;
                    //персональные обработчики для классификаторов ЕНС
                    improvedProcessedGarbageName = SelectHandler(groupClassificationName, new ProcessingContext() {Input = baseProcessedGarbageName, Gost = garbageDataGosts.FirstOrDefault() }/*baseProcessedGarbageName*/);
                    foreach (var standart in standartGroups.Value) //стандарты в каждой отдельной группе
                    {
                        var similarity = /*cosine*/cosineSimpled.Similarity(improvedProcessedGarbageName, standart.Value);//основной алго

                        //выделение уникальных чисел для позиции эталона
                        var standartGosts = new HashSet<string>() { standart.Key.MaterialNTD, standart.Key.NTD };
                        var standartTokensFromGosts = /*GetTokensFromName(standart.Value, standartGosts);*/GetTokensFromGosts(standartGosts);
                        //var standartTokensFromName = GetTokensFromName(standart.Value).Count;

                        int commonElementsCountNow = standartTokensFromGosts.Intersect(tokensFromGosts).Count();
                        if (similarity > similarityCoeff)//сравнение с предыдущим наилучшим результатом
                        {              
                            bestStandart.TryAdd(standart.Key, (similarity, commonElementsCountNow/*, standartTokensFromName*/));
                            similarityCoeff = similarity;
                            commonElementsCount = commonElementsCountNow;
                        }
                        else if (similarity == similarityCoeff && commonElementsCountNow > commonElementsCount)
                        {
                            bestStandart.TryAdd(standart.Key, (similarity, commonElementsCountNow/*, standartTokensFromName*/));
                            commonElementsCount = commonElementsCountNow;
                        }                          
                        else if (similarity - similarityCoeff < 0.25/*0.1*/)//если эталон по уровню сопоставления не очень сильно отличается от "идеального" на тот момент сопоставления, то есть вероятность того, что именно этот эталон и будет искомым
                            bestStandart.TryAdd(standart.Key, (similarity, commonElementsCountNow/*, standartTokensFromName*/));
                    }
                }
                var bestOfOrderedStandarts = orderService.GetBestStandarts(bestStandart);//отсортированные стандарты.Сначала по коэффициенту сопоставления, потом по количеству общих чисел из ГОСТов (у позиции с двумя общими ГОСТами приоритет будет выше, чем у позиции с одним общим ГОСТом, если коэффициент сопоставления с этими эталонами идентичен)

                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.1) //данным грязным позициям даётся второй шанс на дефолтном прогоне
                    dataForPostProcessing.Add((garbageDataItem, improvedProcessedGarbageName, garbageDataGosts));
                else if (/*similarityCoeff < 1 ||*/ bestOfOrderedStandarts.FirstOrDefault().Value.Item1 < 1)
                    midBag.TryAdd(garbageDataItem, (DictionaryConverter(bestOfOrderedStandarts), string.Empty)); 
                else if(bestOfOrderedStandarts.FirstOrDefault().Value.Item2 < tokensFromGosts.Count)
                    midBag.TryAdd(garbageDataItem, (DictionaryConverter(bestOfOrderedStandarts), "Для идеально сопоставленного наименования отсутствует полное совпадение по ГОСТам"));              
                else if (Math.Abs(similarityCoeff - 1) < epsilon)//для сопоставления с уровнем 1, берём в качестве сопоставленного эталона позицию с уронем сопоставления 1 и только её. Логика может быть изменена(брать три лучших, но при этом опустить границу с 1 до 0,95). Необходимо продумать данную логику, чтобы не изменять данный метод.
                    AddToBestBag(bestBag, garbageDataItem, DictionaryConverter(bestOfOrderedStandarts));      
                
                LogProgress(10, matchedData.Count, ref currentProgress, "3. Базовый прогон алгоритма Cosine");
            });
        }

        public void DefaultRun<TStandart,TGarbageData>
            (ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing, 
            ConcurrentDictionary<TStandart, string> allProcessedStandarts,
            ConcurrentDictionary<(TGarbageData, TStandart), double> worstBag, 
            ConcurrentDictionary<TGarbageData,(Dictionary<TStandart, double>,string)> midBag, 
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag)
            where TStandart : IStandart
            where TGarbageData : IGarbageData
        {
            Parallel.ForEach(dataForPostProcessing, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                var (garbageDataItem, garbageName, gosts) = item;
                Dictionary<TStandart, double> bestStandart = new();
                double similarityCoeff = -1;
                foreach (var (standart, standartName) in allProcessedStandarts)
                {
                    double coeff = /*cosine*/cosineSimpled.Similarity(garbageName, standartName);
                    if (coeff > similarityCoeff)
                    {
                        similarityCoeff = coeff;
                        bestStandart.TryAdd(standart, coeff);
                    }
                    else if (coeff - similarityCoeff < 0.2)
                        bestStandart.TryAdd(standart, coeff);
                }
                var bestOfOrderedStandarts = orderService.GetBestStandarts(bestStandart);

                if ((gosts.Count == 0 || gosts.All(t => t.Length == 0)) && similarityCoeff > 0.05)//если у позиции отсутствует ГОСТ, то она переносится в коллекцию требует уточнения
                    midBag.TryAdd(garbageDataItem, (bestOfOrderedStandarts, "У позиции с грязными данными отсутствует ГОСТ или ГОСТ не был выявлен алгоритмом"));
                else
                {
                    var orderedStandart = bestOfOrderedStandarts.FirstOrDefault();
                    if (similarityCoeff < 0.1)
                        worstBag.TryAdd((garbageDataItem, orderedStandart.Key), Math.Round(orderedStandart.Value, 3));
                    else if (Math.Abs(similarityCoeff - 1) < epsilon)
                        AddToBestBag(bestBag, garbageDataItem, bestOfOrderedStandarts);
                    else if (similarityCoeff < 1)
                        midBag.TryAdd(garbageDataItem, (bestOfOrderedStandarts, "Прогон по умолчанию. Для позиции с грязными данными не найден соответствующий ГОСТ в эталонах"));
                    
                }
                LogProgress(10, dataForPostProcessing.Count,ref currentProgress, "4. Дополнительный прогон алгоритма Cosine");
            });
        }

        public HashSet<long> GetTokensFromName(string name) 
            => name.Split().Where(s => long.TryParse(s, out _)).Select(long.Parse).ToHashSet();
        //{
        //    var tokens = 
        //    //foreach (var handledGost in gosts)
        //    //{
        //    //    var handledGostTokens = handledGost.Split([' ', '-']).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToHashSet();
        //    //    foreach (var handledToken in handledGostTokens)
        //    //    {
        //    //        tokens.Add(handledToken);
        //    //    }
        //    //}
        //    return tokens;
        //}

        public HashSet<long> GetTokensFromGosts(HashSet<string> gosts)
        {
            var tokens = new HashSet<long>();
            foreach (var handledGost in gosts)
            {
                var handledGostTokens = handledGost.Split([' ', '-', '.']).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToHashSet();
                foreach (var handledToken in handledGostTokens)
                {
                    tokens.Add(handledToken);
                }
            }
            return tokens;
        }
       
        public Dictionary<TStandart, double> DictionaryConverter<TStandart>(Dictionary<TStandart, (double, double)> val)
            => val.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Item1);

        public void AddToBestBag<TStandart, TGarbageData>(ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag, TGarbageData garbageDataItem, Dictionary<TStandart, double> bestOfOrderedStandarts)
        {
            var orderedStandart = bestOfOrderedStandarts.FirstOrDefault();
            bestBag.TryAdd(garbageDataItem, new Dictionary<TStandart, double>() { { orderedStandart.Key, orderedStandart.Value } });
        }

        public void LogProgress(int freq, int counter, ref int currentProgress, string step)
        {
            if (currentProgress % freq == 0)
            {
                progressStrategy.UpdateProgress(new Progress { Step = step, CurrentProgress = Math.Round((double)currentProgress / counter * 100, 2) });
                progressStrategy.LogProgress();
            }
            currentProgress = Interlocked.Increment(ref currentProgress);
        }

        public (Dictionary<(TGarbageData, TStandart), double>, Dictionary<TGarbageData, (Dictionary<TStandart, double>, string)>, Dictionary<TGarbageData, Dictionary<TStandart, double>>) TransferData<TStandart,TGarbageData>
            (ConcurrentDictionary<(TGarbageData, TStandart), double> worstBag, ConcurrentDictionary<TGarbageData, (Dictionary<TStandart, double>,string)> midBag, ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag)
        {
            Dictionary<(TGarbageData, TStandart), double> worst = new();
            Dictionary<TGarbageData, (Dictionary<TStandart, double>,string)> mid = new();
            Dictionary<TGarbageData, Dictionary<TStandart, double>> best = new();

            //перенос данных из потокобезопасных коллекций в обычные
            foreach (var ((item, standart), bestValue) in worstBag)
            {
                worst.Add((item, standart), bestValue);
            }
            foreach (var (item, standart) in midBag)
            {
                mid.Add(item, standart);
            }
            foreach (var (item, standart) in bestBag)
            {
                best.Add(item, standart);
            }
            var sortedDict = mid.OrderByDescending(kvp => kvp.Value.Item1.Values.Max()).ToDictionary();
            return (worst, sortedDict, best);
        }
    }
}
