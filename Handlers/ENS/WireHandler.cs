using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class WireHandler : IAdditionalENSHandler<WireHandler>
    {
        /// <summary>
        /// Проволока
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> { "СЕРЕБР", "СТ", "ПРУЖ", "УГЛЕР", "2АКЛ", "КАЧ", "ОТВЕТСТВ", "НАЗНАЧ", "ОЦИНК", "НЕРЖ", "ХОЛОДНО", "ТЯНУТАЯ"};
        private Dictionary<string, string> wireReplacements = new Dictionary<string, string>
        {
            {"ПР", "ПРОВОЛКА" },
            {"Х/Т", "" },
            {"Н/", "" },
            { "(", "" },
            { ")", "" }
        };
        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var pair in wireReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }

            var tokens = str.Split(new[] { ' ', '/', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append($"{filteredTokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
