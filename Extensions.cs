using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        public static IEnumerable<T2> SelectMany<T1,T2>(this IEnumerable<T1> self) where T1 : IEnumerable<T2>
        {
            var res = self.SelectMany(x => x);
            return res;
        }

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

    /// <summary>
    /// Extensions to ease the xml (de)serialization
    /// </summary>
    public static class XmlSerializationExtensions
    {
        /// <summary>Serializes an object of type T in to an xml string</summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <returns>A string that represents Xml, empty otherwise</returns>
        public static string XmlSerialize<T>(this T obj) where T : class, new()
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        /// <summary>Deserializes an xml string in to an object of Type T</summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="xml">Xml as string to deserialize from</param>
        /// <returns>A new object of type T is successful, null if failed</returns>
        public static T XmlDeserialize<T>(this string xml) where T : class, new()
        {
            if (xml == null) throw new ArgumentNullException("xml");

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                try { return (T)serializer.Deserialize(reader); }
                catch { return null; } // Could not be deserialized to this type.
            }
        }
    }
}
