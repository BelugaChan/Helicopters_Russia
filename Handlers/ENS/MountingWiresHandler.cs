using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    /// <summary>
    /// Провода монтажные
    /// </summary>
    public class MountingWiresHandler : IAdditionalENSHandler<MountingWiresHandler>
    {
        private Dictionary<string, string> wiresReplacements = new Dictionary<string, string>
        {
            { "КАБЕЛЬ","ПРОВОД" }
        };
        public string AdditionalStringHandle(string str)
        {
            foreach (var pair in wiresReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
