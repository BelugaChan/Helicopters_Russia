using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Interfaces;
using F23.StringSimilarity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.Standart
{
    public class StandartHandler : IStandartHandle
    {
        private IENSHandler eNSHandler;
        //private ParallelOptions parallelOptions;
        private Cosine cosine;
        public StandartHandler(IENSHandler eNSHandler, Cosine cosine)
        {
            this.eNSHandler = eNSHandler;
            this.cosine = cosine;
        }

        public Dictionary<string, List<TStandart>> GroupingStandartsByENS<TStandart>(List<TStandart> standarts) //new feature
            where TStandart : IStandart
        {
            var res = standarts.GroupBy(e => e.ENSClassification).ToDictionary(group => group.Key, group => group.ToList());
            return res;
        }

        public List<TStandart> HandleStandartNames<TStandart>(List<TStandart> standarts, IUpdatedEntityFactory<TStandart> factory)
            where TStandart : IStandart
        {
            var fixedStandarts = new List<TStandart>();
            foreach (var item in standarts)
            {
                fixedStandarts.Add(factory.CreateUpdatedEntity(item.Id, item.Code, eNSHandler.StringHandler(item.Name), item.NTD, item.MaterialNTD, item.ENSClassification));
            }
            return fixedStandarts;
        }

        public ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart> HandleStandarts<TStandart>(List<TStandart> standarts)
            where TStandart : IStandart
        {
            var standartDict = new ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>();
            Parallel.ForEach(standarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (standartItem, state) =>
            {
                standartDict.TryAdd(new ConcurrentDictionary<string, int>(cosine.GetProfile(standartItem.Name/*StringHandler()*/)), standartItem);
            });
            return standartDict;
        }

        public Dictionary<string, List<TStandart>> FindStandartsWhichComparesWithGosts<TStandart>(List<string> gosts, Dictionary<string, List<TStandart>> standarts)
            where TStandart : IStandart
        {
            var filteredData = standarts.Where(group => group.Value.Any(item => gosts.Contains(item.NTD) || gosts.Contains(item.MaterialNTD)))
                .ToDictionary(group => group.Key, group => group.Value);
            return filteredData;
        }
    }
}
