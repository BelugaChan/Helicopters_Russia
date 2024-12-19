using Abstractions.Interfaces;
using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.Handlers.GOST;
using Algo.Interfaces.Handlers.Standart;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Collections.Concurrent;

namespace Algo.Handlers.Standart
{
    public class StandartHandler<TStandart> : IStandartHandle<TStandart>
        where TStandart : IStandart
    {
        private IENSHandler eNSHandler;
        private IGostRemove gostRemove;
        private IGostHandle gostHandle;
        private IUpdatedEntityFactoryStandart<TStandart> updatedEntityFactoryStandart;
        private IProgressStrategy progressStrategy;
        public StandartHandler(IENSHandler eNSHandler,IGostRemove gostRemove,IUpdatedEntityFactoryStandart<TStandart> updatedEntityFactoryStandart, IGostHandle gostHandle, IProgressStrategy progressStrategy)
        {
            this.eNSHandler = eNSHandler;
            this.gostRemove = gostRemove;
            this.updatedEntityFactoryStandart = updatedEntityFactoryStandart;
            this.gostHandle = gostHandle;
            this.progressStrategy = progressStrategy;
        }

        public ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> GroupingStandartsByENS(ConcurrentDictionary<TStandart, string> standarts) //new feature
        {
            var res = standarts.GroupBy(e => e.Key.ENSClassification)
                .ToDictionary(group => group.Key, group => 
                new ConcurrentDictionary<TStandart, string>(group.ToDictionary(e => e.Key, e => e.Value)));
            return new ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>(res);
        }

        public ConcurrentDictionary<TStandart, string> HandleStandartNames(HashSet<TStandart> standarts)
        {
            int currentProgress = 0;
            var fixedStandarts = new ConcurrentDictionary<TStandart, string>();
            Parallel.ForEach(standarts, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (standartItem, state) =>
            {
                //удаление гостов из эталона
                var gosts = new HashSet<string>() { standartItem.MaterialNTD, standartItem.NTD }
                .Where(item => !string.IsNullOrEmpty(item) && item.Length > 0).ToHashSet();
                //удаление букв и др. символов из ГОСТ
                var handledGost = gostHandle.RemoveLettersAndOtherSymbolsFromGosts(gosts).ToArray();

                //так как в приоритете ГОСТы из столбцов то их обрабатываем и добавляем в обновлённый экземпляр класса Standart. Для удаления гостов из наименования эталона не используем значения из столбцов, а как и с грязными данными через Regex ищем подстроки и удаляем их.
                var itemGosts = gostHandle.GetGOSTFromPositionName(standartItem.Name);
                var copyItems = new HashSet<string>(itemGosts);               
                var itemNameWithRemovedGosts = gostRemove.RemoveGosts(standartItem.Name, copyItems);


                fixedStandarts.TryAdd(
                    updatedEntityFactoryStandart.CreateUpdatedEntity(
                        standartItem.Id,
                        standartItem.Code,
                        standartItem.Name,
                        (handledGost.Length > 1) ? handledGost[1] : "",
                        (handledGost.Length > 0) ? handledGost[0] : "",
                        standartItem.ENSClassification),
                    eNSHandler.BaseStringHandle(/*standartItem.Name*/itemNameWithRemovedGosts)
                    );
                currentProgress = Interlocked.Increment(ref currentProgress);
                UpdateProgress(currentProgress, standarts.Count);
            });
            return fixedStandarts;
        }

        public ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> FindStandartsWhichComparesWithGosts(HashSet<string> gosts, ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>> standarts)
        {
            var filteredData = standarts
                .Where(category => category.Value
                    .Any(subCategory => gosts
                        .Any(gostItem => subCategory.Key.NTD.Contains(gostItem) || subCategory.Key.MaterialNTD.Contains(gostItem))));

            return new ConcurrentDictionary<string, ConcurrentDictionary<TStandart, string>>(filteredData);

        }

        private void UpdateProgress(int current, int total)
        {
            if (current % 100 == 0)
            {
                progressStrategy.UpdateProgress(new Progress
                {
                    Step = "1. Обработка наименований стандартов",
                    CurrentProgress = Math.Round((double)current / total * 100, 2)
                });
                progressStrategy.LogProgress();
            }
        }
    }
}
