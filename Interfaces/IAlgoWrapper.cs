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
        (List<ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>>>>, ConcurrentDictionary<string, TStandart>) AlgoWrap<TStandart,TGarbageData>(HashSet<TStandart> standarts, HashSet<TGarbageData> garbageData)
            where TStandart : IStandart
            where TGarbageData : IGarbageData;
    }
}
