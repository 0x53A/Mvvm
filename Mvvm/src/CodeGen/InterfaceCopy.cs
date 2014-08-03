using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    public class InterfaceCopyException : Exception
    {
        public InterfaceCopyException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }

    internal class CopyContext
    {
        public CKey Key { get; private set; }
        public List<MethodInfo> Getters { get; private set; }
        public List<MethodInfo> Setters { get; private set; }

        public CopyContext(CKey key, IEnumerable<MethodInfo> getters, IEnumerable<MethodInfo> setters)
        {
            Key = key;
            Getters = getters.ToList();
            Setters = setters.ToList();
        }

        public struct CKey
        {
            public Type TCopy { get; private set; }
            public Type TSource { get; private set; }
            public Type TDestination { get; private set; }

            public CKey(Type tCopy, Type tSource, Type tDestination)
                : this()
            {
                TCopy = tCopy;
                TSource = tSource;
                TDestination = tDestination;
            }
        }
    }

    public static class InterfaceCopy
    {
        static object _lock = new object();
        static Dictionary<CopyContext.CKey, CopyContext> _contextCache = new Dictionary<CopyContext.CKey, CopyContext>();

        public static void Copy(Type tInterface, object source, object destination)
        {
            var tSource = source.GetType();
            var tDestination = destination.GetType();

            var key = new CopyContext.CKey(tInterface, tSource, tDestination);

            var context = _contextCache.GetFromKeyOrCreate(key, _lock, ()=>CreateContext(key));
            DoCopy(context, source, destination);
        }

        internal static CopyContext CreateContext(CopyContext.CKey key)
        {
            try
            {
                var getters = new List<MethodInfo>();
                var setters = new List<MethodInfo>();

                var props = key.TCopy.Flatten(t => t.GetTypeInfo().GetInterfaces()).Distinct().SelectMany(i => i.GetRuntimeProperties()).Distinct((a, b) => a.Name == b.Name && a.PropertyType == b.PropertyType);
                foreach (var p in props)
                {
                    var getter = key.TSource.GetRuntimeProperty(p.Name).GetGetMethod();
                    var setter = key.TDestination.GetRuntimeProperty(p.Name).GetSetMethod();
                    getters.Add(getter);
                    setters.Add(setter);
                }

                return new CopyContext(key, getters, setters);
            }
            catch (Exception ex)
            {
                throw new InterfaceCopyException("Some error occured getting getters and setters, see inner exception", ex);
            }
        }

        internal static void DoCopy(CopyContext context, object source, object destination)
        {
            try
            {
                Queue<object> values = new Queue<object>();
                foreach (var get in context.Getters)
                {
                    var val = get.Invoke(source, null);
                    values.Enqueue(val);
                }
                foreach (var set in context.Setters)
                {
                    var val = values.Dequeue();
                    set.Invoke(destination, new[] { val });
                }
            }
            catch (Exception ex)
            {
                throw new InterfaceCopyException("An Exception occured during copying, please see inner exception", ex);
            }
        }
    }
}
