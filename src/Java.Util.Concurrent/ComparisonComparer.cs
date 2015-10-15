using System;
using System.Collections.Generic;

namespace Java.Util.Concurrent
{
    [Serializable]
    internal class ComparisonComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> _comparison;

        internal ComparisonComparer(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException("comparison");
            _comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }

        internal static IComparer<T> From(Comparison<T> comparison)
        {
            return comparison == null ? null : new ComparisonComparer<T>(comparison);
        }
    }
}