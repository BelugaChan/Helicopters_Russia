﻿using Algo.Abstract;
using Algo.Facade;
using Algo.Interfaces.Handlers.ENS;
using Algo.Registry;
using F23.StringSimilarity;
using System.Collections.Concurrent;

namespace Algo.Algotithms
{
    public class CosineSimAlgo : SimilarityCalculator
    {
        //private int shigleLength;
        private Cosine cosine;
        private IENSHandler eNSHandler;
        private ENSHandlerRegistry handlerRegistry;
        public CosineSimAlgo
            (IENSHandler eNSHandler, 
            ENSHandlerRegistry handlerRegistry,
            Cosine cosine)
        {
            this.cosine = cosine;
            this.eNSHandler = eNSHandler;
            this.handlerRegistry = handlerRegistry;
        }
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
        {
            currentProgress = 0;          
            Dictionary<(TGarbageData, TStandart?), double> worst = new();
            Dictionary<(TGarbageData, TStandart?), double> mid = new();
            Dictionary<(TGarbageData, TStandart?), double> best = new();

            ConcurrentBag<(TGarbageData, string, HashSet<string>)> dataForPostProcessing = new();

            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> midBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> bestBag = new();

            var matchedData = algoResult.MatchedData;
            var processedStandarts = algoResult.ProcessedStandards;
            var unmatchedGarbageData = algoResult.UnmatchedGarbageData;

            Parallel.ForEach(matchedData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                TStandart? bestStandart = default;
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
                    var gostTokens = gost.Split(new char[] {' ', '-'}).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
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
                        if (similarity > similarityCoeff)
                        {
                            similarityCoeff = similarity;
                            bestStandart = standart.Key;
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
                            //HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                            commonElementsCount = standartTokens.Where(tokens.Contains).ToArray().Length;
                        }
                        else if (similarity == similarityCoeff)
                        {
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
                            //HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                            int commonElementsCountNow = standartTokens.Where(tokens.Contains).ToArray().Length;
                            if (commonElementsCountNow > commonElementsCount)
                            {
                                bestStandart = standart.Key;
                            }
                        }

                    }
                }
                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.05)
                {
                    dataForPostProcessing.Add((garbageDataItem, improvedProcessedGarbageName,garbageDataGosts));
                    //worstBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }
                else if (similarityCoeff < 0.6)
                    midBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                else
                    bestBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                if (currentProgress % 100 == 0)
                {
                    Console.WriteLine($"текущий прогресс: {currentProgress} | наилучшее сопоставление (> 0.7), ед.: {bestBag.Count} | среднее сопоставление, ед: {midBag.Count}");
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
                TStandart? bestStandart = default;
                double similarityCoeff = -1;
                foreach (var (standart, standartName) in allProcessedStandarts)
                {
                    double coeff = cosine.Similarity(garbageName, standartName);
                    if (coeff > similarityCoeff)
                    {
                        similarityCoeff = coeff;
                        bestStandart = standart;
                    }
                }
                if ((gosts.Count == 0 || gosts.All(t => t.Length == 0)) && similarityCoeff > 0.05)//если у позиции отсутствует ГОСТ, то она переносится в коллекцию требует уточнения
                {
                    midBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }
                else
                {
                    if (similarityCoeff < 0.05)
                    {
                        worstBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                    }
                    else if (similarityCoeff < 0.6)
                        midBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                    else
                        bestBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }               
                if (currentProgress % 10 == 0)
                {
                    Console.WriteLine($"Additional Checkin' текущий прогресс: {Math.Round((double)currentProgress / dataForPostProcessing.Count,2) * 100}%");
                }
                currentProgress = Interlocked.Increment(ref currentProgress);
            });



            foreach (var ((item, standart), bestValue) in worstBag)
            {
                worst.Add((item, standart), bestValue);
            }
            foreach (var ((item, standart), bestValue) in midBag)
            {
                mid.Add((item, standart), bestValue);
            }
            foreach (var ((item, standart), bestValue) in bestBag)
            {
                best.Add((item, standart), bestValue);
            }
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
