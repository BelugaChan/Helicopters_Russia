using Algo.Interfaces.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Interfaces.Factory
{
    public interface IProcessingGostStrategyFactory
    {
        IGostStrategy GetStrategy(string gost);
    }
}
