using Algo.Interfaces.Handlers.GOST;
using System.Text.RegularExpressions;


namespace Algo.Handlers.Garbage
{
    public class GostHandler : IGostHandle
    {
        //private HashSet<string> resGosts = new HashSet<string>();
        private List<string> patterns = new List<string>() 
        {
            /*@"\b(ГОСТ|Г|ОСТ\s*1|ОСТ1)\s*Р?\s*\d{3,5}(?:\.\d+)?\s*-\s*\d{2,4}(?:[\/, ]?)\b",*///добавить (?<![a-zA-Z])(\d+)? в начало
            @"\b(?:\/?\s*-?\s*)?(ГОСТ|Г|ОСТ\s*1|ОСТ1)\s*Р?\s*\d{1,5}(?:[-\d]{1,5})+(?:\/?\s*)?\b",
            @"\b(?:\/?\s*)?ТУ\s*[a-zA-Zа-яА-Я]*\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*(?:\/?\s*)?\b",
            @"\bСТО\s*\d{1,9}-\d{1,5}-\d{1,5}\b",
            @"\bЕТУ\s*\d{1,5}",
            @"(?<!\s)ГОСТ\s?\d{4}-\d{2}"
        };
        public HashSet<string> GetGOSTFromPositionName(string name)
        {
            var res = new HashSet<string>();
            var fixedName = name.ToUpper();
            var resStr = "";                    
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(fixedName, pattern);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        resStr = match.Value;
                        //resGosts.Add(match.Value.Replace("/",""));
                        if (resStr.Length > 0 && !char.IsLetter(resStr[0]))
                        {
                            resStr = resStr.Substring(1);
                        }
                        if (resStr.Length > 0 && !char.IsDigit(resStr[^1]))
                        {
                            resStr = resStr.Substring(0, resStr.Length - 1);
                        }
                        if (resStr.Length < 4)
                        {
                            continue;
                        }
                        res.Add(resStr);
                    }
                }
            }
            return res;
        }

        public HashSet<string> GostsPostProcessor(HashSet<string> gosts)
        {
            //resGosts.Clear();
            //var res = new HashSet<string>();
            //foreach (var gost in gosts)
            //{
            //    string tyPatternFirst = @"Г\d+-\d+-\d+-\d+";
            //    string tyPatternSecond = @"(ТУ\s+)(\d+)\.(\d+\.\d+-\d+)";
            //    string resultFirst = Regex.Replace(gost, tyPatternFirst, match =>
            //    {
            //        return match.Value.Replace("Г", "ТУ");
            //    });
            //    string resultSecond = Regex.Replace(resultFirst, tyPatternSecond, "$1$2-$3");

            //    string resStr = Regex.Replace(gost, @"Г\s*\d", match =>
            //    {
            //        return match.Value.Replace("Г", "ГОСТ");
            //    });
            //    res.Add(resStr.Replace(" ", "").TrimEnd('/').TrimEnd(','));
            //}
            //return res;
            return gosts.Select(item =>
            {
                string processedGost = Regex.Replace(item, @"Г\d+-\d+-\d+-\d+", match => match.Value.Replace("Г", "ТУ"));
                processedGost = Regex.Replace(processedGost, @"(ТУ\s+)(\d+)\.(\d+\.\d+-\d+)", "$1$2-$3");
                processedGost = Regex.Replace(processedGost, @"Г\s*\d", match => match.Value.Replace("Г", "ГОСТ"));
                return processedGost.Replace(" ", "").TrimEnd('/').TrimEnd(',');
            }).ToHashSet();
        }

        public HashSet<string> RemoveLettersAndOtherSymbolsFromGosts(HashSet<string> gosts)
        {
            //var res = new HashSet<string>();
            //foreach (var item in gosts)
            //{
            //    if (item.Length > 0)
            //    {
            //        var withoutLetters = Regex.Replace(item, @"\p{L}", "");
            //        res.Add(withoutLetters.Replace(".","").Replace("-","").Replace("/","").Replace(" ",""));
            //    }               
            //}
            //return res;
            return gosts.Select(item => Regex.Replace(item, @"[\p{L}.\-/\s]", "")).Where(cleaned => cleaned.Length > 0).ToHashSet();
        }
    }
}
