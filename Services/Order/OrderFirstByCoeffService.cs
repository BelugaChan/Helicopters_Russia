using System.Collections.Concurrent;
using AbstractionsAndModels.Abstract;

namespace Algo.Services.Order
{
    public class OrderFirstByCoeffService : OrderService
    {

        public override Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double)> bestStandart)
        {
            var orderedStandarts = bestStandart
                .OrderByDescending(t => t.Value.Item1)
                .ThenByDescending(t => t.Value.Item2)
                .Take(3).ToList();

            return ProcessOrderedStandarts(orderedStandarts, (current, next) => Math.Abs(Math.Round(next.Item1, 4) - Math.Round(current.Item1, 4)) > 0.25);
        }

        /// <summary>
        /// This method is not implemented in this class
        /// </summary>
        /// <typeparam name="TStandart"></typeparam>
        /// <param name="bestStandart"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double, double)> bestStandart)
            => throw new NotImplementedException();
    }
}
