using Algo.Abstract;
using Algo.Interfaces.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class LumberHandler :  ILumberHandler
    {
        protected static Dictionary<string, string> lumberReplacements = new Dictionary<string, string>
        {
            { "XBOЙH","XB" },
            { "XBOЙHЫЙ","XB" },
        };
        public string AdditionalStringHandle(string str)
        {
            foreach (var pair in lumberReplacements)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }
    }
}
