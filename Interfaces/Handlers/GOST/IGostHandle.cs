using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.Handlers.GOST
{
    public interface IGostHandle
    {
        List<string> GetGOSTFromGarbageName(string name);

        List<string> GostsPostProcessor(List<string> gosts);
    }
}
