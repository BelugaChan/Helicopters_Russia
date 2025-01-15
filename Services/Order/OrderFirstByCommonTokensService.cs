﻿using System.Collections.Concurrent;

namespace Algo.Services.Order
{
    public class OrderFirstByCommonTokensService : OrderService
    {
        public override Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double)> bestStandart)
        {
            var orderedStandarts = bestStandart
               .OrderByDescending(t => t.Value.Item2)
               .ThenByDescending(t => t.Value.Item1)
               .Take(3).ToList();

            return ProcessOrderedStandarts(orderedStandarts, (current, next) => Math.Abs(Math.Round(next.Item1, 4) - Math.Round(current.Item1, 4)) > 0.25);
        }
    }
}
