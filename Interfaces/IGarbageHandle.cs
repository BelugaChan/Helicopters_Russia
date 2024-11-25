using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces
{
    public interface IGarbageHandle
    {
        List<string> GetGOSTFromGarbageName(string name);
    }
}
