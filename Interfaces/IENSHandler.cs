using Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces
{
    public interface IENSHandler
    {
        public abstract string AdditionalStringHandle(string str);
        string BaseStringHandle(string str);
    }
}
