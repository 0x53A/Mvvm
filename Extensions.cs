﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    /// <summary>
    /// Extensions to Stream
    /// </summary>
    public static class StreamExtensions
    {
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }

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
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TNode> Flatten<TNode>(this TNode source, Func<TNode, IEnumerable<TNode>> extractChildNodes)
        {
            var stack = new Stack<TNode>();
            stack.Push(source);

            while (stack.Count > 0)
            {
                TNode item = stack.Pop();
                yield return item;
                foreach (var child in extractChildNodes(item))
                    stack.Push(child);

            }
        }

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
