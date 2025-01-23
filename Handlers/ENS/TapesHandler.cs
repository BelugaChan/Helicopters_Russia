using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.MethodStrategy;
using Algo.Models;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class TapesHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Ленты, широкополосный прокат
        /// </summary>

        private readonly HashSet<string> stopWords = new HashSet<string> { "СТ", "ИЗ", "ПРУЖ" };

        private IStopWordsStrategy stopWordsStrategy;
        public TapesHandler(IStopWordsStrategy stopWordsStrategy)
        {
            this.stopWordsStrategy = stopWordsStrategy;
        }

        public IEnumerable<string> SupportedKeys => new[] { "Ленты, широкополосный прокат" };
        public string AdditionalStringHandle(ProcessingContext processingContext/*string str*/)
        {
            var res = stopWordsStrategy.RemoveWords(processingContext.Input, stopWords);
            return res;

        }
    }
}
