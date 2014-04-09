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
        internal static Type Map(Type type)
        {
            var lookingFor = "{0}.Generated.{1}".FormatWith(type.Namespace, type.Name);
            var mappedType = Type.GetType(lookingFor);
            if (mappedType == null)
                throw new IOException("Could not find a mapping type");
            return mappedType;
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
