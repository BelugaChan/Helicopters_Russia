using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Interfaces;
using Algo.Models;
using F23.StringSimilarity;
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
        private IENSHandler enshandler;
        public CosineSimAlgo(IENSHandler eNSHandler, Cosine cosine /*int shigleLength = 3*/)
        {
            //this.shigleLength = shigleLength;
            this.cosine = cosine;
            this.enshandler = eNSHandler;
        }

        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (/*List<TStandart> standarts*/ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> standartDict, 
            List<TGarbageData> garbageData)
        {
            currentProgress = 0;
            totalGarbageDataItems = garbageData.Count;//инициализация, необходимая для получения процента выполнения алгоритма.
            Dictionary<(TGarbageData, TStandart), double> worst = new();
            Dictionary<(TGarbageData, TStandart), double> mid = new();
            Dictionary<(TGarbageData, TStandart), double> best = new();
            //var standartDict = new ConcurrentDictionary<ConcurrentDictionary<string, int>,TStandart>();

            //Parallel.ForEach(standarts, parallelOptions, (standartItem, state) =>
            //{
            //    standartDict.TryAdd(new ConcurrentDictionary<string, int>(cosine.GetProfile(StringHandler(standartItem.Name))),standartItem);
            //});

            ConcurrentDictionary<(TGarbageData, TStandart), double> worstBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart), double> midBag = new();
            ConcurrentDictionary<(TGarbageData, TStandart), double> bestBag = new();
            //int processedItems = 0;           
            Parallel.ForEach(garbageData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                double bestValue = -1;
                TStandart bestStandart = standartDict.FirstOrDefault().Value;
                var profileGarbge = cosine.GetProfile(enshandler.StringHandler(item.ShortName));
                foreach (var (profile, standart) in standartDict)
                {
                    var res = cosine.Similarity(profile, profileGarbge);
                    if (res > bestValue)
                    {
                        bestValue = res;
                        bestStandart = standart;
                    }                        
                    if (bestValue == 1)
                        break;
                }

                if (bestValue < 0.1)
                {
                    worstBag.TryAdd((item, bestStandart), Math.Round(bestValue,3));
                }
                else if (bestValue < 0.7)
                    midBag.TryAdd((item, bestStandart), Math.Round(bestValue, 3));
                else
                    bestBag.TryAdd((item, bestStandart), Math.Round(bestValue, 3));
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
    }
}
