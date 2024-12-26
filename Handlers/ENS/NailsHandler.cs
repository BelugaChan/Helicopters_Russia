using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class NailsHandler : IAdditionalENSHandler
    {
        /// <summary>
        /// Гвозди, Дюбели
        /// </summary>
        
        private readonly HashSet<string> stopWords = new HashSet<string> { "ПР", "СТРОИТ", "С", "ПЛОСКОЙ", "ГОЛОВ" };

        private IStopWordsStrategy stopWordsStrategy;
        public NailsHandler(IStopWordsStrategy stopWordsStrategy)
        {
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public IEnumerable<string> SupportedKeys => new[] { "Гвозди, Дюбели" };
        public string AdditionalStringHandle(string str)
        {
            var res = stopWordsStrategy.RemoveWords(str, stopWords);
            return res;

        }
    }
}
