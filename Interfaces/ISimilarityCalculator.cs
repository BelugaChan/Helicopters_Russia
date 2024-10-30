using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinHash.Interfaces
{
    public interface ISimilarityCalculator
    {
        void CalculateCoefficent<TStandart, TGarbageData>(List<TStandart> standarts, List<TGarbageData> garbageData)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;
    }
}
