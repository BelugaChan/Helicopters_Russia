using Abstractions.Interfaces;
using Algo.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.Wrappers
{
    public interface IAlgoWrapper<TStandart,TGarbageData>
        where TStandart : IStandart
        where TGarbageData : IGarbageData
    {
        (List<ConcurrentDictionary<(string, TGarbageData), ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>>>>, ConcurrentDictionary<string, TStandart>) AlgoWrap(HashSet<TStandart> standarts, HashSet<TGarbageData> garbageData);
    }
}
