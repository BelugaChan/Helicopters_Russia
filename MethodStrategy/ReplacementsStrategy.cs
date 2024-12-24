using Algo.Interfaces.ProgressStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.MethodStrategy
{
    public class ReplacementsStrategy : IReplacementsStrategy
    {
        public string ReplaceItems(string str, Dictionary<string, string> replacements)
        {
            foreach (var pair in replacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
