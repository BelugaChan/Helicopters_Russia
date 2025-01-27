using AbstractionsAndModels.Interfaces.ProgressStrategy;
using System.Text;

namespace Algo.MethodStrategy
{
    public class StopWordsStrategy : IStopWordsStrategy
    {
        public string RemoveWords(string str, HashSet<string> stopWords)
        {
            StringBuilder stringBuilder = new ();

            var tokens = str.Split([ ' ', '/', '.' ], StringSplitOptions.RemoveEmptyEntries);

            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append($"{filteredTokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
