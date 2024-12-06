using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Handlers.ENS;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.Handlers.GOST;
using Algo.Models;
using F23.StringSimilarity;
using NPOI.SS.Formula.Functions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Algotithms
{
    public class CosineSimAlgo : SimilarityCalculator
    {
        //private int shigleLength;
        private Cosine cosine;
        private IENSHandler eNSHandler;
        private IAdditionalENSHandler<LumberHandler> lumberHandler;
        private IAdditionalENSHandler<CalsibCirclesHandler> calsibCirclesHandler;
        private IAdditionalENSHandler<RopesAndCablesHandler> ropesAndCablesHandler;
        private IAdditionalENSHandler<MountingWiresHandler> mountingWiresHandler;
        private IAdditionalENSHandler<WireHandler> wireHandler;
        public CosineSimAlgo
            (IENSHandler eNSHandler, 
            IAdditionalENSHandler<LumberHandler> lumberHandler, 
            IAdditionalENSHandler<CalsibCirclesHandler> calsibCirclesHandler, 
            IAdditionalENSHandler<RopesAndCablesHandler> ropesAndCablesHandler,
            IAdditionalENSHandler<MountingWiresHandler> mountingWiresHandler,
            IAdditionalENSHandler<WireHandler> wireHandler,
            Cosine cosine)
        {
            this.cosine = cosine;
            this.eNSHandler = eNSHandler;
            this.lumberHandler = lumberHandler;
            this.calsibCirclesHandler = calsibCirclesHandler;
            this.ropesAndCablesHandler = ropesAndCablesHandler;
            this.mountingWiresHandler = mountingWiresHandler;
            this.wireHandler = wireHandler;
        }
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<(string, TGarbageData, HashSet<string>), ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>>> data, ConcurrentDictionary<TStandart, string> standarts, ConcurrentBag<TGarbageData> garbageDataWithoutComparedStandarts)
        {
            currentProgress = 0;          
            Dictionary<(TGarbageData, TStandart?), double> worst = new();
            Dictionary<(TGarbageData, TStandart?), double> mid = new();
            Dictionary<(TGarbageData, TStandart?), double> best = new();

            ConcurrentDictionary<TGarbageData, string> dataForPostProcessing = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> midBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> bestBag = new();
           
            Parallel.ForEach(data, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                TStandart? bestStandart = default;
                int commonElementsCount = 0;
                double similarityCoeff = -1;
                
                var (garbageDataHandeledName, garbageDataItem, garbageDateGosts) = item.Keys.FirstOrDefault();
                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataHandeledName/*garbageDataItem.ShortName*/);
                var tokens = baseProcessedGarbageName.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                foreach (var gost in garbageDateGosts)
                {
                    var gostTokens = gost.Split(new char[] {' ', '-'}).Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                    foreach (var gostToken in gostTokens)
                    {
                        tokens.Add(gostToken); //добавление в список токенов всех чисел из ГОСТов, найденных для данной грязной позиции.
                    }
                }
                HashSet<int> tokenSet = new HashSet<int>(tokens);
                string improvedProcessedGarbageName = "";
                var standartStuff = item.Values; //сопоставленные группы эталонов для грязной позиции по ГОСТам
                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией
                {
                    if (standartGroups.Count == 0)//null reference!!!!
                    {
                        dataForPostProcessing.TryAdd(garbageDataItem, baseProcessedGarbageName);
                        //worstBag.TryAdd((garbageDataItem, bestStandart), 0);
                        break;
                    }

                    var groupClassificationName = standartGroups.Keys.FirstOrDefault();
                    //персональные обработчики для классификаторов ЕНС
                    switch (groupClassificationName)
                    {
                        case string name when name.Contains("Круги, шестигранники, квадраты") ||
                                              name.Contains("Калиброванные круги, шестигранники, квадраты"):
                            {
                                improvedProcessedGarbageName = calsibCirclesHandler.AdditionalStringHandle(baseProcessedGarbageName);
                                break;
                            }
                        case string name when name.Contains("Пиломатериалы"):
                            {       
                                improvedProcessedGarbageName = lumberHandler.AdditionalStringHandle(baseProcessedGarbageName);
                                break;
                            }
                        case string name when name.Contains("Канаты, Тросы"):
                            {
                                improvedProcessedGarbageName = ropesAndCablesHandler.AdditionalStringHandle(baseProcessedGarbageName);
                                break;
                            }
                        case string name when name.Contains("Провода монтажные"):
                            {
                                improvedProcessedGarbageName = mountingWiresHandler.AdditionalStringHandle(baseProcessedGarbageName);
                                break;
                            }
                        case string name when name.Contains("Проволока"):
                            {
                                improvedProcessedGarbageName = wireHandler.AdditionalStringHandle(baseProcessedGarbageName);
                                break;
                            }
                        default:
                            {
                                improvedProcessedGarbageName = baseProcessedGarbageName;
                                break;
                            }
                    } 
                    foreach (var standart in standartGroups.Values) //стандарты в каждой отдельной группе
                    {
                        foreach (var standartItem in standart)
                        {
                            var similarity = cosine.Similarity(improvedProcessedGarbageName/*garbageProfile*/, standartItem.Value);
                            if (similarity > similarityCoeff)
                            {
                                similarityCoeff = similarity;
                                bestStandart = standartItem.Key;
                                var standartTokens = standartItem.Value.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                var standartGosts = new HashSet<string>() {standartItem.Key.MaterialNTD, standartItem.Key.NTD };
                                foreach (var handledGost in standartGosts)
                                {
                                    var handledGostTokens = handledGost.Split(new char[] {' ', '-'}).Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                    foreach (var handledToken in handledGostTokens)
                                    {
                                        standartTokens.Add(handledToken);
                                    }
                                }
                                HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                                commonElementsCount = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                            }
                            else if (similarity == similarityCoeff)
                            {
                                var standartTokens = standartItem.Value.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                var standartGosts = new HashSet<string>() { standartItem.Key.MaterialNTD, standartItem.Key.NTD };
                                foreach (var handledGost in standartGosts)
                                {
                                    var handledGostTokens = handledGost.Split(new char[] { ' ', '-' }).Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
                                    foreach (var handledToken in handledGostTokens)
                                    {
                                        standartTokens.Add(handledToken);
                                    }
                                }
                                HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                                int commonElementsCountNow = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                                if (commonElementsCountNow > commonElementsCount)
                                {
                                    bestStandart = standartItem.Key;
                                }
                            }
                        }
                        
                    }
                }
                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.05)
                {
                    dataForPostProcessing.TryAdd(garbageDataItem, improvedProcessedGarbageName);
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
            Parallel.ForEach(garbageDataWithoutComparedStandarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                dataForPostProcessing.TryAdd(item, eNSHandler.BaseStringHandle(item.ShortName));
            });
            //дополнительный прогон по позициям с для которых не были найдены подходящие стандарты
            Parallel.ForEach(dataForPostProcessing, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                TStandart? bestStandart = default;
                double similarityCoeff = -1;
                var garbageName = item.Value;
                var garbageDataItem = item.Key;
                foreach (var (standart, standartName) in standarts)
                {
                    double coeff = cosine.Similarity(garbageName, standartName);
                    if (coeff > similarityCoeff)
                    {
                        similarityCoeff = coeff;
                        bestStandart = standart;
                    }
                }

                if (similarityCoeff < 0.05)
                {
                    worstBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                }
                else if (similarityCoeff < 0.6)
                    midBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                else
                    bestBag.TryAdd((garbageDataItem, bestStandart), Math.Round(similarityCoeff, 3));
                if (currentProgress % 100 == 0)
                {
                    Console.WriteLine($"Additional Checkin' текущий прогресс: {Math.Round((double)currentProgress / dataForPostProcessing.Count,2)}%");
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
        //List<ConcurrentDictionary<GarbageData, ConcurrentDictionary<string,List<Standart>>>>

        //public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
        //    (ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> standartDict, 
        //    List<TGarbageData> garbageData)
        //{
        //    currentProgress = 0;
        //    totalGarbageDataItems = garbageData.Count;//инициализация, необходимая для получения процента выполнения алгоритма.
        //    Dictionary<(TGarbageData, TStandart), double> worst = new();
        //    Dictionary<(TGarbageData, TStandart), double> mid = new();
        //    Dictionary<(TGarbageData, TStandart), double> best = new();

        //    ConcurrentDictionary<(TGarbageData, TStandart), double> worstBag = new();
        //    ConcurrentDictionary<(TGarbageData, TStandart), double> midBag = new();
        //    ConcurrentDictionary<(TGarbageData, TStandart), double> bestBag = new();        
        //    Parallel.ForEach(garbageData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
        //    {
        //        double bestValue = -1;
        //        TStandart bestStandart = standartDict.FirstOrDefault().Value;
        //        var profileGarbge = cosine.GetProfile(enshandler.StringHandler(item.ShortName));
        //        foreach (var (profile, standart) in standartDict)
        //        {
        //            var res = cosine.Similarity(profile, profileGarbge);
        //            if (res > bestValue)
        //            {
        //                bestValue = res;
        //                bestStandart = standart;
        //            }                        
        //            if (bestValue == 1)
        //                break;
        //        }

        //        if (bestValue < 0.1)
        //        {
        //            worstBag.TryAdd((item, bestStandart), Math.Round(bestValue,3));
        //        }
        //        else if (bestValue < 0.7)
        //            midBag.TryAdd((item, bestStandart), Math.Round(bestValue, 3));
        //        else
        //            bestBag.TryAdd((item, bestStandart), Math.Round(bestValue, 3));
        //        if (currentProgress % 100 == 0)
        //        {
        //            Console.WriteLine($"текущий прогресс: {currentProgress} | наилучшее сопоставление (> 0.7), ед.: {bestBag.Count} | среднее сопоставление, ед: {midBag.Count}");
        //        }
        //        currentProgress = Interlocked.Increment(ref currentProgress);
        //    });
        //    foreach (var ((item, bestStandart), bestValue) in worstBag)
        //    {
        //        worst.Add((item, bestStandart), bestValue);
        //    }
        //    foreach (var ((item, bestStandart), bestValue) in midBag)
        //    {
        //        mid.Add((item, bestStandart), bestValue);
        //    }
        //    foreach (var ((item, bestStandart), bestValue) in bestBag)
        //    {
        //        best.Add((item, bestStandart), bestValue);
        //    }
        //    return (worst, mid, best);

        //}
    }
}
