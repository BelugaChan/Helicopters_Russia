using Algo.Interfaces.Handlers.GOST;
using System.Text.RegularExpressions;


namespace Algo.Handlers.Garbage
{
    public class GostHandler : IGostHandle
    {
        private HashSet<string> resGosts = new HashSet<string>();
        private List<string> patterns = new List<string>() 
        {
            @"\b(ГОСТ|Г|ОСТ\s*1|ОСТ1)\s*Р?\s*\d{3,5}(?:\.\d+)?\s*-\s*\d{2,4}(?:[\/, ]?)\b",//добавить (?<![a-zA-Z])(\d+)? в начало
            @"\bТУ\s*[a-zA-Zа-яА-Я]*\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*[-.]\d{1,5}[a-zA-Zа-яА-Я]*\b",
            @"\bСТО\s*\d{1,9}-\d{1,5}-\d{1,5}\b",
            @"\bЕТУ\s*\d{1,5}",
            @"(?:^|[^а-яА-Я0-9])(ГОСТ)\s*(\d{4,5}-\d{2,4})(?=[^0-9]|$)"
            //@"ГОСТ \d{3,4,5}-\d{2,4}",
            //@"ОСТ \d{3,4,5}-\d{2,4}"
            //@"Г\d{3,4,5}-\d{2,4}"
        };
        public HashSet<string> GetGOSTFromGarbageName(string name)
        {
            resGosts.Clear();
            var fixedName = name.ToUpper();
                                
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(fixedName, pattern);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        resGosts.Add(match.Value);
                        //замена Г на ГОСТ
                        //string res = Regex.Replace(match.Value, @"Г\s*\d", match =>
                        //{
                        //    return match.Value.Replace("Г","ГОСТ");
                        //});
                        //resGosts.Add(res.Replace(" ", "").TrimEnd('/').TrimEnd(','));
                    }
                }
            }
            return resGosts;
        }

        public HashSet<string> GostsPostProcessor(HashSet<string> gosts)
        {
            resGosts.Clear();
            foreach (var gost in gosts)
            {
                string tyPatternFirst = @"Г\d+-\d+-\d+-\d+";
                string tyPatternSecond = @"(ТУ\s+)(\d+)\.(\d+\.\d+-\d+)";
                string resultFirst = Regex.Replace(gost, tyPatternFirst, match =>
                {
                    return match.Value.Replace("Г", "ТУ");
                });
                string resultSecond = Regex.Replace(resultFirst, tyPatternSecond, "$1$2-$3");

                string res = Regex.Replace(gost, @"Г\s*\d", match =>
                {
                    return match.Value.Replace("Г", "ГОСТ");
                });
                resGosts.Add(res.Replace(" ", "").TrimEnd('/').TrimEnd(','));
            }
            return resGosts;
        }
    }
}
