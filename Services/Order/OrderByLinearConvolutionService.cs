using System.Collections.Concurrent;
using Algo.Abstract;

namespace Algo.Services.Order
{
    public class OrderByLinearConvolutionService : OrderService
    {
        public override Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double)> bestStandart)
        {
            var bestStandartSnapshot = bestStandart.ToArray();

            var maxCommonTokensAmount = bestStandartSnapshot.Max(t => t.Value.Item2);
            var minCommonTokensAmount = bestStandartSnapshot.Min(t => t.Value.Item2);
            var normalizedCommonEls = new ConcurrentDictionary<TStandart, (double,double)>();
            foreach (var standart in bestStandartSnapshot)
            {
                if (standart.Value.Item1 == 1)
                {
                    normalizedCommonEls.TryAdd(standart.Key, (1, standart.Value.Item2));
                    continue;
                }
                double normalizedCommonEl;
                if (maxCommonTokensAmount == minCommonTokensAmount)
                    normalizedCommonEl = 1;                
                else
                    normalizedCommonEl = (standart.Value.Item2 - minCommonTokensAmount) / (maxCommonTokensAmount - minCommonTokensAmount);
                
                var linearCoeff = 0.3 * standart.Value.Item1 + 0.7 * normalizedCommonEl;
                normalizedCommonEls.TryAdd(standart.Key, (linearCoeff, standart.Value.Item2));
            }
            return normalizedCommonEls
                    .OrderByDescending(t => t.Value.Item1)
                    .ThenByDescending(t => t.Value.Item2)
                    .Take(3)
                    .ToDictionary(t => t.Key, t => t.Value);
        }
    }
}
