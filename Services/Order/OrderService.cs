using System.Collections.Concurrent;

namespace Algo.Services.Order
{
    public abstract class OrderService
    {
        public virtual Dictionary<TStandart, double> GetBestStandarts<TStandart>(Dictionary<TStandart, double> bestStandart)
        {
            // Упорядочиваем элементы и берём топ-3
            var orderedStandarts = bestStandart
                .OrderByDescending(t => t.Value)
                .Take(3)
                .ToList(); // Преобразуем в список для эффективного индексирования

            return ProcessOrderedStandarts(orderedStandarts, (currentCoeff, nextCoeff) => Math.Abs(Math.Round(nextCoeff, 4) - Math.Round(currentCoeff, 4)) > 0.2);
        }

        public abstract Dictionary<TStandart, (double, double)> GetBestStandarts<TStandart>(ConcurrentDictionary<TStandart, (double, double)> bestStandart);
        public virtual Dictionary<TStandart, TValue> ProcessOrderedStandarts<TStandart, TValue>(
            List<KeyValuePair<TStandart, TValue>> orderedStandarts,
            Func<TValue, TValue, bool> differenceCondition)
        {
            var result = new Dictionary<TStandart, TValue>();
            bool addedFirst = false;

            for (int i = 0; i < orderedStandarts.Count; i++)
            {
                var currentCoeff = orderedStandarts[i].Value;

                // Проверяем разницу с коэффициентом следующего элемента
                if (i < orderedStandarts.Count - 1)
                {
                    var nextCoeff = orderedStandarts[i + 1].Value;
                    if (differenceCondition(currentCoeff, nextCoeff)/*Math.Abs(Math.Round(nextCoeff, 4) - Math.Round(currentCoeff, 4)) > 0.2*/)
                    {
                        addedFirst = true;
                        result[orderedStandarts[i].Key] = currentCoeff;
                        break;
                    }
                }

                if (!addedFirst)
                    result[orderedStandarts[i].Key] = currentCoeff;
            }

            return result;
        }
    }
}
