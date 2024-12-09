using Algo.Interfaces.Handlers.ENS;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class ScrewsHandler : IAdditionalENSHandler<ScrewsHandler>
    {
        /// <summary>
        /// Шурупы
        /// </summary>

        private HashSet<string> stopWords = new HashSet<string> { "С", "ПОТАЙНОЙ", "ГОЛОВКОЙ", "БЕЗ", "ПОКРЫТИЯ" };
        protected static Dictionary<string, string> screwsReplacements = new Dictionary<string, string>
        {
            { "БЕ 3 ","" }
        };
        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var pair in screwsReplacements)
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
