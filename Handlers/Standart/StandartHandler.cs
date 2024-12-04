using Abstractions.Interfaces;
using Algo.Abstract;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.Handlers.GOST;
using Algo.Interfaces.Handlers.Standart;
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
    public class StandartHandler<TStandart> : IStandartHandle<TStandart>
        where TStandart : IStandart
    {
        private IENSHandler eNSHandler;
        private IGostRemove gostRemove;
        private IUpdatedEntityFactoryStandart<TStandart> updatedEntityFactoryStandart;
        //private ParallelOptions parallelOptions;
        private Cosine cosine;
        public StandartHandler(IENSHandler eNSHandler,IGostRemove gostRemove,IUpdatedEntityFactoryStandart<TStandart> updatedEntityFactoryStandart, Cosine cosine)
        {
            this.eNSHandler = eNSHandler;
            this.gostRemove = gostRemove;
            this.updatedEntityFactoryStandart = updatedEntityFactoryStandart;
            this.cosine = cosine;
        }

        public ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>> GroupingStandartsByENS(ConcurrentDictionary<string, TStandart> standarts) //new feature
        {
            var res = standarts.GroupBy(e => e.Value.ENSClassification).ToDictionary(group => group.Key, group => new ConcurrentDictionary<string, TStandart>(group.ToDictionary(e => e.Key, e => e.Value)));
            return new ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>>(res);
        }

        public ConcurrentDictionary<string, TStandart> HandleStandartNames(HashSet<TStandart> standarts)
        {
            int currentProgress = 0;
            var fixedStandarts = new ConcurrentDictionary<string, TStandart>();
            Parallel.ForEach(standarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (standartItem, state) =>
            {
                //удаление гостов из эталона
                var gosts = new HashSet<string>() {standartItem.MaterialNTD, standartItem.NTD}
                .Where(item => !string.IsNullOrEmpty(item) && item.Length > 0).ToHashSet();
                var itemNameWithRemovedGosts = gostRemove.RemoveGosts(standartItem.Name, gosts);

                fixedStandarts.TryAdd(eNSHandler.BaseStringHandle(/*standartItem.Name*/itemNameWithRemovedGosts), 
                    updatedEntityFactoryStandart.CreateUpdatedEntity(
                        standartItem.Id,
                        standartItem.Code,
                        standartItem.Name,
                        standartItem.NTD.Replace(" ", ""),
                        standartItem.MaterialNTD.Replace(" ", ""),
                        standartItem.ENSClassification)
                    );
                currentProgress = Interlocked.Increment(ref currentProgress);
                if (currentProgress % 10 == 0)
                {
                    Console.WriteLine($"HandleStandartNames: {Math.Round((double)currentProgress / standarts.Count * 100,2)}");
                }
            });
            return fixedStandarts;
        }

        //public ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>> HandleStandarts<TStandart>(ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>> standarts)
        //    where TStandart : IStandart
        //{
        //    var standartDict = new ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>>();
        //    int currentProgress = 0;
        //    Parallel.ForEach(standarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (standartItem, state) =>
        //    {
        //        var midDict = new ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>();
        //        foreach (var item in standartItem.Value)
        //        {
        //            //var internalDict = new ConcurrentDictionary<string, int>(cosine.GetProfile(item.Name).ToDictionary()); //один обработанный эталон                    
        //            midDict.TryAdd(/*internalDict*/item.Name, item);
        //        }
        //        standartDict.TryAdd(standartItem.Key, midDict);
        //        currentProgress = Interlocked.Increment(ref currentProgress);
        //        Console.WriteLine($"HandleStandarts: {Math.Round((double)currentProgress / standarts.Count * 100, 2)}");
        //    });
        //    return standartDict;
        //    /*внешний словарь.
        //     * Ключ - класс, по которым сгруппированы эталоны
        //     * Значение - словарь словарей эталонов
        //     *срединный словарь.
        //     * Ключ - словарь с обработанными словами и частотами встречаемости? (чекнуть библиотеку). Значение - экземпляр класса Standart (добавлен для удобства)
        //     */
        //}

        public ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>> FindStandartsWhichComparesWithGosts(HashSet<string> gosts, ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>> standarts)
        {
            var filteredData = standarts
                .Where(category => category.Value
                    .Any(subCategory => gosts
                        .Any(gostItem => subCategory.Value.NTD.Contains(gostItem) || subCategory.Value.MaterialNTD.Contains(gostItem))));

            return new ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>>(filteredData);

        }
    }
}
