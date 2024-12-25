using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class ConnectionPartsHandler : IAdditionalENSHandler<ConnectionPartsHandler>
    {
        /// <summary>
        /// Части соединительные
        /// </summary>

        private HashSet<string> stopWords = new HashSet<string> { "Д","D" };
        private Dictionary<string, string> connectionReplacements = new Dictionary<string, string>
        {
            { "КОНТРАГАЙКА ", "КОНТРГАЙКА "},
        };

        private IReplacementsStrategy replacementsStrategy;
        private IStopWordsStrategy stopWordsStrategy;
        public ConnectionPartsHandler(IReplacementsStrategy replacementsStrategy, IStopWordsStrategy stopWordsStrategy)
        {
            this.replacementsStrategy = replacementsStrategy;
            this.stopWordsStrategy = stopWordsStrategy;
        }
        public string AdditionalStringHandle(string str)
        {
            var midRes = replacementsStrategy.ReplaceItems(str, connectionReplacements);
            var final = stopWordsStrategy.RemoveWords(midRes, stopWords);
            return final;

        }
    }
}
