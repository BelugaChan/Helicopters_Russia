using AbstractionsAndModels.Interfaces.ProgressStrategy;
using System.Text.RegularExpressions;

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
