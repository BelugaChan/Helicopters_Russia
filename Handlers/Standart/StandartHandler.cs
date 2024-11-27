using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Interfaces;
using Algo.Models;
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

        public Dictionary<string, List<TStandart>> GroupingStandartsByENS<TStandart>(ConcurrentBag<TStandart> standarts) //new feature
            where TStandart : IStandart
        {
            var res = standarts.GroupBy(e => e.ENSClassification).ToDictionary(group => group.Key, group => group.ToList());
            return res;
        }

        public ConcurrentBag<TStandart> HandleStandartNames<TStandart>(List<TStandart> standarts, IUpdatedEntityFactory<TStandart> factory)
            where TStandart : IStandart
        {
            int currentProgress = 0;
            var fixedStandarts = new ConcurrentBag<TStandart>();
            Parallel.ForEach(standarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (standartItem, state) =>
            {
                fixedStandarts.Add(factory.CreateUpdatedEntity(standartItem.Id, standartItem.Code, eNSHandler.BaseStringHandle(standartItem.Name), standartItem.NTD, standartItem.MaterialNTD, standartItem.ENSClassification));
                currentProgress = Interlocked.Increment(ref currentProgress);
                if (currentProgress % 10 == 0)
                {
                    Console.WriteLine($"HandleStandartNames: {Math.Round((double)currentProgress / standarts.Count * 100,2)}");
                }
            });
            return fixedStandarts;
        }

        public ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>> HandleStandarts<TStandart>(Dictionary<string, List<TStandart>> standarts)
            where TStandart : IStandart
        {
            var standartDict = new ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>>();
            int currentProgress = 0;
            Parallel.ForEach(standarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (standartItem, state) =>
            {
                var midDict = new ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>();
                foreach (var item in standartItem.Value)
                {
                    var internalDict = new ConcurrentDictionary<string, int>(cosine.GetProfile(item.Name).ToDictionary()); //один обработанный эталон                    
                    midDict.TryAdd(internalDict, item);
                }
                standartDict.TryAdd(standartItem.Key, midDict);
                currentProgress = Interlocked.Increment(ref currentProgress);
                Console.WriteLine($"HandleStandarts: {Math.Round((double)currentProgress / standarts.Count * 100, 2)}");
            });
            return standartDict;
            /*внешний словарь.
             * Ключ - класс, по которым сгруппированы эталоны
             * Значение - словарь словарей эталонов
             *срединный словарь.
             * Ключ - словарь с обработанными словами и частотами встречаемости? (чекнуть библиотеку). Значение - экземпляр класса Standart (добавлен для удобства)
             */
        }

        public ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>> FindStandartsWhichComparesWithGosts<TStandart>(List<string> gosts, ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>> standarts)
            where TStandart : IStandart
        {
            var filteredData = standarts
                .Where(category => category.Value
                    .Any(subCategory => gosts
                        .Any(gostItem => subCategory.Value.NTD.Contains(gostItem) || subCategory.Value.MaterialNTD.Contains(gostItem))));

            return new ConcurrentDictionary<string, ConcurrentDictionary<ConcurrentDictionary<string, int>, TStandart>>(filteredData);

        }
    }
}
