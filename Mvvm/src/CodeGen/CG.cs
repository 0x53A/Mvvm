#if WINDOWS_PHONE || UNIVERSAL
#define NO_RUNTIME_CODEGEN
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Mvvm;
using System.IO;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace Mvvm.CodeGen
{
    public class CG
    {
        public static T New<T>(Action<T> init = null)
        {
            var t = typeof(T);
#if !NO_RUNTIME_CODEGEN
            if (t.IsInterface)
                return DBCGenerator.Generate<T>(init);
            else
                return VMWrapper.Wrap<T>(init);
#else
            Debug.Assert(t.GetTypeInfo().GetCustomAttribute<TypeOverrideAttribute>() != null, "Attribute 'TypeOverrideAttribute' missing on type '{0}'".FormatWith(t.Name));
            return CompileTimeMapping.New<T>(init);
#endif
        }

        public static Type Map(Type type)
        {
#if !NO_RUNTIME_CODEGEN
            if (type.IsInterface)
                return DBCGenerator.Map(type);
            else
                return VMWrapper.Map(type);
#else
            Debug.Assert(type.GetTypeInfo().GetCustomAttribute<TypeOverrideAttribute>() != null, "Attribute 'TypeOverrideAttribute' missing on type '{0}'".FormatWith(type.Name));
            return CompileTimeMapping.Map(type);
#endif
        }

        public static void Copy<TCopy>(object source, object destination)
        {
            Contract.Requires(typeof(TCopy).GetTypeInfo().IsInterface);
            Contract.Requires(source != null);
            Contract.Requires(destination != null);
        }
    }
}
