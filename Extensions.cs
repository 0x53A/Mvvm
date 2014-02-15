using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    /// <summary>
    /// Extensions to IList
    /// </summary>
    public static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var i in items)
                list.Add(i);
        }
    }

    /// <summary>
    /// Extensions to IEnumerable
    /// </summary>
    public static class  IEnumerableExtensions
    {
        private class EqualityComparer<T> : IEqualityComparer<T>
        {
            Func<T, T, bool> _comparisonFunc;
            public EqualityComparer(Func<T, T, bool> comparisonFunc)
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
    }

    /// <summary>
    /// Extensions to IDictionary
    /// </summary>
    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> ienum)
        {
            return ienum.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    /// <summary>
    /// Extensions to String
    /// </summary>
    public static class StringExtensions
    {
        public static string FormatWith(this string format, params object[] args)
        {
            return String.Format(format, args);
        }
    }
}
