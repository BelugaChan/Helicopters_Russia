using AbstractionsAndModels.Interfaces.Models;
using Helicopters_Russia.Facade;

namespace Helicopters_Russia.Interfaces.Algorithms
{
    public interface ISimilarityCalculator
    {
        (List<TGarbageData> worst, Dictionary<TGarbageData, (Dictionary<TStandart, double>, string)> mid, Dictionary<TGarbageData, Dictionary<TStandart, double>> best) CalculateCoefficent<TStandart, TGarbageData>(AlgoResult<TStandart, TGarbageData> algoResult)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;
    }
}
