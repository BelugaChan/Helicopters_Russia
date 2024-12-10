using Abstractions.Interfaces;
using Algo.Interfaces.Algorithms;
using Algo.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Abstract
{
    public abstract class SimilarityCalculator : ISimilarityCalculator
    {
        protected int totalGarbageDataItems = 0;

        protected int currentProgress = 0;

        public abstract (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<(string, TGarbageData, HashSet<string>), ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>>> data, ConcurrentDictionary<TStandart, string> standarts, ConcurrentBag<(TGarbageData, HashSet<string>)> garbageDataWithoutComparedStandarts)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        public double GetProgress() 
        {
            return (double)currentProgress * 100 / totalGarbageDataItems;
        }
    }
}
