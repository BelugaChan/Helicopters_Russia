using Abstractions.Interfaces;
using Algo.Facade;
using Algo.Interfaces.Algorithms;

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
