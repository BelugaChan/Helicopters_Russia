using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.ProgressStrategy
{
    public interface IReplacementsStrategy
    {
        string ReplaceItems(string str, Dictionary<string, string> replacements);
    }
}
