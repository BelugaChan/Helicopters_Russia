using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Handlers.ENS;
using Algo.Interfaces;
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
        public CosineSimAlgo(IENSHandler eNSHandler, Cosine cosine /*int shigleLength = 3*/)
        {
            //this.shigleLength = shigleLength;
            this.cosine = cosine;
            this.eNSHandler = eNSHandler;
        }
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>>>> data)
        {
            currentProgress = 0;
            Dictionary<(TGarbageData, TStandart?), double> worst = new();
            Dictionary<(TGarbageData, TStandart?), double> mid = new();
            Dictionary<(TGarbageData, TStandart?), double> best = new();


            ConcurrentDictionary<(TGarbageData, TStandart?), double> worstBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> midBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart?), double> bestBag = new();


            Parallel.ForEach(data, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                int commonElementsCount = 0;
                double similarityCoeff = -1;
                TStandart? bestStandart = default;
                var garbageDataItem = item.Keys.FirstOrDefault();
                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageDataItem.ShortName);
                var tokens = baseProcessedGarbageName.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray();
                HashSet<int> tokenSet = new HashSet<int>(tokens);

                var standartStuff = item.Values; //сопоставленные группы эталонов для грязной позиции по ГОСТам
                foreach (var standartGroups in standartStuff) //сравнение грязной строки со всеми позициями каждой из групп, где хотя бы в одном из элементов совпал гост с грязной позицией
                {
                    if (standartGroups.Count == 0)//null reference!!!!
                    {
                        worstBag.TryAdd((garbageDataItem, bestStandart), 0);
                        break;
                    }
                    string improvedProcessedGarbageName = "";
                    var groupClassificationName = standartGroups.Keys.FirstOrDefault();
                    //switch-case
                    if (groupClassificationName.Contains("Калиброванные круги, шестигранники, квадраты"))
                    {
                        CalsibCirclesHandler sMTHHandler = (CalsibCirclesHandler)eNSHandler;
                        improvedProcessedGarbageName = sMTHHandler.AdditionalStringHandle(baseProcessedGarbageName);
                    }//другие обработчики   
                    else
                    {
                        improvedProcessedGarbageName = baseProcessedGarbageName;
                    }
                              
                    var garbageProfile = cosine.GetProfile(improvedProcessedGarbageName);

                    foreach (var standart in standartGroups.Values) //стандарты в каждой отдельной группе
                    {
                        var similarity = cosine.Similarity(garbageProfile, standart.Keys.FirstOrDefault());
                        if (similarity > similarityCoeff)
                        {
                            similarityCoeff = similarity;
                            bestStandart = standart.Values.FirstOrDefault();
                            var standartTokens = baseProcessedGarbageName.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray();
                            HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                            commonElementsCount = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                        }
                        else if (similarity == similarityCoeff)
                        {
                            var standartTokens = baseProcessedGarbageName.Split().Where(s => int.TryParse(s, out _)).Select(int.Parse).ToArray();
                            HashSet<int> standartTokenSet = new HashSet<int>(standartTokens);
                            int commonElementsCountNow = standartTokenSet.Where(tokenSet.Contains).ToArray().Length;
                            if (commonElementsCountNow > commonElementsCount)
                            {
                                bestStandart = standart.Values.FirstOrDefault();
                            }
                        }
                    }
                }
                //в итоговый словарь добавляем только лучшее сопоставление из всех предложенных групп (может быть изменено. К примеру, брать лучшие позиции для каждой из групп)
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
                    Console.WriteLine($"текущий прогресс: {currentProgress} | наилучшее сопоставление (> 0.7), ед.: {bestBag.Count} | среднее сопоставление, ед: {midBag.Count}");
                }
                currentProgress = Interlocked.Increment(ref currentProgress);
            });


            foreach (var ((item, bestStandart), bestValue) in worstBag)
            {
                worst.Add((item, bestStandart), bestValue);
            }
            foreach (var ((item, bestStandart), bestValue) in midBag)
            {
                mid.Add((item, bestStandart), bestValue);
            }
            foreach (var ((item, bestStandart), bestValue) in bestBag)
            {
                best.Add((item, bestStandart), bestValue);
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
