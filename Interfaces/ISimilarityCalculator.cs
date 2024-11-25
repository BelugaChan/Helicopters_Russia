using Abstractions.Interfaces;
using System.Collections.Concurrent;

namespace Algo.Interfaces
{
    public interface ISimilarityCalculator
    {  

        (Dictionary<(TGarbageData, TStandart), double> worst, Dictionary<(TGarbageData, TStandart), double> mid, Dictionary<(TGarbageData, TStandart), double> best)/*выходные параметры*/ CalculateCoefficent<TStandart, TGarbageData>
            (/*List<TStandart> standarts*/ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> standartDict,//входные параметры
            List<TGarbageData> garbageData)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
