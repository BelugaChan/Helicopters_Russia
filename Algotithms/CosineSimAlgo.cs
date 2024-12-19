using Algo.Abstract;
using Algo.Facade;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Registry;
using F23.StringSimilarity;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Algo.Algotithms
{
    public class CosineSimAlgo : SimilarityCalculator
    {
        //private int shigleLength;       
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
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<TGarbageData, Dictionary<TStandart, double>> mid, Dictionary<TGarbageData, Dictionary<TStandart, double>> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
        {
            currentProgress = 0;          
            Dictionary<(TGarbageData, TStandart?), double> worst = new();
            Dictionary<TGarbageData, Dictionary<TStandart, double>> mid = new();
            Dictionary<TGarbageData, Dictionary<TStandart, double>> best = new();

            ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing = new();

            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag = new();
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> midBag = new();
            ConcurrentDictionary<TGarbageData, Dictionary<TStandart, double>> bestBag = new();

            var matchedData = algoResult.MatchedData;
            var processedStandarts = algoResult.ProcessedStandards;
            var unmatchedGarbageData = algoResult.UnmatchedGarbageData;

            Parallel.ForEach(matchedData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                ConcurrentDictionary<TStandart,double> bestStandart = new();
                int commonElementsCount = 0;
                double similarityCoeff = -1;

                var garbageItem = item.GarbageItem;
                var garbageDataHandeledName = garbageItem.ProcessedName;
                var garbageDataItem = garbageItem.Data;
                var garbageDataGosts = garbageItem.ProcessedGosts;

                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataHandeledName/*garbageDataItem.ShortName*/);
                var tokens = baseProcessedGarbageName.Split().Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                foreach (var gost in garbageDataGosts)
                {
                    var gostTokens = gost.Split(new char[] { ' ', '-' }).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                    foreach (var gostToken in gostTokens)
                    {
                        tokens.Add(gostToken); //добавление в список токенов всех чисел из ГОСТов, найденных для данной грязной позиции.
                    }
                }
                //HashSet<int> tokenSet = new HashSet<int>(tokens);
                string improvedProcessedGarbageName = "";
                var standartStuff = item.Matches; //сопоставленные группы эталонов для грязной позиции по ГОСТам
                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией
                {
                    var groupClassificationName = standartGroups.Key;
                    //персональные обработчики для классификаторов ЕНС
                    improvedProcessedGarbageName = SelectHandler(groupClassificationName, baseProcessedGarbageName);
                    foreach (var standart in standartGroups.Value) //стандарты в каждой отдельной группе
                    {
                        var similarity = cosine.Similarity(improvedProcessedGarbageName/*garbageProfile*/, standart.Value);

                        var standartTokens = standart.Value.Split().Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                        var standartGosts = new HashSet<string>() { standart.Key.MaterialNTD, standart.Key.NTD };
                        foreach (var handledGost in standartGosts)
                        {
                            var handledGostTokens = handledGost.Split(new char[] { ' ', '-' }).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                            foreach (var handledToken in handledGostTokens)
                            {
                                standartTokens.Add(handledToken);
                            }
                        }

                        if (similarity > similarityCoeff)
                        {
                            similarityCoeff = similarity;
                            bestStandart.TryAdd(standart.Key,similarity);
                            commonElementsCount = standartTokens.Where(tokens.Contains).ToArray().Length;
                        }
                        else if (similarity == similarityCoeff)
                        {
                            int commonElementsCountNow = standartTokens.Where(tokens.Contains).ToArray().Length;
                            if (commonElementsCountNow > commonElementsCount)
                            {
                                bestStandart.TryAdd(standart.Key, similarity);
                            }
                        }
                        else if (similarity - similarityCoeff < 0.1)
                        {
                            bestStandart.TryAdd(standart.Key, similarity);
                        }
                    }
                }
                bool addedFirst = false;
                var orderedStandarts = bestStandart.OrderByDescending(t => t.Value).Take(3);
                var a =
                orderedStandarts.Where((kvp, index) =>
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
                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.1)
                {
                    dataForPostProcessing.Add((garbageDataItem, improvedProcessedGarbageName, garbageDataGosts));
                    //worstBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }
                else if (similarityCoeff < 1)
                    midBag.TryAdd(garbageDataItem, a/*orderedStandarts*/);
                else
                {
                    var orderedStandart = a.FirstOrDefault();
                    if (Math.Round(orderedStandart.Value) == 1)
                    {                       
                        bestBag.TryAdd(garbageDataItem, new Dictionary<TStandart, double>() { {orderedStandart.Key,orderedStandart.Value } });
                    }
                    else
                    {
                        bestBag.TryAdd(garbageDataItem, a);
                    }
                }
                    
                if (currentProgress % 100 == 0)
                {
                    progressStrategy.UpdateProgress(new Models.Progress {Step = "3. Базовый прогон алгоритма Cosine", CurrentProgress=Math.Round((double)currentProgress / matchedData.Count * 100,2) });
                    progressStrategy.LogProgress();                    
                }
                currentProgress = Interlocked.Increment(ref currentProgress);
            });
            currentProgress = 0;
            
            Parallel.ForEach(unmatchedGarbageData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                var (garbageData, gosts) = item;
                dataForPostProcessing.Add((garbageData, eNSHandler.BaseStringHandle(garbageData.ShortName),gosts));
            });
            //дополнительный прогон по позициям с для которых не были найдены подходящие стандарты
            var allProcessedStandarts = new ConcurrentDictionary<TStandart, string>(processedStandarts.GroupStandarts.Values.SelectMany(innerDict => innerDict));
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
                    {
                        bestStandart.TryAdd(standart, coeff);
                    }
                }
                bool addedFirst = false;
                var orderedStandarts = bestStandart.OrderByDescending(t => t.Value).Take(3);
                var a =
                orderedStandarts.Where((kvp, index) =>
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
                if ((gosts.Count == 0 || gosts.All(t => t.Length == 0)) && similarityCoeff > 0.05)//если у позиции отсутствует ГОСТ, то она переносится в коллекцию требует уточнения
                {
                    midBag.TryAdd(garbageDataItem, a);
                }
                else
                {
                    var orderedStandart = orderedStandarts.FirstOrDefault();
                    if (similarityCoeff < 0.1)
                    {
                        worstBag.TryAdd((garbageDataItem, orderedStandart.Key), Math.Round(orderedStandart.Value, 3));
                    }
                    else if (similarityCoeff < 1)
                        midBag.TryAdd(garbageDataItem, a);
                    else
                    {
                        if (a.ContainsValue(1))
                        {
                            bestBag.TryAdd(garbageDataItem, new Dictionary<TStandart, double>() { { orderedStandart.Key, orderedStandart.Value } });
                        }
                        else
                        {
                            bestBag.TryAdd(garbageDataItem, a);
                        }
                    }
                }               
                if (currentProgress % 10 == 0)
                {
                    //Console.WriteLine($"Additional Checkin' текущий прогресс: {Math.Round((double)currentProgress / dataForPostProcessing.Count * 100, 2):f2}%");
                    progressStrategy.UpdateProgress(new Models.Progress { Step = "4. Дополнительный прогон алгоритма Cosine", CurrentProgress = Math.Round((double)currentProgress / dataForPostProcessing.Count * 100, 2)});
                    progressStrategy.LogProgress();
                }
                currentProgress = Interlocked.Increment(ref currentProgress);
            });



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
            progressStrategy.UpdateProgress(new Models.Progress { Step = "Алгоритм завершил свою работу. Ожидайте записи результатов обработки в файл", CurrentProgress = 100 });
            return (worst, mid, best);
        }


        public string SelectHandler(string groupClassificationName, string baseProcessedGarbageName)
        {
            var handler = handlerRegistry.GetHandler(groupClassificationName);
            if (handler != null)
            {
                return handler(baseProcessedGarbageName);
            }

            return baseProcessedGarbageName;
        }
       
    }
}
