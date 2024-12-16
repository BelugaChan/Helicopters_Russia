using Abstractions.Interfaces;
using Algo.Facade;
using Algo.Interfaces.Handlers.ENS;
using Algo.Models;
using System.Collections.Concurrent;

namespace Algo.Interfaces.Algorithms
{
    public interface ISimilarityCalculator
    {

        (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
