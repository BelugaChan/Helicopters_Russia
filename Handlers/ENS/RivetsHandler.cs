using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class RivetsHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Заклепки
        /// </summary>

        private readonly HashSet<string> stopWords = new () { "С", "СЕРДЕЧ", "СЕРДЕЧНИКОМ"};

        private IStopWordsStrategy stopWordsStrategy;
        public RivetsHandler(IStopWordsStrategy stopWordsStrategy) => this.stopWordsStrategy = stopWordsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Заклепки" ];
        public string AdditionalStringHandle(ProcessingContext processingContext) => stopWordsStrategy.RemoveWords(processingContext.Input, stopWords); 
    }
}
