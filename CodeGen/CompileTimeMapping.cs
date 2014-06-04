using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mvvm;
using System.IO;

namespace Mvvm.CodeGen
{
    internal static class CompileTimeMapping
    {
        static Dictionary<Type, Type> mapping = new Dictionary<Type, Type>();
        static object _lock = new object();

        internal static Type Map(Type type)
        {
            return mapping.GetFromKeyOrCreate(type, _lock, () =>
            {
                var lookingFor = "{0}.Generated.{1}".FormatWith(type.Namespace, type.Name);
                var mappedType = Type.GetType(lookingFor);
                if (mappedType == null)
                    throw new IOException("Could not find a mapping type");
                mapping[type] = mappedType;
                return mappedType;
            });
        }

        internal static object New(Type t)
        {
            var mappedType = Map(t);
            var instance = Activator.CreateInstance(mappedType);
            return instance;
        }

        internal static T New<T>(Action<T> init = null)
        {
            var mappedType = Map(typeof(T));
            var instance = (T)Activator.CreateInstance(mappedType);
            if (init != null)
                init(instance);
            return instance;
        }
    }
}
