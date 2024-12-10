using Abstractions.Interfaces;
using Algo.Models;
using System.Collections.Concurrent;

namespace Algo.Interfaces.Algorithms
{
    public interface ISimilarityCalculator
    {

        (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best) CalculateCoefficent<TStandart, TGarbageData>
            (List<ConcurrentDictionary<(string, TGarbageData, HashSet<string>), ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>>> data, ConcurrentDictionary<TStandart, string> standarts, ConcurrentBag<(TGarbageData, HashSet<string>)> garbageDataWithoutComparedStandarts)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
