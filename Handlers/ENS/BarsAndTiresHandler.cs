using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class BarsAndTiresHandler : IAdditionalENSHandler<BarsAndTiresHandler>
    {
        /// <summary>
        /// Прутки, шины из алюминия и сплавов
        /// Прутки, шины из меди и сплавов
        /// Прутки из титана и сплавов
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> { "ПРЕСС", "ИЗ", "АЛЮМИНИЯ", "АЛЮМИНИЕВЫХ", "СПЛАВОВ", "АЛЮМИН","И", "КАТ", "КРУПНОГАБАРИТ", "ТИТАНОВЫЕ", "ТИТАНОВЫХ", "КОВАНЫЕ" };

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
