using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class NailsHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Гвозди, Дюбели
        /// </summary>
        
        private readonly HashSet<string> stopWords = new () { "ПР", "СТРОИТ", "С", "ПЛОСКОЙ", "ГОЛОВ" };

        private IStopWordsStrategy stopWordsStrategy;
        public NailsHandler(IStopWordsStrategy stopWordsStrategy) => this.stopWordsStrategy = stopWordsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Гвозди, Дюбели" ];
        public string AdditionalStringHandle(ProcessingContext processingContext) => stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);

    }
}
