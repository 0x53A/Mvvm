using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.App
{
    public static class SimpleContainer
    {
        static Dictionary<Type, object> mapping = new Dictionary<Type, object>();

        public static void Register<T>(Func<T> getter)
        {
            lock (mapping)
                mapping[typeof(T)] = getter;
        }

        public static T Get<T>()
        {
            lock (mapping)
                return (mapping[typeof(T)] as Func<T>)();
        }
    }
}
