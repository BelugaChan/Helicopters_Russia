using Abstractions.Interfaces;

namespace Algo.Interfaces
{
    public interface ISimilarityCalculator
    {
        void CalculateCoefficent<TStandart, TGarbageData>(List<TStandart> standarts, List<TGarbageData> garbageData,
            out HashSet<TGarbageData> worst, out HashSet<TGarbageData> mid, out HashSet<TGarbageData> best)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;

        double GetProgress();
    }
}
