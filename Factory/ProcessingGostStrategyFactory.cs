using AbstractionsAndModels.Interfaces.Factory;
using AbstractionsAndModels.Interfaces.Strategy;
using Algo.MethodStrategy.Circles;

namespace Algo.Factory
{
    public class ProcessingGostStrategyFactory : IProcessingGostStrategyFactory
    {
        private readonly Dictionary<Func<string, bool>, IGostStrategy> strategies = new();
        public ProcessingGostStrategyFactory()
            => Register(gost => gost.Contains("5950"), new Gost5950Strategy());

        //public IGostStrategy GetStrategy(string gost)
        //{
        //    return gost switch
        //    {
        //        "5950" => new Gost5950Strategy(),
        //        _ => throw new ArgumentException($"Неизвестный ГОСТ: {gost}")
        //    };
        //}
        public IGostStrategy GetStrategy(string gost)
        {
            foreach (var entry in strategies)
            {
                if (entry.Key(gost))
                {
                    return entry.Value;
                }
            }
            throw new ArgumentException($"Неизвестный ГОСТ: {gost}");
        }

        private void Register(Func<string, bool> condition, IGostStrategy strategy)
            => strategies[condition] = strategy;

    }
}
