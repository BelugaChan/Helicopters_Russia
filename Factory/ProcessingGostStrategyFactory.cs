using Algo.Interfaces.Factory;
using Algo.Interfaces.Strategy;
using Algo.MethodStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Factory
{
    public class ProcessingGostStrategyFactory : IProcessingGostStrategyFactory
    {
        public IGostStrategy GetStrategy(string gost)
        {
            return gost switch
            {
                "5920" => new Gost5950Strategy(),
                _ => throw new ArgumentException($"Неизвестный ГОСТ: {gost}")
            };
        }
    }
}
