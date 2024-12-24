using Algo.Interfaces.ProgressStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.MethodStrategy
{
    public class StopWordsStrategy : IStopWordsStrategy
    {
        public string RemoveWords(string str, HashSet<string> stopWords)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var tokens = str.Split(new[] { ' ', '/', '.' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append($"{filteredTokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
