using AbstractionsAndModels.Abstract;
using AbstractionsAndModels.Interfaces.Handlers.GOST;
using AbstractionsAndModels.Interfaces.Handlers.Standart;
using AbstractionsAndModels.Interfaces.Models;
using Helicopters_Russia.Interfaces.Repository;
using Serilog;
using System.Collections.Concurrent;

namespace Helicopters_Russia.Facade
{
    public class AlgoFacade<TStandart, TGarbageData>
        where TStandart : IStandart
        where TGarbageData : IGarbageData
    {
        private IGostHandle gostHandle;
        private IStandartHandle<TStandart> standartHandle;
        private IGostRemove gostRemove;
        //private IStandartRepository<TStandart> standartRepository;
        private IServiceProvider serviceProvider;
        private ProgressStrategy progressStrategy;

        private Dictionary<string, List<string>> keyWords = new Dictionary<string, List<string>>() 
        { {"Круги, шестигранники, квадраты", new List<string>(){"КРУГ","КРУГИ","ШЕСТИГРАННИК"} } };

        public AlgoFacade(IGostHandle gostHandle, IStandartHandle<TStandart> standartHandle, IGostRemove gostRemove, ProgressStrategy progressStrategy, /*IStandartRepository<TStandart> standartRepository*/IServiceProvider serviceProvider)
        {
            this.gostHandle = gostHandle;
            this.standartHandle = standartHandle;
            this.gostRemove = gostRemove;
            this.progressStrategy = progressStrategy;
            this.serviceProvider = serviceProvider; 
            //this.standartRepository = standartRepository;
        }

        //Основной метод
        public async Task<AlgoResult<TStandart,TGarbageData>> AlgoWrap(/*HashSet<TStandart> standarts,*/ HashSet<TGarbageData> garbageData)
        {
            var processedGarbageData = ProcessedGarbageData(garbageData);
            var matchedResults = await MatchResults(processedGarbageData/*, processedStandarts*/);

            //old
            //var processedStandarts = ProcessedStandarts(standarts);//абсолютно все стандарты
            if (processedGarbageData.Unmatched.Count > 0)
            {

            }

            var groupedStandarts = await GetGroupedStandarts();


            return new AlgoResult<TStandart, TGarbageData>
            {
                MatchedData = matchedResults,
                //old
                //ProcessedStandards = processedStandarts,
                ProcessedStandards = groupedStandarts,
                UnmatchedGarbageData = processedGarbageData.Unmatched
            };
        }

        //Обработка стандартов при их вставке в бд
        public async Task ProcessStandartsAndInsertThemIntoDbAsync(HashSet<TStandart> standarts)
        {
            using var scope = serviceProvider.CreateScope();
            var standartRepository = scope.ServiceProvider.GetRequiredService<IStandartRepository<TStandart>>();
            Log.Information("In AlgoFacade. Start handling standarts.");
            var standartsWithHandledNames = standartHandle.HandleStandartNames(standarts).ToList();
            Log.Information("In AlgoFacade. Start handling standarts. Done.");
            await standartRepository.AddStandartsAsync(standartsWithHandledNames);
        }

        private void DetectKeyWordsInNames()
        {

        }

        private async Task<GroupedStandarts<TStandart>> GetGroupedStandarts()
        {
            using var scope = serviceProvider.CreateScope();
            var standartRepository = scope.ServiceProvider.GetRequiredService<IStandartRepository<TStandart>>();

            var groupedStandarts = await standartRepository.GetAllGroupedStandartsAsync();
            return new GroupedStandarts<TStandart>(new ConcurrentDictionary<string, List<TStandart>>(groupedStandarts));
        }
        //old
        //public GroupedStandarts<TStandart> ProcessedStandarts(HashSet<TStandart> standarts)
        //{
        //    var standartsWithHandledNames = standartHandle.HandleStandartNames(standarts);
        //    var groupedStandartsByEns = standartHandle.GroupingStandartsByENS(standartsWithHandledNames);
        //    return new GroupedStandarts<TStandart>(groupedStandartsByEns);
        //}
        //Обработка грязных данных
        private ProcessedGarbageData<TGarbageData> ProcessedGarbageData(HashSet<TGarbageData> garbageData)
        {
            var result = new ProcessedGarbageData<TGarbageData>();
            foreach (var item in garbageData)
            {
                var itemGosts = gostHandle.GetGOSTFromPositionName(item.ShortName);

                //удаление ГОСТов из грязной опзиции
                var garbageNameWithoutGosts = gostRemove.RemoveGosts(item.ShortName, itemGosts);
                var upgradedItemGosts = gostHandle.GostsPostProcessor(itemGosts)
                                                  .Select(gostHandle.RemoveLettersAndOtherSymbolsFromGost).ToHashSet();
                //var gostWithoutLetters = RemoveLettersAndOtherSymbolsFromGost(upgradedItemGosts); // удаление букв из госта (понадобится в алгоритме Cosine)

                result.Add(garbageNameWithoutGosts, item, /*gostWithoutLetters*/upgradedItemGosts);
            }
            return result;
        }

        //Поиск сопоставлений: для каждой позиции из грязных данных идёт сопоставление по ГОСТам. Если сопоставление положительное, то грязной позиции прикрепляется группа ЕНС, для которой было сопоставление по ГОСТу.
        //Если сопоставление не находится (ГОСТ грязной позиции не распознан, либо такого ГОСТа нет в эталонах, то грязная запись добавляется в отдельную коллекцию, по которой в дальнейшем будет дефолтный прогон алгоритма)
        private async Task<ConcurrentBag<MatchedResult<TStandart, TGarbageData>>> MatchResults(ProcessedGarbageData<TGarbageData> processedGarbageData/*, GroupedStandarts<TStandart> groupedStandarts*/)
        {
            using var scope = serviceProvider.CreateScope();
            var standartRepository = scope.ServiceProvider.GetRequiredService<IStandartRepository<TStandart>>();

            int currentProgress = 0;
            var result = new ConcurrentBag<MatchedResult<TStandart, TGarbageData>>();
            foreach (var processedGarbageDataItem in processedGarbageData.Items)
            {
                var matches = await standartRepository.GetStandartsWhichContainGostsAsync(processedGarbageDataItem.ProcessedGosts);
                if (matches.Count == 0)
                {
                    processedGarbageData.MarkAsUnmatched(processedGarbageDataItem);
                }
                else
                {
                    result.Add(new MatchedResult<TStandart, TGarbageData>(processedGarbageDataItem, new ConcurrentDictionary<string, List<TStandart>>(matches)));
                }
            }
            //old
            //Parallel.ForEach(processedGarbageData.Items, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (processedGarbageDataItem) =>
            //{
            //    var matches = standartHandle.FindStandartsWhichComparesWithGosts(processedGarbageDataItem.ProcessedGosts, groupedStandarts.GroupStandarts);
            //    if (matches.IsEmpty)
            //    {
            //        processedGarbageData.MarkAsUnmatched(processedGarbageDataItem);
            //    }
            //    else
            //    {
            //        result.Add(new MatchedResult<TStandart, TGarbageData>(processedGarbageDataItem, matches));
            //    }
            //    currentProgress = Interlocked.Increment(ref currentProgress);
            //    if (currentProgress % 100 == 0)
            //    {
            //        progressStrategy.UpdateProgress(new Progress { Step = "2. Сопоставление групп стандартов грязным позициям", CurrentProgress = Math.Round((double)currentProgress / processedGarbageData.Items.Count * 100, 2) });
            //        progressStrategy.LogProgress();
            //    }
            //});
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
            => Items.Add(new GarbageItem<TGarbageData>(cleanedName, data, processedGosts));

        public void MarkAsUnmatched(GarbageItem<TGarbageData> item)
            => Unmatched.Add((item.Data, item.ProcessedGosts));
    }

    public class GroupedStandarts<TStandart>
    {
        public /*ConcurrentDictionary<string, Dictionary<TStandart, string>>*/ConcurrentDictionary<string, List<TStandart>> GroupStandarts { get; }
        public GroupedStandarts(/*ConcurrentDictionary<string, Dictionary<TStandart, string>>*/ConcurrentDictionary<string, List<TStandart>> groupedStandarts)
            => GroupStandarts = groupedStandarts;
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

        public ConcurrentDictionary<string, List<TStandart>> Matches { get; }
        //old
        //public ConcurrentDictionary<string,Dictionary<TStandart,string>> Matches { get; }

        public MatchedResult(GarbageItem<TGarbageData> garbageItem, ConcurrentDictionary<string, List<TStandart>> /*ConcurrentDictionary<string, Dictionary<TStandart, string>>*/ matches)
        {
            GarbageItem = garbageItem;
            Matches = matches;
        }
    }
}
