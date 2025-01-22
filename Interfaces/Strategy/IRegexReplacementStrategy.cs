using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Interfaces.ProgressStrategy
{
    public interface IRegexReplacementStrategy
    {
        string ReplaceItemsWithRegex(string str, Dictionary<string, string> regexReplacements, RegexOptions regexOptions);
    }
}
