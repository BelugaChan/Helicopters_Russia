using Abstractions.Interfaces;
using Algo.Algotithms;
using Algo.Factory;
using Algo.Handlers.ENS;
using Algo.Handlers.Garbage;
using Algo.Handlers.Standart;
using Algo.Interfaces.Handlers.GOST;
using Algo.Interfaces.Handlers.Standart;
using Algo.Interfaces.Wrappers;
using Algo.Models;
using ExcelHandler.Interfaces;
using ExcelHandler.Readers;
using ExcelHandler.Writers;
using F23.StringSimilarity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Wrappers
{
    public class AlgoWrapper<TStandart, TGarbageData> : IAlgoWrapper<TStandart, TGarbageData>
        where TStandart : IStandart
        where TGarbageData : IGarbageData
    {
        private IGostHandle gostHandle;
        private IStandartHandle<TStandart> standartHandle;
        private IGostRemove gostRemove;
        public AlgoWrapper(IGostHandle gostHandle, IStandartHandle<TStandart> standartHandle, IGostRemove gostRemove)
        {
            this.gostHandle = gostHandle;
            this.standartHandle = standartHandle;
            this.gostRemove = gostRemove;
        }
        public (List<ConcurrentDictionary<(string, TGarbageData), ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>>>>, ConcurrentDictionary<string, TStandart>) AlgoWrap(HashSet<TStandart> standarts, HashSet<TGarbageData> garbageData)
        {
            //Pullenti.Sdk.InitializeAll();
            List<Dictionary<(string, TGarbageData), HashSet<string>>> gosts = new List<Dictionary<(string, TGarbageData), HashSet<string>>>();
            Console.WriteLine("Starting getting gosts from dirty data");
            foreach (var item in garbageData)
            {
                var itemGosts = gostHandle.GetGOSTFromGarbageName(item.ShortName);
                var copyItems = new HashSet<string>(itemGosts);
                //удаление ГОСТов из грязной опзиции
                var garbageNameWithoutGosts = gostRemove.RemoveGosts(item.ShortName, itemGosts);
                var upgradedItemGosts = gostHandle.GostsPostProcessor(copyItems);
                var copyUpgradedItems = new HashSet<string>(upgradedItemGosts);
                var dict = new Dictionary<(string, TGarbageData), HashSet<string>>
                {
                    { (garbageNameWithoutGosts,item)/*item*/, copyUpgradedItems }
                };
                gosts.Add(dict);
            }
            Console.WriteLine("Done");
            var standartsWithHandledNames = standartHandle.HandleStandartNames(standarts);
            var groupedStandartsByEns = standartHandle.GroupingStandartsByENS(standartsWithHandledNames);//абсолютно все стандарты
            //var cosineAlgoHandledStandarts = standartHandle.HandleStandarts(groupedStandartsByEns);//абсолютно все обработанные стандарты\
            var final = new List<ConcurrentDictionary<(string, TGarbageData), ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>>>>();//список грязных данных,которым сопоставлены группы эталонов
            int currentProgress = 0;

            Parallel.ForEach(gosts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (gostItems, state) =>
            {
                var res = standartHandle.FindStandartsWhichComparesWithGosts(gostItems.Values.FirstOrDefault(), groupedStandartsByEns);
                var midDict = new ConcurrentDictionary<(string, TGarbageData), ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>>>();
                midDict.TryAdd(gostItems.Keys.FirstOrDefault(), res);
                final.Add(midDict);
                currentProgress = Interlocked.Increment(ref currentProgress);
                Console.WriteLine($"FindStandartsWhichComparesWithGosts: {Math.Round((double)currentProgress / gosts.Count * 100, 2)}");
            });

            return (final, standartsWithHandledNames);
        }
    }
}
