using Abstractions.Interfaces;
using Algo.Algotithms;
using Algo.Factory;
using Algo.Handlers.ENS;
using Algo.Handlers.Garbage;
using Algo.Handlers.Standart;
using Algo.Interfaces;
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
    public class AlgoWrapper<TStandart> : IAlgoWrapper
        where TStandart : IStandart
    {
        private IGarbageHandle garbageHandle;
        private IStandartHandle standartHandle;
        public AlgoWrapper(IGarbageHandle garbageHandle, IStandartHandle standartHandle)
        {
            this.garbageHandle= garbageHandle;
            this.standartHandle= standartHandle;
        }
        public (List<ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary<string, TStandart>>>>,ConcurrentDictionary<string,TStandart>) AlgoWrap<TStandart, TGarbageData>(HashSet<TStandart> standarts, HashSet<TGarbageData> garbageData)
            where TStandart : IStandart
            where TGarbageData : IGarbageData
        {
            //Pullenti.Sdk.InitializeAll();
            List<Dictionary<TGarbageData, List<string>>> gosts = new List<Dictionary<TGarbageData, List<string>>>();
            Console.WriteLine("Starting getting gosts from dirty data");
            foreach (var item in garbageData)
            {
                var itemGosts = garbageHandle.GetGOSTFromGarbageName(item.ShortName);
                var copyItems = new List<string>(itemGosts);
                var dict = new Dictionary<TGarbageData, List<string>>();
                dict.Add(item, copyItems);
                gosts.Add(dict);
            }
            Console.WriteLine("Done");
            var standartsWithHandledNames = standartHandle.HandleStandartNames(standarts);
            var groupedStandartsByEns = standartHandle.GroupingStandartsByENS(standartsWithHandledNames);//абсолютно все стандарты
            //var cosineAlgoHandledStandarts = standartHandle.HandleStandarts(groupedStandartsByEns);//абсолютно все обработанные стандарты\
            var final = new List<ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>>>>();//список грязных данных,которым сопоставлены группы эталонов
            int currentProgress = 0;

            Parallel.ForEach(gosts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (gostItems, state) =>
            {
                var res = standartHandle.FindStandartsWhichComparesWithGosts(gostItems.Values.FirstOrDefault(), groupedStandartsByEns);
                var midDict = new ConcurrentDictionary<TGarbageData, ConcurrentDictionary<string, ConcurrentDictionary<string/*ConcurrentDictionary<string, int>*/, TStandart>>>();
                midDict.TryAdd(gostItems.Keys.FirstOrDefault(), res);
                final.Add(midDict);
                currentProgress = Interlocked.Increment(ref currentProgress);
                Console.WriteLine($"FindStandartsWhichComparesWithGosts: {Math.Round((double)currentProgress / gosts.Count * 100, 2)}");
            });

            return (final, standartsWithHandledNames);
        }
    }
}
