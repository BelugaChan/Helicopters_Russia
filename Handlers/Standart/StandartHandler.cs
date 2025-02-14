using AbstractionsAndModels.Abstract;
using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.Handlers.GOST;
using AbstractionsAndModels.Interfaces.Handlers.Standart;
using AbstractionsAndModels.Interfaces.Models;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;
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
        private ProgressStrategy progressStrategy;
        public StandartHandler(IENSHandler eNSHandler,IGostRemove gostRemove,IUpdatedEntityFactoryStandart<TStandart> updatedEntityFactoryStandart, IGostHandle gostHandle, ProgressStrategy progressStrategy)
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
            var fixedStandarts = new ConcurrentDictionary<TStandart, string>();
            int currentProgress = 0;           
            int total = standarts.Count;

            Parallel.For(0, total, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                var standartItem = standarts.ElementAt(i);
                //удаление гостов из эталона
                var gosts = new[] { standartItem.MaterialNTD, standartItem.NTD }
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Select(gostHandle.RemoveLettersAndOtherSymbolsFromGost)//удаление букв и др. символов из ГОСТ
                    .ToArray();
                
                //var handledGost = gostHandle.RemoveLettersAndOtherSymbolsFromGosts(gosts).ToArray();

                //так как в приоритете ГОСТы из столбцов то их обрабатываем и добавляем в обновлённый экземпляр класса Standart. Для удаления гостов из наименования эталона не используем значения из столбцов, а как и с грязными данными через Regex ищем подстроки и удаляем их.
                var itemGosts = gostHandle.GetGOSTFromPositionName(standartItem.Name);
                //var copyItems = new HashSet<string>(itemGosts);               
                var itemNameWithRemovedGosts = gostRemove.RemoveGosts(standartItem.Name, itemGosts);

                var updatedStandart = updatedEntityFactoryStandart.CreateUpdatedEntity
                (
                    standartItem.Id,
                    standartItem.Code,
                    standartItem.Name,
                    (gosts.Length > 1) ? gosts[1] : "",
                    (gosts.Length > 0) ? gosts[0] : "",
                    standartItem.ENSClassification
                );

                var handledName = eNSHandler.BaseStringHandle(itemNameWithRemovedGosts);
                fixedStandarts.TryAdd(updatedStandart,handledName);

                if (Interlocked.Increment(ref currentProgress) % 100 == 0)
                {
                    UpdateProgress(currentProgress, total);
                }
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
