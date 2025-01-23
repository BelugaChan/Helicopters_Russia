using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class RopesAndCablesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Канаты, Тросы
        /// </summary>
        private readonly HashSet<string> stopWords = new HashSet<string> {"СТ","АВИАЦ","КОНСТР", "ТИПА","ТК" };

        private IStopWordsStrategy stopWordsStrategy;
        public RopesAndCablesHandler(IStopWordsStrategy stopWordsStrategy)
        {
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Канаты, Тросы" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
            return res;

        }
    }
}
