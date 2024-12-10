using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.Handlers.GOST
{
    public interface IGostHandle
    {
        HashSet<string> GetGOSTFromPositionName(string name);

        HashSet<string> GostsPostProcessor(HashSet<string> gosts);

        HashSet<string> RemoveLettersAndOtherSymbolsFromGosts(HashSet<string> gosts);
    }
}
