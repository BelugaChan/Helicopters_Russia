using AbstractionsAndModels.Abstract;
using Algo.Comparers;
using System.Collections.Concurrent;

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
                double normalizedCommonEl;
                if (maxCommonTokensAmount == minCommonTokensAmount)
                    normalizedCommonEl = 1;                
                else
                    normalizedCommonEl = (standart.Value.Item2 - minCommonTokensAmount) / (maxCommonTokensAmount - minCommonTokensAmount);
                
                var linearCoeff = 0.3 * standart.Value.Item1 + 0.7 * normalizedCommonEl;
                normalizedCommonEls.TryAdd(standart.Key, (linearCoeff, standart.Value.Item2));
            }
            //var comparer = new StandartsComparer<TStandart>((x,y) => x Name==y.Name,s => s.Name.GetHashCode());


            var res = normalizedCommonEls
                    .OrderByDescending(t => t.Value.Item1)
                    .ThenByDescending(t => t.Value.Item2)
                    .Take(3)
                    .ToDictionary(t => t.Key, t => t.Value);
            return res;
            //return ProcessOrderedStandartsBestOfTheBest(res, (cur, bestOfTheBest) => bestOfTheBest.Item1 - Math.Round(cur.Item1, 4) > 0.07);
        }

        public override Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double, double)> bestStandart)
        {
            var bestStandartSnapshot = bestStandart.ToArray();

            var maxCommonTokensAmountFromGost = bestStandartSnapshot.Max(t => t.Value.Item2);
            var minCommonTokensAmountFromGost = bestStandartSnapshot.Min(t => t.Value.Item2);

            var maxCommonTokensAmountFromName = bestStandartSnapshot.Max(t => t.Value.Item3);
            var minCommonTokensAmountFromName = bestStandartSnapshot.Min(t => t.Value.Item3);

            var normalizedCommonEls = new ConcurrentDictionary<TStandart, (double, double)>();

            foreach (var standart in bestStandartSnapshot)
            {
                double normalizedCommonElName;
                double normalizedCommonElGost;
                if (maxCommonTokensAmountFromGost == minCommonTokensAmountFromGost)
                    normalizedCommonElGost = 1;
                else
                    normalizedCommonElGost = (standart.Value.Item2 - minCommonTokensAmountFromGost) / (maxCommonTokensAmountFromGost - minCommonTokensAmountFromGost);
               
                if (maxCommonTokensAmountFromName == minCommonTokensAmountFromName)
                    normalizedCommonElName = 1;
                else
                    normalizedCommonElName = (standart.Value.Item3 - minCommonTokensAmountFromName) / (maxCommonTokensAmountFromName - minCommonTokensAmountFromName);


                var linearCoeff = 0.2 * standart.Value.Item1 + 0.4 * normalizedCommonElName + 0.4 * normalizedCommonElGost;
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
