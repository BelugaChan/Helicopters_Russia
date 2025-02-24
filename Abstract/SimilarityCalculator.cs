using Helicopters_Russia.Facade;
using Helicopters_Russia.Interfaces.Algorithms;
using AbstractionsAndModels.Interfaces.Models;

namespace Helicopters_Russia.Abstract
{
    public abstract class SimilarityCalculator : ISimilarityCalculator
    {
        protected int totalGarbageDataItems = 0;

        protected int currentProgress = 0;

        public abstract (List<TGarbageData> worst, Dictionary<TGarbageData, (Dictionary<TStandart, double>, string)> mid, Dictionary<TGarbageData, Dictionary<TStandart, double>> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

    }
}
