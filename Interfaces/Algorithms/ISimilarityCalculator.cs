using Abstractions.Interfaces;
using Algo.Facade;
using System.Collections.Concurrent;

namespace Algo.Interfaces.Algorithms
{
    public interface ISimilarityCalculator
    {

        (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<TGarbageData, Dictionary<TStandart,double>> mid, Dictionary<TGarbageData, Dictionary<TStandart, double>> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
