using Algo.Interfaces.Handlers.ENS;
using System.Text;
using System.Text.RegularExpressions;

namespace Algo.Handlers.ENS
{
    public class CalsibCirclesHandler : IAdditionalENSHandler<CalsibCirclesHandler>
    {
        /// <summary>
        /// Калиброванные круги, шестигранники, квадраты
        /// </summary>
        private HashSet<string> stopWords = new HashSet<string> {"АТП","АКБ","БЕЗНИКЕЛ", "СОДЕРЖ", "КЛ", "КАЛИБР", "ЛЕГИР", "ОБРАВ", "СТ", "НА", "ТОЧН", "МЕХОБР", "КАЧ", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КОНСТР", "КОНСТРУКЦ", "ОЦИНК", "НИК", "ЛЕГИР", "ИНСТР"};

        private Dictionary<string, string> circleReplacements = new Dictionary<string, string>
        {
            { " КР ", " КРУГ "},
            { " ШГ ", " ШЕСТИГРАННИК " },
            { " ШЕСТИГРАН ", " ШЕСТИГРАННИК " },
            { "КРУГ В I", "КРУГ В 1 I"}
        };

        private Dictionary<string, string> circleRegex = new Dictionary<string, string>
        {
            { @"(КР.*)(КР)", "$1"},//удаление повторений КР и ШГ
            { @"(ШГ.*)(ШГ)", "$1"},
            { @"\bКР\s*\d+","КРУГ" },
            { @"\bШГ\s*\d+","ШЕСТГРАННИК" },
            { @"\bВ\s*Н\s*(\d+)\s*АТП?\s*Ф?\s*(\d+)\b", @"Н $1 НД $2" }
        };
        public string AdditionalStringHandle(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var pair in circleRegex)
            {
                str = Regex.Replace(str, pair.Key, pair.Value);
            }

            foreach (var pair in circleReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }

            var tokens = str.Split(new[] { ' ', '/', '.'}, StringSplitOptions.RemoveEmptyEntries);
            
            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append($"{filteredTokens[i]} ");
            }
            return stringBuilder.ToString().TrimEnd(' ');
        }
    }
}
