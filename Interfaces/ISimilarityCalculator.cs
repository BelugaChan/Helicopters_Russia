using Abstractions.Interfaces;
using Algo.Models;
using System.Collections.Concurrent;

namespace Algo.Interfaces
{
    public interface ISimilarityCalculator
    {

        (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>>>> data)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
