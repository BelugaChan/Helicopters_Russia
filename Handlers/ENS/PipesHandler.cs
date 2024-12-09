using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class PipesHandler : IAdditionalENSHandler<PipesHandler>
    {
        /// <summary>
        /// Трубы бесшовные
        /// Трубы сварные
        /// Трубы, трубки из алюминия и сплавов
        /// Трубы, трубки из меди и сплавов
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> { "КОНСТРУКЦ", "И", "ХОЛ", "ТЕПЛОДЕФОРМИРОВАННЫЕ", "КВАДРАТНАЯ", "ПРЯМОУГОЛЬНАЯ", "ПРОФИЛЬНАЯ","СВАРНЫЕ","ЭЛЕКТРОСВАРНЫЕ" };

        private Dictionary<string, string> barsReplacements = new Dictionary<string, string>
        {
            { "ТРУБА ТР","ТРУБА " },
        };
        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var pair in barsReplacements)
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
