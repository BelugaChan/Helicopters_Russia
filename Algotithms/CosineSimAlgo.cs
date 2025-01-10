using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Facade;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Registry;
using F23.StringSimilarity;
using Org.BouncyCastle.Asn1.Cmp;
using System.Collections.Concurrent;

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
            ConcurrentDictionary<TGarbageData, (Dictionary<TStandart, double>, string)> midBag,
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag)
            where TStandart : IStandart
            where TGarbageData : IGarbageData
        {
            Parallel.ForEach(matchedData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                ConcurrentDictionary<TStandart, (double, double)> bestStandart = new();//необходимо для запоминания лучших сопоставленных эталонов для каждой грязной позиции. В значении первый double - уровень сопоставления. Второй - общее количество числовых элементов с эталоном. (Необходимо для дополнительной сортировки, так как существуют одинаковые наименования, но с разными ГОСТ)
                int commonElementsCount = 0;//количество общих чисел у грязной позиции и эталона
                double similarityCoeff = -1;

                var garbageItem = item.GarbageItem;
                var garbageDataHandeledName = garbageItem.ProcessedName;//наименование грязной позиции БЕЗ ГОСТов
                var garbageDataItem = garbageItem.Data; //TGarbageData
                var garbageDataGosts = garbageItem.ProcessedGosts; //вытянутые из наименования грязной позиции ГОСТы

                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataHandeledName);//дефолтная обработка наименования грязной позиции, так же, как и для эталона
                var tokens = /*GetTokensFromName(baseProcessedGarbageName, garbageDataGosts);*/GetTokensFromGosts(garbageDataGosts);

                string improvedProcessedGarbageName = "";

                var standartStuff = item.Matches; //сопоставленные группы эталонов для грязной позиции по ГОСТам

                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией. Чаще всего количество сопоставленных групп - 1.
                {
                    var groupClassificationName = standartGroups.Key;
                    //персональные обработчики для классификаторов ЕНС
                    improvedProcessedGarbageName = SelectHandler(groupClassificationName, baseProcessedGarbageName);
                    foreach (var standart in standartGroups.Value) //стандарты в каждой отдельной группе
                    {
                        var similarity = cosine.Similarity(improvedProcessedGarbageName, standart.Value);//основной алго

                        //выделение уникальных чисел для позиции эталона
                        var standartGosts = new HashSet<string>() { standart.Key.MaterialNTD, standart.Key.NTD };
                        var standartTokens = /*GetTokensFromName(standart.Value, standartGosts);*/GetTokensFromGosts(standartGosts);
                        int commonElementsCountNow = standartTokens.Intersect(tokens).Count();
                        if (similarity > similarityCoeff)//сравнение с предыдущим наилучшим результатом
                        {              
                            bestStandart.TryAdd(standart.Key, (similarity, commonElementsCountNow));
                            similarityCoeff = similarity;
                            commonElementsCount = commonElementsCountNow;
                        }
                        else if (similarity == similarityCoeff && commonElementsCountNow > commonElementsCount)
                        {
                            bestStandart.TryAdd(standart.Key, (similarity, commonElementsCountNow));
                            commonElementsCount = commonElementsCountNow;
                        }                          
                        else if (similarity - similarityCoeff < 0.25/*0.1*/)//если эталон по уровню сопоставления не очень сильно отличается от "идеального" на тот момент сопоставления, то есть вероятность того, что именно этот эталон и будет искомым
                            bestStandart.TryAdd(standart.Key, (similarity, commonElementsCountNow));
                    }
                }
                var bestOfOrderedStandarts = GetBestStandarts(bestStandart);

                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.1) //данным грязным позициям даётся второй шанс на дефолтном прогоне
                    dataForPostProcessing.Add((garbageDataItem, improvedProcessedGarbageName, garbageDataGosts));
                else if (similarityCoeff < 1)
                    midBag.TryAdd(garbageDataItem, (DictionaryConverter(bestOfOrderedStandarts), string.Empty)); 
                else if (bestOfOrderedStandarts.Where(kvp => kvp.Value.Item1 == 1).All(kvp => kvp.Value.Item2 != tokens.Count) && tokens/*garbageDataGosts*/.Count == 1) //для данного случая требуется наличие у всех идеально сопоставленных эталонов несовпадения по ГОСТам, при том, что коичество ГОСТов будет равно 1.         
                    midBag.TryAdd(garbageDataItem, (DictionaryConverter(bestOfOrderedStandarts), "Наличие идеально сопоставленной записи с некорректным ГОСТом"));
                else if (bestOfOrderedStandarts.Where(kvp => kvp.Value.Item1 == 1 && kvp.Value.Item2 < tokens.Count || kvp.Value.Item1 > 0.75/*0.9*/ && kvp.Value.Item2 == tokens.Count).Count() > 1
                        && !bestOfOrderedStandarts.Where(kvp => kvp.Value.Item1 == 1 && kvp.Value.Item2 == tokens.Count).Any()) //проверка случая, когда запись эталона и грязной позиции равны, но хотя бы один из ГОСТов отличается и в bestOfOrderedStandarts существует позиция, очень похожая на грязную позицию, у которой с грязной позицие совпадают все госты (в данном случае сравниваются все числа в наименовании)
                    midBag.TryAdd(garbageDataItem, (DictionaryConverter(bestOfOrderedStandarts), "Наличие идеально сопоставленной записи с некорректным ГОСТом")); //для данного случая необходимо наличие как минимум двух записей. Одна идеально сопоставлена по наименованию но не идеальна по ГОСТам, а другая наоборот
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
                    midBag.TryAdd(garbageDataItem, (bestOfOrderedStandarts, "У позиции с грязными данными отсутствует ГОСТ"));
                else
                {
                    var orderedStandart = bestOfOrderedStandarts.FirstOrDefault();
                    if (similarityCoeff < 0.1)
                        worstBag.TryAdd((garbageDataItem, orderedStandart.Key), Math.Round(orderedStandart.Value, 3));
                    else if (Math.Abs(similarityCoeff - 1) < epsilon)
                        AddToBestBag(bestBag, garbageDataItem, bestOfOrderedStandarts);
                    else if (similarityCoeff < 1)
                        midBag.TryAdd(garbageDataItem, (bestOfOrderedStandarts, string.Empty));
                    
                }
                LogProgress(10, dataForPostProcessing.Count,ref currentProgress, "4. Дополнительный прогон алгоритма Cosine");
            });
        }

        public HashSet<long> GetTokensFromName(string name, HashSet<string> gosts)
        {
            var tokens = name.Split().Where(s => long.TryParse(s, out _)).Select(long.Parse).ToHashSet();
            foreach (var handledGost in gosts)
            {
                var handledGostTokens = handledGost.Split([' ', '-']).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToHashSet();
                foreach (var handledToken in handledGostTokens)
                {
                    tokens.Add(handledToken);
                }
            }
            return tokens;
        }

        public HashSet<long> GetTokensFromGosts(HashSet<string> gosts)
        {
            var tokens = new HashSet<long>();
            foreach (var handledGost in gosts)
            {
                var handledGostTokens = handledGost.Split([' ', '-']).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToHashSet();
                foreach (var handledToken in handledGostTokens)
                {
                    tokens.Add(handledToken);
                }
            }
            return tokens;
        }

        public Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double)> bestStandart)
        {
            // Создаём копию данных для работы
            var orderedStandarts = bestStandart
                .OrderByDescending(t => t.Value.Item1)
                .ThenByDescending(t => t.Value.Item2)
                .Take(3)
                .ToArray();

            bool addedFirst = false; //данный флаг необходим для того, чтобы понять, был ли встречен элемент, для которого рзаница коэффициентов совпадения текущего и последующего больше 0,25
            var result = new Dictionary<TStandart, (double, double)>();

            for (int i = 0; i < orderedStandarts.Length; i++)
            {
                var (coeff, commonEls) = orderedStandarts[i].Value;

                if (i < orderedStandarts.Length - 1)
                {
                    var (nextCoeff, nextCommonEls) = orderedStandarts[i + 1].Value;
                    if (Math.Abs(Math.Round(nextCoeff, 4) - Math.Round(coeff, 4)) > 0.25)
                    {
                        addedFirst = true;
                        result[orderedStandarts[i].Key] = orderedStandarts[i].Value;
                        break;
                    }
                }

                if (!addedFirst)
                    result[orderedStandarts[i].Key] = orderedStandarts[i].Value;
            }

            return result;
        }


        public Dictionary<TStandart, double> GetBestStandarts<TStandart>(Dictionary<TStandart, double> bestStandart)
        {
            // Упорядочиваем элементы и берём топ-3
            var orderedStandarts = bestStandart
                .OrderByDescending(t => t.Value)
                .Take(3)
                .ToList(); // Преобразуем в список для эффективного индексирования

            var result = new Dictionary<TStandart, double>();
            bool addedFirst = false;

            for (int i = 0; i < orderedStandarts.Count; i++)
            {
                var currentCoeff = orderedStandarts[i].Value;

                // Проверяем разницу с коэффициентом следующего элемента
                if (i < orderedStandarts.Count - 1)
                {
                    var nextCoeff = orderedStandarts[i + 1].Value;
                    if (Math.Abs(Math.Round(nextCoeff, 4) - Math.Round(currentCoeff, 4)) > 0.2)
                    {
                        addedFirst = true;
                        result[orderedStandarts[i].Key] = currentCoeff;
                        break;
                    }
                }

                if (!addedFirst)
                    result[orderedStandarts[i].Key] = currentCoeff;
            }

            return result;
        }

        public Dictionary<TStandart, double> DictionaryConverter<TStandart>(Dictionary<TStandart, (double, double)> val)
        {
            return val.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Item1);
        }

        public void AddToBestBag<TStandart, TGarbageData>(ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag, TGarbageData garbageDataItem, Dictionary<TStandart, double> bestOfOrderedStandarts)
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
