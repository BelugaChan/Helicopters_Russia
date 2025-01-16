using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algo.Abstract;

namespace Algo.Services.Order
{
    public class OrderFirstByCommonElsCountService : OrderService
    {
        public override Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double)> bestStandart)
        {
            var orderedStandarts = bestStandart
               .OrderByDescending(t => t.Value.Item2)
               .ThenByDescending(t => t.Value.Item1).ToDictionary()
               .Take(3).ToDictionary();

            //var maxCommonTokensAmount = bestStandart.Max(t => t.Value.Item2);
            //var normalizedCommonEls = new Dictionary<TStandart, double>();
            //foreach (var standart in bestStandart) 
            //{
            //    var normalizedCommonEl = 1 - bestStandart.Count / maxCommonTokensAmount;
            //    var linearCoeff = 0.6 * standart.Value.Item1 + 0.4 * normalizedCommonEl; 
            //    normalizedCommonEls.Add(standart.Key, linearCoeff);
            //}
            //return ProcessOrderedStandarts(orderedStandarts, (current, next) => Math.Abs(Math.Round(next.Item1, 4) - Math.Round(current.Item1, 4)) > 0.25);
            return orderedStandarts;
        }
    }
}
