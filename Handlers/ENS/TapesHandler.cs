using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using Algo.MethodStrategy;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class TapesHandler : IAdditionalENSHandler<TapesHandler>
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
        public string AdditionalStringHandle(string str)
        {
            var res = stopWordsStrategy.RemoveWords(str, stopWords);
            return res;

        }
    }
}
