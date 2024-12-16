using Algo.Interfaces.Handlers.ENS;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class SheetsAndPlatesHandler : IAdditionalENSHandler<SheetsAndPlatesHandler>
    {
        /// <summary>
        /// Листы, плиты, ленты из титана и сплавов
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> { "ИЗ","ТИТАНОВЫХ","СПЛАВОВ" };
        public string AdditionalStringHandle(string str)
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
