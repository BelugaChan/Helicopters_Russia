using Abstractions.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces
{
    public interface IStandartHandle
    {
        ConcurrentBag<TStandart> HandleStandartNames<TStandart>(List<TStandart> standarts, IUpdatedEntityFactory<TStandart> factory)
        where TStandart : IStandart;

        ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>> HandleStandarts<TStandart>(Dictionary<string, List<TStandart>> standarts)
            where TStandart : IStandart;

        Dictionary<string, List<TStandart>> GroupingStandartsByENS<TStandart>(ConcurrentBag<TStandart> standarts)
            where TStandart : IStandart;//new feature

        ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>> FindStandartsWhichComparesWithGosts<TStandart>(List<string> gosts, ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>> standarts)
            where TStandart : IStandart;
    }
}
