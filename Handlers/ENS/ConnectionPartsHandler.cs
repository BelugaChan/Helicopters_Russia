using Algo.Interfaces.Handlers.ENS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class ConnectionPartsHandler : IAdditionalENSHandler<ConnectionPartsHandler>
    {
        /// <summary>
        /// Части соединительные
        /// </summary>
        private Dictionary<string, string> connectionReplacements = new Dictionary<string, string>
        {
            { "КОНТРАГАЙКА ", "КОНТРГАЙКА "},
        };
        public string AdditionalStringHandle(string str)
        {
            foreach (var pair in connectionReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
