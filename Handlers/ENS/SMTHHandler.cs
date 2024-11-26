using Algo.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Algo.Handlers.ENS
{
    public class SMTHHandler : ENSHandler
    {
        public override string AdditionalStringHandle(string str)
        {
            return str; //заглушка
        }
    }
}
