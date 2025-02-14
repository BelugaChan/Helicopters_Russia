using AbstractionsAndModels.Interfaces.Models;
using NPOI.SS.Formula.Functions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Algo.Comparers
{
    public class StandartsComparer<TStandart> : IEqualityComparer<ConcurrentDictionary<TStandart, (double, double)>>
        where TStandart : IStandart
    {
        private Func<ConcurrentDictionary<TStandart, (double, double)>, ConcurrentDictionary<TStandart, (double, double)>, bool> compareFunction;
        private Func<ConcurrentDictionary<TStandart, (double, double)>, int> hashFunction;
        public StandartsComparer(Func<ConcurrentDictionary<TStandart, (double, double)>, ConcurrentDictionary<TStandart, (double, double)>, bool> compareFunction, Func<ConcurrentDictionary<TStandart, (double, double)>, int> hashFunction)
        {
            this.compareFunction = compareFunction;
            this.hashFunction = hashFunction;
        }

        public bool Equals(ConcurrentDictionary<TStandart, (double, double)>? x, ConcurrentDictionary<TStandart, (double, double)>? y)
        {
            return compareFunction(x, y);
        }

        public int GetHashCode([DisallowNull] ConcurrentDictionary<TStandart, (double, double)> obj)
        {
            return hashFunction(obj);
        }
    }
}
