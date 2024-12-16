using Algo.Interfaces.Handlers.ENS;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class SoldersHandler : IAdditionalENSHandler<SoldersHandler>
    {
        /// <summary>
        /// Припои (прутки, проволока, трубки)
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> { "ОЛОВЯННОСВИНЦОВЫЕ", "В", "ПРОВОЛОКЕ", "ОЛОВЯННО", "СВИНЦОВЫЙ"};

        private Dictionary<string, string> soldersReplacements = new Dictionary<string, string>
        {
            { "ПРВ КР", "ПРВКР" },
        };
        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var pair in soldersReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }

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
