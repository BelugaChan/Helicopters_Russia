using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class ScrewsHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Шурупы
        /// </summary>

        private HashSet<string> stopWords = new () { "С", "ПОТАЙНОЙ", "ГОЛОВКОЙ", "БЕЗ", "ПОКРЫТИЯ" };
        protected static Dictionary<string, string> screwsReplacements = new ()
        {
            { "БЕ 3 ","" }
        };

        private IReplacementsStrategy replacementsStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public ScrewsHandler(IReplacementsStrategy replacementsStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => [ "Шурупы" ];
        public string AdditionalStringHandle(ProcessingContext processingContext)
        {
            var mid = replacementsStrategy.ReplaceItems(processingContext.Input, screwsReplacements);
            var final = stopWordsStrategy.RemoveWords(mid, stopWords);
            return final;

        }
    }
}
