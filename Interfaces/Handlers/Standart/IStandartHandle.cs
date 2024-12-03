using Abstractions.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.Handlers.Standart
{
    public interface IStandartHandle<TStandart>
        where TStandart : IStandart
    {
        public ConcurrentDictionary<string, TStandart> HandleStandartNames(HashSet<TStandart> standarts);

        //ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>> HandleStandarts<TStandart>(Dictionary<string, List<TStandart>> standarts)
        //    where TStandart : IStandart;

        ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>> GroupingStandartsByENS(ConcurrentDictionary<string, TStandart> standarts);

        ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>> FindStandartsWhichComparesWithGosts(HashSet<string> gosts, ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>> standarts);

    }
}
