using AbstractionsAndModels.Interfaces.Handlers.ENS;
using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.Handlers.ENS
{
    public class RopesAndCablesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Канаты, Тросы
        /// </summary>
        private readonly HashSet<string> stopWords = new () {"СТ","АВИАЦ","КОНСТР", "ТИПА","ТК" };

        private IStopWordsStrategy stopWordsStrategy;
        public RopesAndCablesHandler(IStopWordsStrategy stopWordsStrategy) => this.stopWordsStrategy = stopWordsStrategy;

        public IEnumerable<string> SupportedKeys => [ "Канаты, Тросы" ];
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
            return res;

        }
    }
}
