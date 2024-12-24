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
            //StringBuilder sb = new StringBuilder();

            //foreach (var pair in connectionReplacements)
            //{
            //    str = str.Replace(pair.Key, pair.Value);
            //}

            //var tokens = str.Split(new[] { ' ', '/', '.' }, StringSplitOptions.RemoveEmptyEntries);

            //var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            //for (int i = 0; i < filteredTokens.Count; i++)
            //{
            //    sb.Append($"{filteredTokens[i]} ");
            //}
            //return sb.ToString().TrimEnd(' ');
        }
    }
}
