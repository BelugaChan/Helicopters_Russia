using Abstractions.Interfaces;
using Algo.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces
{
    public interface IAlgoWrapper
    {
        List<ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>>>> AlgoWrap<TStandart,TGarbageData>(List<TStandart> standarts, List<TGarbageData> garbageData)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;
    }
}
