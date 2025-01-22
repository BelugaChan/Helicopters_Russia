using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.ProgressStrategy
{
    public interface IStopWordsStrategy
    {
        string RemoveWords(string str, HashSet<string> stopWords);
    }
}
