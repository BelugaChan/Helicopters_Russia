using Abstractions.Interfaces;
using System.Collections.Concurrent;

namespace Algo.Interfaces.Handlers.Standart
{
    public interface IStandartHandle<TStandart>
        where TStandart : IStandart
    {
        public ConcurrentDictionary<TStandart, string> HandleStandartNames(HashSet<TStandart> standarts);


        ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> GroupingStandartsByENS(ConcurrentDictionary<TStandart, string> standarts);

        ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> FindStandartsWhichComparesWithGosts(HashSet<string> gosts, ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> standarts);

    }
}
