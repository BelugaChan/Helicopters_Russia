using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class TapesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Ленты, широкополосный прокат
        /// </summary>

        private readonly HashSet<string> stopWords = new () { "СТ", "ИЗ", "ПРУЖ" };

        private IStopWordsStrategy stopWordsStrategy;
        public TapesHandler(IStopWordsStrategy stopWordsStrategy) => this.stopWordsStrategy = stopWordsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Ленты, широкополосный прокат" ];
        public string AdditionalStringHandle(ProcessingContext processingContext) => stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);

    }
}
