using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class SoldersHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Припои (прутки, проволока, трубки)
        /// </summary>
        private HashSet<string> stopWords = new () { "ОЛОВЯННОСВИНЦОВЫЕ", "В", "ПРОВОЛОКЕ", "ОЛОВЯННО", "СВИНЦОВЫЙ"};

        private Dictionary<string, string> soldersReplacements = new ()
        {
            { "ПРВ КР", "ПРВКР" },
        };

        private IReplacementsStrategy replacementsStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public SoldersHandler(IReplacementsStrategy replacementsStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }

        public IEnumerable<string> SupportedKeys => [ "Припои (прутки, проволока, трубки)" ];
        public string AdditionalStringHandle(ProcessingContext processingContext)
        {
            var mid = replacementsStrategy.ReplaceItems(processingContext.Input, soldersReplacements);
            var final = stopWordsStrategy.RemoveWords(mid, stopWords);
            return final;

        }
    }
}
