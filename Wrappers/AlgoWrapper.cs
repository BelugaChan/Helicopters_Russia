using Abstractions.Interfaces;
using Algo.Interfaces.Handlers.GOST;
using Algo.Interfaces.Handlers.Standart;
using Algo.Interfaces.Wrappers;
using System.Collections.Concurrent;

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
        public (List<ConcurrentDictionary<(string, TGarbageData, HashSet<string>), ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>>>, ConcurrentDictionary<TStandart, string>, ConcurrentBag<(TGarbageData,HashSet<string>)>) AlgoWrap(HashSet<TStandart> standarts, HashSet<TGarbageData> garbageData)
        {
            //Pullenti.Sdk.InitializeAll();
            ConcurrentBag<(TGarbageData,HashSet<string>)> garbageDataWWithNoComparedStandartGroups = new();
            List<Dictionary<(string, TGarbageData), HashSet<string>>> gosts = new List<Dictionary<(string, TGarbageData), HashSet<string>>>();
            Console.WriteLine("Starting getting gosts from dirty data");
            foreach (var item in garbageData)
            {
                var itemGosts = gostHandle.GetGOSTFromPositionName(item.ShortName);
                var copyItems = new HashSet<string>(itemGosts);
                //удаление ГОСТов из грязной опзиции
                var garbageNameWithoutGosts = gostRemove.RemoveGosts(item.ShortName, itemGosts);
                var upgradedItemGosts = gostHandle.GostsPostProcessor(copyItems);

                var gostWithoutLetters = gostHandle.RemoveLettersAndOtherSymbolsFromGosts(upgradedItemGosts); // удаление букв из госта (понадобится в алгоритме Cosine)
                
                var copyUpgradedItems = new HashSet<string>(gostWithoutLetters);             
                var dict = new Dictionary<(string, TGarbageData), HashSet<string>>
                {
                    { (garbageNameWithoutGosts,item), copyUpgradedItems }
                };
                gosts.Add(dict);
            }
            Console.WriteLine("Done");
            var standartsWithHandledNames = standartHandle.HandleStandartNames(standarts);
            var groupedStandartsByEns = standartHandle.GroupingStandartsByENS(standartsWithHandledNames);//абсолютно все стандарты
            //var cosineAlgoHandledStandarts = standartHandle.HandleStandarts(groupedStandartsByEns);//абсолютно все обработанные стандарты\
            var final = new List<ConcurrentDictionary<(string, TGarbageData, HashSet<string>), ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>>>();//список грязных данных,которым сопоставлены группы эталонов
            int currentProgress = 0;

            Parallel.ForEach(gosts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (gostItems, state) =>
            {
                var gost = gostItems.Values.FirstOrDefault();
                var res = standartHandle.FindStandartsWhichComparesWithGosts(gost, groupedStandartsByEns);

                var (name, garbageData) = gostItems.Keys.FirstOrDefault();

                currentProgress = Interlocked.Increment(ref currentProgress);

                if (!res.Any())
                {
                    Console.WriteLine("No standarts!");
                    garbageDataWWithNoComparedStandartGroups.Add((garbageData, gost));
                    return;
                }
                                
                var midDict = new ConcurrentDictionary<(string, TGarbageData, HashSet<string>), ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>>();
                midDict.TryAdd((name,garbageData, gost), res);
                final.Add(midDict);          
                Console.WriteLine($"FindStandartsWhichComparesWithGosts: {Math.Round((double)currentProgress / gosts.Count * 100, 2)}");
            });

            return (final, standartsWithHandledNames, garbageDataWWithNoComparedStandartGroups);
        }
    }
}
