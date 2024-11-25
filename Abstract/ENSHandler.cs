using Abstractions.Interfaces;
using Algo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Abstract
{
    public abstract class ENSHandler : IENSHandler
    {
        protected static HashSet<string> stopWords = new HashSet<string> { "СТ", "НА", "И", "ИЗ", "С", "СОДЕРЖ", "ТОЧН", "КЛ", "ШГ", "МЕХОБР", "КАЧ", "Х/Т", "УГЛЕР", "СОРТ", "НЕРЖ", "НСРЖ", "КАЛИБР", "ХОЛ", "ПР", "ПРУЖ", "АВИАЦ", "КОНСТР", "КОНСТРУКЦ", "ПРЕЦИЗ", "СПЛ", "ПРЕСС", "КА4", "ОТВЕТСТВ", "НАЗНА4", "ОЦИНК", "НИК", "БЕЗНИКЕЛ", "ЛЕГИР", "АВТОМАТ", "Г/К", "КОРРОЗИННОСТОЙК", "Н/УГЛЕР", "ПРЕСС", "АЛЮМИН", "СПЛАВОВ" };

        protected static string pattern = @"(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])|(?<=[А-Яа-я])(?=\d)|(?<=\d)(?=[А-Яа-я])";

        protected static Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "А","A" },
            { "В","B" },
            { "Е","E" },
            { "К", "K" },
            { "М", "M" },
            { "Н", "H" },
            { "О", "O" },
            { "Р", "P" },
            { "С", "C" },
            { "Т", "T" },
            { "У", "Y" },
            { "Х", "X" },
            { "OCT1","OCT 1" }
        };

        public virtual string StringHandler(string str) 
        {
            StringBuilder stringBuilder = new StringBuilder();

            var fixedStr = str.ToUpper();
            string result = Regex.Replace(fixedStr, pattern, " ");
            foreach (var pair in replacements)
            {
                result = result.Replace(pair.Key, pair.Value);
            }
            result = result.TrimEnd(',');
            var tokens = result.Split(new[] { ' ', '.', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var filteredTokens = tokens.Where(token => !stopWords.Contains(token)).ToList();
            if ("AEЁИOYЭЫЯ".IndexOf(filteredTokens[0][filteredTokens[0].Length - 1]) >= 0)
            {
                filteredTokens[0] = filteredTokens[0].Substring(0, filteredTokens[0].Length - 1);
            }
            for (int i = 0; i < filteredTokens.Count; i++)
            {
                stringBuilder.Append(filteredTokens[i]);
            }
            return stringBuilder.ToString();
        } //обработка строк
    }
}
