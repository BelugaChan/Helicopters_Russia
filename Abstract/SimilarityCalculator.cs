using Abstractions.Interfaces;
using Algo.Facade;
using Algo.Interfaces.Algorithms;
using Algo.Interfaces.Handlers.ENS;
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

        public abstract (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<TGarbageData, (Dictionary<TStandart, double>, string)> mid, Dictionary<TGarbageData, Dictionary<TStandart, double>> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

    }
}
