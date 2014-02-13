using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public static class Extensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var i in items)
                list.Add(i);

        }

        private class EqualityComparer<T> : IEqualityComparer<T>
        {
            Func<T, T, bool> _comparisonFunc;
            public EqualityComparer(Func<T,T,bool> comparisonFunc)
            {
                _comparisonFunc = comparisonFunc;
            }

            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                return _comparisonFunc(x, y);
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, bool> comparisonFunc)
        {
            return source.Distinct(new EqualityComparer<TSource>(comparisonFunc));
        }

        public static IDictionary<TKey, TVal> ToDictionary<TKey,TVal>(this IEnumerable<KeyValuePair<TKey,TVal>> ienum)
        {
            return ienum.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
