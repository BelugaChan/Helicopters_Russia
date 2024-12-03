using Abstractions.Interfaces;
using Algo.Models;
using System.Collections.Concurrent;

namespace Algo.Interfaces.Algorithms
{
    public interface ISimilarityCalculator
    {

        (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<(string, TGarbageData), ConcurrentDictionary<string, ConcurrentDictionary</*ConcurrentDictionary<string, int>*/string, TStandart>>>> data, ConcurrentDictionary<string, TStandart> standarts)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
