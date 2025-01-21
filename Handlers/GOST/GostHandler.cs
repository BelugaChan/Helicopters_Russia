using Algo.Interfaces.Handlers.GOST;
using System.Text.RegularExpressions;


namespace Algo.Handlers.Garbage
{
    public class GostHandler : IGostHandle
    {
        //private HashSet<string> resGosts = new HashSet<string>();
        private List<string> patterns = new List<string>() 
        {
            @"(?<!\s)ГОСТ\s?\d{4}-\d{2}\b",
            @"\bГОСТ\d{1,5}-\d{2}(?=[.,]|\b)",//new feature
            @"\b(Г|ГОСТ)\s*\d{5,6}\.\d{1,3}-\d{1,4}",
            /*@"\b(ГОСТ|Г|ОСТ\s*1|ОСТ1)\s*Р?\s*\d{3,5}(?:\.\d+)?\s*-\s*\d{2,4}(?:[\/, ]?)\b",*///добавить (?<![a-zA-Z])(\d+)? в начало
            @"\b(?:\/?\s*-?\s*)?(ГОСТ|Г|ОСТ\s*1|ОСТ1)\s*Р?\s*\d{1,5}(?:[-\d]{1,5})+(?:\/?\s*)?(?![-.])\b",
            @"\b(?:\/?\s*)?ТУ\s*[a-zA-Zа-яА-Я]*\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*(?:\/?\s*)?\b",
            @"\bСТО\s*\d{1,9}-\d{1,5}-\d{1,5}\b",
            @"\bЕТУ\s*\d{1,5}"           
        };
        public HashSet<string> GetGOSTFromPositionName(string name)
        {
            var res = new HashSet<string>();
            var fixedName = name.ToUpper();
            var resStr = "";                    
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(fixedName, pattern).OrderByDescending(m => m.Length);
                if (matches.Count() > 0)
                {
                    foreach (Match match in matches)
                    {
                        resStr = match.Value;
                        if (resStr.Length > 0 && !char.IsLetter(resStr[0]))
                            resStr = resStr.Substring(1);
                        if (resStr.Length > 0 && !char.IsDigit(resStr[^1]))
                            resStr = resStr.Substring(0, resStr.Length - 1);
                        if (resStr.Length < 4)
                            continue;
                        res.Add(resStr);
                    }
                }
            }
            return res;
        }

        public HashSet<string> GostsPostProcessor(HashSet<string> gosts)
        {
            var handled = gosts.Select(item =>
            {
                string processedGost = Regex.Replace(item, @"Г\d+-\d+-\d+-\d+", match => match.Value.Replace("Г", "ТУ"));
                processedGost = Regex.Replace(processedGost, @"(ТУ\s+)(\d+)\.(\d+\.\d+-\d+)", "$1$2-$3");
                processedGost = Regex.Replace(processedGost, @"Г\s*\d", match => match.Value.Replace("Г", "ГОСТ"));
                return processedGost.Replace(" ", "").TrimEnd('/').TrimEnd(',');
            }).ToHashSet();
            var itemsToRemove = new HashSet<string>();
            foreach (var str in handled) //Если в GetGOSTFromPositionName выделилась лишняя часть ГОСТа, к примеру ГОСТ1144-80 и ГОСТ1144, то ГОСТ1144 будет удалён как повторяющийся элемент
            {
                if (handled.Any(other => other != str && other.Contains(str)))
                {
                    itemsToRemove.Add(str);
                }
            }
            foreach (var str in itemsToRemove)
            {
                handled.Remove(str);
            }
            return handled;
        }

        public string RemoveLettersAndOtherSymbolsFromGost(string gost)
        {
            if (!string.IsNullOrEmpty(gost))
            {
                return Regex.Replace(gost, @"[\p{L}/\s]", "");
            }
            return "";/*gosts.Select(item => Regex.Replace(item, @"[\p{L}.\-/\s]", "")).Where(cleaned => cleaned.Length > 0).ToArray();*/
        }
    }
}
