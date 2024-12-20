using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Facade;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Registry;
using F23.StringSimilarity;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Signers;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Algo.Algotithms
{
    public class CosineSimAlgo : SimilarityCalculator
    {
        private const double epsilon = 1e-9;
        private IENSHandler eNSHandler;
        private IProgressStrategy progressStrategy;

        private ENSHandlerRegistry handlerRegistry;
        private Cosine cosine;
        public CosineSimAlgo
            (IENSHandler eNSHandler, 
            IProgressStrategy progressStrategy,
            ENSHandlerRegistry handlerRegistry,
            Cosine cosine)
        {            
            this.eNSHandler = eNSHandler;
            this.progressStrategy = progressStrategy;
            this.handlerRegistry = handlerRegistry;
            this.cosine = cosine;
        }
        //основной алгоритм в данном классе
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<TGarbageData, Dictionary<TStandart, double>> mid, Dictionary<TGarbageData, Dictionary<TStandart, double>> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
        {
            currentProgress = 0;          

            ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing = new();

            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag = new();
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> midBag = new();
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

            progressStrategy.UpdateProgress(new Models.Progress { Step = "Алгоритм завершил свою работу. Ожидайте записи результатов обработки в файл", CurrentProgress = 100 });
            
            return (worst,mid,best);
        }


        public string SelectHandler(string groupClassificationName, string baseProcessedGarbageName) //метод для выбора обработчика в соответствие с классификатором ЕНС.
        {
            var handler = handlerRegistry.GetHandler(groupClassificationName);
            if (handler is not null)
            {
                return handler(baseProcessedGarbageName);
            }
            return baseProcessedGarbageName;
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
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> midBag, 
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag)
        where TStandart : IStandart
        where TGarbageData : IGarbageData
        {
            Parallel.ForEach(matchedData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                ConcurrentDictionary<TStandart, double> bestStandart = new();//необходимо для запоминания лучших сопоставленных эталонов для каждой грязной позиции
                int commonElementsCount = 0;//количество общих чисел у грязной позиции и эталона
                double similarityCoeff = -1;

                var garbageItem = item.GarbageItem;
                var garbageDataHandeledName = garbageItem.ProcessedName;//наименование грязной позиции БЕЗ ГОСТов
                var garbageDataItem = garbageItem.Data; //TGarbageData
                var garbageDataGosts = garbageItem.ProcessedGosts; //вытянутые из наименования грязной позиции ГОСТы

                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataHandeledName);//дефолтная обработка наименования грязной позиции, так же, как и для эталона
                var tokens = GetTokensFromName(baseProcessedGarbageName, garbageDataGosts);

                string improvedProcessedGarbageName = "";
                var standartStuff = item.Matches; //сопоставленные группы эталонов для грязной позиции по ГОСТам
                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией
                {
                    var groupClassificationName = standartGroups.Key;

                    //персональные обработчики для классификаторов ЕНС
                    improvedProcessedGarbageName = SelectHandler(groupClassificationName, baseProcessedGarbageName);
                    foreach (var standart in standartGroups.Value) //стандарты в каждой отдельной группе
                    {
                        var similarity = cosine.Similarity(improvedProcessedGarbageName, standart.Value);//основной алго

                        //выделение уникальных чисел для позиции эталона
                        var standartGosts = new HashSet<string>() { standart.Key.MaterialNTD, standart.Key.NTD };
                        var standartTokens = GetTokensFromName(standart.Value, standartGosts);

                        if (similarity > similarityCoeff)//сравнение с предыдущим наилучшим результатом
                        {
                            similarityCoeff = similarity;
                            bestStandart.TryAdd(standart.Key, similarity);
                            commonElementsCount = standartTokens.Where(tokens.Contains).ToArray().Length;
                        }
                        else if (similarity == similarityCoeff)
                        {
                            int commonElementsCountNow = standartTokens.Where(tokens.Contains).ToArray().Length;
                            if (commonElementsCountNow > commonElementsCount)
                                bestStandart.TryAdd(standart.Key, similarity);
                        }
                        else if (similarity - similarityCoeff < 0.1)//если эталон по уровню сопоставления не очень сильно отличается от "идеального" на тот момент сопоставления, то есть вероятность того, что именно этот эталон и будет искомым
                            bestStandart.TryAdd(standart.Key, similarity);
                    }
                }
                var bestOfOrderedStandarts = GetBestStandarts(bestStandart.ToDictionary());

                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.1) //данным грязным позициям даётся второй шанс на дефолтном прогоне
                    dataForPostProcessing.Add((garbageDataItem, improvedProcessedGarbageName, garbageDataGosts));
                else if (Math.Abs(similarityCoeff - 1) < epsilon)//для сопоставления с уровнем 1, берём в качестве сопоставленного эталона позицию с уронем сопоставления 1 и только её. Логика может быть изменена(брать три лучших, но при этом опустить границу с 1 до 0,95). Необходимо продумать данную логику, чтобы не изменять данный метод.
                    AddToBestBag(bestBag, garbageDataItem, bestOfOrderedStandarts);
                else if (similarityCoeff < 1)
                    midBag.TryAdd(garbageDataItem, bestOfOrderedStandarts);
                

                LogProgress(100, matchedData.Count,ref currentProgress, "3. Базовый прогон алгоритма Cosine");
            });
        }

        public void DefaultRun<TStandart,TGarbageData>
            (ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing, 
            ConcurrentDictionary<TStandart, string> allProcessedStandarts,
            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag, 
            ConcurrentDictionary<TGarbageData,Dictionary<TStandart, double>> midBag, 
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag)
        {
            Parallel.ForEach(dataForPostProcessing, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                var (garbageDataItem, garbageName, gosts) = item;
                Dictionary<TStandart, double> bestStandart = new();
                double similarityCoeff = -1;
                foreach (var (standart, standartName) in allProcessedStandarts)
                {
                    double coeff = cosine.Similarity(garbageName, standartName);
                    if (coeff > similarityCoeff)
                    {
                        similarityCoeff = coeff;
                        bestStandart.TryAdd(standart, coeff);
                    }
                    else if (coeff - similarityCoeff < 0.2)
                        bestStandart.TryAdd(standart, coeff);
                }
                var bestOfOrderedStandarts = GetBestStandarts(bestStandart);

                if ((gosts.Count == 0 || gosts.All(t => t.Length == 0)) && similarityCoeff > 0.05)//если у позиции отсутствует ГОСТ, то она переносится в коллекцию требует уточнения
                    midBag.TryAdd(garbageDataItem, bestOfOrderedStandarts);
                else
                {
                    var orderedStandart = bestOfOrderedStandarts.FirstOrDefault();
                    if (similarityCoeff < 0.1)
                        worstBag.TryAdd((garbageDataItem, orderedStandart.Key), Math.Round(orderedStandart.Value, 3));
                    else if (Math.Abs(similarityCoeff - 1) < epsilon)
                        AddToBestBag(bestBag, garbageDataItem, bestOfOrderedStandarts);
                    else if (similarityCoeff < 1)
                        midBag.TryAdd(garbageDataItem, bestOfOrderedStandarts);
                    
                }
                LogProgress(10, dataForPostProcessing.Count,ref currentProgress, "4. Дополнительный прогон алгоритма Cosine");
            });
        }

        public List<long> GetTokensFromName(string name, HashSet<string> gosts)
        {
            var tokens = name.Split().Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
            foreach (var handledGost in gosts)
            {
                var handledGostTokens = handledGost.Split(new char[] { ' ', '-' }).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                foreach (var handledToken in handledGostTokens)
                {
                    tokens.Add(handledToken);
                }
            }
            return tokens;
        }
        
        public Dictionary<TStandart, double> GetBestStandarts<TStandart>(Dictionary<TStandart, double> bestStandart)
        {
            bool addedFirst = false;
            var orderedStandarts = bestStandart.OrderByDescending(t => t.Value).Take(3);
            return orderedStandarts.Where((kvp, index) =>
            {
                if (index < orderedStandarts.Count() - 1)
                {
                    var nextValue = Math.Round(orderedStandarts.ElementAt(index + 1).Value, 4);
                    if (Math.Abs(nextValue - Math.Round(kvp.Value, 4)) > 0.2)
                    {
                        addedFirst = true;
                        return true;
                    }
                }
                return !addedFirst;
            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void AddToBestBag<TStandart, TGarbageData>(ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag, TGarbageData garbageDataItem, Dictionary<TStandart,double> bestOfOrderedStandarts)
        {
            var orderedStandart = bestOfOrderedStandarts.FirstOrDefault();
            bestBag.TryAdd(garbageDataItem, new Dictionary<TStandart, double>() { { orderedStandart.Key, orderedStandart.Value } });
        }

        public void LogProgress(int freq, int counter, ref int currentProgress, string step)
        {
            if (currentProgress % freq == 0)
            {
                progressStrategy.UpdateProgress(new Models.Progress { Step = step, CurrentProgress = Math.Round((double)currentProgress / counter * 100, 2) });
                progressStrategy.LogProgress();
            }
            currentProgress = Interlocked.Increment(ref currentProgress);
        }

        public (Dictionary<(TGarbageData, TStandart?), double>, Dictionary<TGarbageData, Dictionary<TStandart, double>>, Dictionary<TGarbageData, Dictionary<TStandart, double>>) TransferData<TStandart,TGarbageData>
            (ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag, ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> midBag, ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag)
        {
            Dictionary<(TGarbageData, TStandart?), double> worst = new();
            Dictionary<TGarbageData, Dictionary<TStandart, double>> mid = new();
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
            return (worst, mid.OrderByDescending(kvp => kvp.Value.FirstOrDefault().Value).ToDictionary(), best);
        }
    }
}
