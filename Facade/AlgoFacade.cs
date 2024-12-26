using Abstractions.Interfaces;
using Algo.Interfaces.Handlers.GOST;
using Algo.Interfaces.Handlers.Standart;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Collections.Concurrent;

namespace Algo.Facade
{
    public class AlgoFacade<TStandart, TGarbageData>
        where TStandart : IStandart
        where TGarbageData : IGarbageData
    {
        private IGostHandle gostHandle;
        private IStandartHandle<TStandart> standartHandle;
        private IGostRemove gostRemove;
        private IProgressStrategy progressStrategy;
        public AlgoFacade(IGostHandle gostHandle, IStandartHandle<TStandart> standartHandle, IGostRemove gostRemove, IProgressStrategy progressStrategy)
        {
            this.gostHandle = gostHandle;
            this.standartHandle = standartHandle;
            this.gostRemove = gostRemove;
            this.progressStrategy = progressStrategy;
        }

        //Основной метод
        public AlgoResult<TStandart,TGarbageData> AlgoWrap(HashSet<TStandart> standarts, HashSet<TGarbageData> garbageData)
        {
            var processedGarbageData = ProcessedGarbageData(garbageData);

            var processedStandarts = ProcessedStandarts(standarts);//абсолютно все стандарты
            
            var matchedResults = MatchResults(processedGarbageData, processedStandarts);

            return new AlgoResult<TStandart, TGarbageData>
            {
                MatchedData = matchedResults,
                ProcessedStandards = processedStandarts,
                UnmatchedGarbageData = processedGarbageData.Unmatched
            };
        }

        //Обработка стандартов
        public GroupedStandarts<TStandart> ProcessedStandarts(HashSet<TStandart> standarts)
        {
            var standartsWithHandledNames = standartHandle.HandleStandartNames(standarts);
            var groupedStandartsByEns = standartHandle.GroupingStandartsByENS(standartsWithHandledNames);
            return new GroupedStandarts<TStandart>(groupedStandartsByEns);
        }
        //Обработка грязных данных
        public ProcessedGarbageData<TGarbageData> ProcessedGarbageData(HashSet<TGarbageData> garbageData)
        {
            var result = new ProcessedGarbageData<TGarbageData>();
            foreach (var item in garbageData)
            {
                var itemGosts = gostHandle.GetGOSTFromPositionName(item.ShortName);

                //удаление ГОСТов из грязной опзиции
                var garbageNameWithoutGosts = gostRemove.RemoveGosts(item.ShortName, itemGosts);
                var upgradedItemGosts = gostHandle.GostsPostProcessor(itemGosts);
                var gostWithoutLetters = gostHandle.RemoveLettersAndOtherSymbolsFromGosts(upgradedItemGosts); // удаление букв из госта (понадобится в алгоритме Cosine)

                result.Add(garbageNameWithoutGosts, item, gostWithoutLetters);
            }
            return result;
        }

        //Поиск сопоставлений: для каждой позиции из грязных данных идёт сопоставление по ГОСТам. Если сопоставление положительное, то грязной позиции прикрепляется группа ЕНС, для которой было сопоставление по ГОСТу.
        //Если сопоставление не находится (ГОСТ грязной позиции не распознан, либо такого ГОСТа нет в эталонах, то грязная запись добавляется в отдельную коллекцию, по которой в дальнейшем будет дефолтный прогон алгоритма)
        public ConcurrentBag<MatchedResult<TStandart, TGarbageData>> MatchResults(ProcessedGarbageData<TGarbageData> processedGarbageData, GroupedStandarts<TStandart> groupedStandarts)
        {
            int currentProgress = 0;
            var result = new ConcurrentBag<MatchedResult<TStandart, TGarbageData>>();
            Parallel.ForEach(processedGarbageData.Items, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (processedGarbageDataItem) =>
            {
                var matches = standartHandle.FindStandartsWhichComparesWithGosts(processedGarbageDataItem.ProcessedGosts, groupedStandarts.GroupStandarts);
                if (!matches.Any())
                {
                    processedGarbageData.MarkAsUnmatched(processedGarbageDataItem);
                }
                else
                {
                    result.Add(new MatchedResult<TStandart, TGarbageData>(processedGarbageDataItem, matches));
                }
                currentProgress = Interlocked.Increment(ref currentProgress);
                if (currentProgress % 100 == 0)
                {
                    progressStrategy.UpdateProgress(new Progress { Step = "2. Сопоставление групп стандартов грязным позициям", CurrentProgress = Math.Round((double)currentProgress / processedGarbageData.Items.Count * 100, 2) });
                    progressStrategy.LogProgress();
                    //Console.WriteLine($"HandleStandartNames: {Math.Round((double)currentProgress / standarts.Count * 100,2)}");
                }
            });
            return result;
        }
    }

    //Классы-обёртки
    public class AlgoResult<TStandart, TGarbageData>
    {
        public ConcurrentBag<MatchedResult<TStandart, TGarbageData>> MatchedData { get; set; }
        public GroupedStandarts<TStandart> ProcessedStandards { get; set; }
        public ConcurrentBag<(TGarbageData, HashSet<string>)> UnmatchedGarbageData { get; set; }
    }

    public class ProcessedGarbageData<TGarbageData>
    {
        public ConcurrentBag<GarbageItem<TGarbageData>> Items { get; } = new();
        public ConcurrentBag<(TGarbageData, HashSet<string>)> Unmatched { get; } = new();

        public void Add(string cleanedName, TGarbageData data, HashSet<string> processedGosts)
        {
            Items.Add(new GarbageItem<TGarbageData>(cleanedName, data, processedGosts));
        }

        public void MarkAsUnmatched(GarbageItem<TGarbageData> item)
        {
            Unmatched.Add((item.Data, item.ProcessedGosts));
        }
    }

    public class GroupedStandarts<TStandart>
    {
        public ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> GroupStandarts { get; }
        public GroupedStandarts(ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> groupedStandarts)
        {
            GroupStandarts = groupedStandarts;
        }
    }


    public class GarbageItem<TGarbageData>
    {
        public string ProcessedName { get; }

        public TGarbageData Data { get; }

        public HashSet<string> ProcessedGosts { get; }

        public GarbageItem(string processedName, TGarbageData data, HashSet<string> processedGosts)
        {
            ProcessedName = processedName;
            Data = data;
            ProcessedGosts = processedGosts;
        }
    }
    
    public class MatchedResult<TStandart, TGarbageData>
    {
        public GarbageItem<TGarbageData> GarbageItem { get; }
        public ConcurrentDictionary<string,ConcurrentDictionary<TStandart,string>> Matches { get; }

        public MatchedResult(GarbageItem<TGarbageData> garbageItem, ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> matches)
        {
            GarbageItem = garbageItem;
            Matches = matches;
        }
    }
}
