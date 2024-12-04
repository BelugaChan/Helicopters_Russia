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
        private IGostRemove gostRemove;
        public CosineSimAlgo
            (IENSHandler eNSHandler, 
            IAdditionalENSHandler<LumberHandler> lumberHandler, 
            IAdditionalENSHandler<CalsibCirclesHandler> calsibCirclesHandler, 
            IAdditionalENSHandler<RopesAndCablesHandler> ropesAndCablesHandler,
            IAdditionalENSHandler<MountingWiresHandler> mountingWiresHandler,
            IGostRemove gostRemove,
            Cosine cosine)
        {
            this.cosine = cosine;
            this.eNSHandler = eNSHandler;
            this.lumberHandler = lumberHandler;
            this.calsibCirclesHandler = calsibCirclesHandler;
            this.ropesAndCablesHandler = ropesAndCablesHandler;
            this.mountingWiresHandler = mountingWiresHandler;
            this.gostRemove = gostRemove;
        }
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<(string, TGarbageData), ConcurrentDictionary<string, ConcurrentDictionary</*ConcurrentDictionary<string, int>*/string, TStandart>>>> data, ConcurrentDictionary<string, TStandart> standarts)
        {
            currentProgress = 0;          
            Dictionary<(TGarbageData, TStandart?), double> worst = new();
            Dictionary<(TGarbageData, TStandart?), double> mid = new();
            Dictionary<(TGarbageData, TStandart?), double> best = new();

            ConcurrentDictionary<string, TGarbageData> dataForPostProcessing = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> midBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> bestBag = new();

            Parallel.ForEach(data, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                TStandart? bestStandart = default;
                int commonElementsCount = 0;
                double similarityCoeff = -1;
                
                var (garbageDataHandeledName, garbageDataItem) = item.Keys.FirstOrDefault();
                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataHandeledName/*garbageDataItem.ShortName*/);
                var tokens = baseProcessedGarbageName.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray();
                HashSet<int> tokenSet = new HashSet<int>(tokens);
                string improvedProcessedGarbageName = "";
                var standartStuff = item.Values; //сопоставленные группы эталонов для грязной позиции по ГОСТам
                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией
                {
                    if (standartGroups.Count == 0)//null reference!!!!
                    {
                        dataForPostProcessing.TryAdd(baseProcessedGarbageName, garbageDataItem);
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
                            var similarity = cosine.Similarity(improvedProcessedGarbageName/*garbageProfile*/, standartItem.Key);
                            if (similarity > similarityCoeff)
                            {
                                similarityCoeff = similarity;
                                bestStandart = standartItem.Value;
                                var standartTokens = standartItem.Key.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray();
                                HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                                commonElementsCount = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                            }
                            else if (similarity == similarityCoeff)
                            {
                                var standartTokens = standartItem.Key.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray();
                                HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                                int commonElementsCountNow = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                                if (commonElementsCountNow > commonElementsCount)
                                {
                                    bestStandart = standartItem.Value;
                                }
                            }
                        }
                        
                    }
                }
                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
                if (similarityCoeff < 0.05)
                {
                    dataForPostProcessing.TryAdd(improvedProcessedGarbageName, garbageDataItem);
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
            //дополнительный прогон по позициям с наихудшим сопоставлением
            Parallel.ForEach(dataForPostProcessing, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                TStandart? bestStandart = default;
                double similarityCoeff = -1;
                var garbageName = item.Key;
                var garbageDataItem = item.Value;
                foreach (var (standartName,standart) in standarts)
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
