using Algo.Interfaces.Handlers.ENS;
using System.Text;

namespace Algo.Handlers.ENS
{
    public class CirclesHandler : IAdditionalENSHandler<CirclesHandler>
    {
        /// <summary>
        /// Круги, шестигранники, квадраты
        /// </summary>
        
        private HashSet<string> stopWords = new HashSet<string> { "АТП" };
        private Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { "СТАЛЬ ", "КРУГ "},
        };
        public string AdditionalStringHandle(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in circleReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }

            var tokens = str.Split(new[] { ' ', '/', '.' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                sb.Append($"{filteredTokens[i]} ");
            }
            return sb.ToString().TrimEnd(' ');
        }
    }
}
