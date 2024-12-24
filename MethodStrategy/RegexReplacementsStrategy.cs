using Algo.Interfaces.ProgressStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.MethodStrategy
{
    public class RegexReplacementsStrategy : IRegexReplacementStrategy
    {
        public string ReplaceItemsWithRegex(string str, Dictionary<string, string> regexReplacements, RegexOptions regexOptions)
        {
            foreach (var pair in regexReplacements)
            {
                str = Regex.Replace(str, pair.Key, pair.Value, regexOptions);
            }
            return str;
        }
    }
}
