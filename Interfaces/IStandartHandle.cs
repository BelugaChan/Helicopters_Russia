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
        public ConcurrentDictionary<string, TStandart> HandleStandartNames<TStandart>(HashSet<TStandart> standarts)
        where TStandart : IStandart;

        //ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>> HandleStandarts<TStandart>(Dictionary<string, List<TStandart>> standarts)
        //    where TStandart : IStandart;

        ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>> GroupingStandartsByENS<TStandart>(ConcurrentDictionary<string, TStandart> standarts)
            where TStandart : IStandart;//new feature

        ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>> FindStandartsWhichComparesWithGosts<TStandart>(List<string> gosts, ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>> standarts)
            where TStandart : IStandart;
    }
}
