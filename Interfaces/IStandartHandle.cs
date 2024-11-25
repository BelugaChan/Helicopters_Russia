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
        List<TStandart> HandleStandartNames<TStandart>(List<TStandart> standarts, IUpdatedEntityFactory<TStandart> factory)
        where TStandart : IStandart;

        ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> HandleStandarts<TStandart>(List<TStandart> standarts)
            where TStandart : IStandart;

        Dictionary<string, List<TStandart>> GroupingStandartsByENS<TStandart>(List<TStandart> standarts)
            where TStandart : IStandart;//new feature

        Dictionary<string, List<TStandart>> FindStandartsWhichComparesWithGosts<TStandart>(List<string> gosts, Dictionary<string, List<TStandart>> standarts)
            where TStandart : IStandart;
    }
}
