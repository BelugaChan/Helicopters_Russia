using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Handlers.ENS;
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
        private IENSHandler eNSHandler;
        public CosineSimAlgo(IENSHandler eNSHandler, Cosine cosine /*int shigleLength = 3*/)
        {
            //this.shigleLength = shigleLength;
            this.cosine = cosine;
            this.eNSHandler = eNSHandler;
        }
        public override (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (ConcurrentDictionary<GarbageData, ConcurrentDictionary<string, List<Standart>>> data)
        {
            Parallel.ForEach(data, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (item, state) =>
            {
                var garbageItem = item.Key;
                string baseProcessedGarbageName = eNSHandler.BaseStringHandle(garbageItem.ShortName);//базовая обработка наименования для каждой грязной позиции
                var standarts = item.Value;//класс стандартов - ключ словаря, значения - все стандарты данной группы. У одной грязной позиции может быть сопоставление с несколькими группами эталонов (в теории)
                string improvedProcessedGarbageName = "";
                if (standarts.Keys.Contains("Ленты, широкополосный прокат"))
                {
                    SMTHHandler smthHandler = (SMTHHandler)eNSHandler;
                    improvedProcessedGarbageName = smthHandler.AdditionalStringHandle(baseProcessedGarbageName);//специфическая обработка строки грязных данных в зависимости от класса позиции
                }
                //else if(standarts.Keys.Contains("класс")) { }
                foreach (var standart in standarts.Values)
                {

                }
            });
            return (null,null,null);
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
