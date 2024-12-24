using Algo.Interfaces.Handlers.ENS;
using Algo.Interfaces.ProgressStrategy;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class SheetsAndPlatesHandler : IAdditionalENSHandler<SheetsAndPlatesHandler>
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
        public string AdditionalStringHandle(string str)
        {
            var res = stopWordsStrategy.RemoveWords(str, stopWords);
            return res;
            //StringBuilder stringBuilder = new StringBuilder();
            //var tokens = str.Split(new[] { ' ', '/', '.' }, StringSplitOptions.RemoveEmptyEntries);

            //var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            //for (int i = 0; i < filteredTokens.Count; i++)
            //{
            //    stringBuilder.Append($"{filteredTokens[i]} ");
            //}
            //return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
