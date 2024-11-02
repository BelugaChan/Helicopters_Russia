using Abstractions.Interfaces;

namespace Algo.Interfaces
{
    public interface ISimilarityCalculator
    {
        (HashSet<TGarbageData> worst, HashSet<TGarbageData> mid, HashSet<TGarbageData> best)/*выходные параметры*/ CalculateCoefficent<TStandart, TGarbageData>
            (List<TStandart> standarts,//входные параметры
            List<TGarbageData> garbageData)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
