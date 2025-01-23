using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.Models;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class SheetsAndPlatesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Листы, плиты, ленты из титана и сплавов
        /// </summary>
        private readonly HashSet<string> stopWords = new HashSet<string> { "ИЗ","ТИТАНОВЫХ","СПЛАВОВ" };

        private IStopWordsStrategy stopWordsStrategy;
        public SheetsAndPlatesHandler(IStopWordsStrategy stopWordsStrategy)
        {
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Листы, плиты, ленты из титана и сплавов" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
            return res;

        }
    }
}
