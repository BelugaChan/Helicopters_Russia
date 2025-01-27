using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class SheetsAndPlatesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Листы, плиты, ленты из титана и сплавов
        /// </summary>
        private readonly HashSet<string> stopWords = new () { "ИЗ","ТИТАНОВЫХ","СПЛАВОВ" };

        private IStopWordsStrategy stopWordsStrategy;
        public SheetsAndPlatesHandler(IStopWordsStrategy stopWordsStrategy) => this.stopWordsStrategy = stopWordsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Листы, плиты, ленты из титана и сплавов" ];
        public string AdditionalStringHandle(ProcessingContext processingContext) => stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);

    }
}
