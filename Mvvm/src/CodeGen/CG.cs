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
        public static object New(Type t)
        {
#if !NO_RUNTIME_CODEGEN
            if (t.IsInterface)
                return InterfaceImplementor.Generate(t);
            else
                return AbstractClassImplementor.Wrap(t);
#else
            Debug.Assert(t.GetTypeInfo().GetCustomAttribute<TypeOverrideAttribute>() != null, "Attribute 'TypeOverrideAttribute' missing on type '{0}'".FormatWith(t.Name));
            return CompileTimeMapping.New(t);
#endif
        }

        public static T New<T>(Action<T> init = null)
        {
            var t = typeof(T);
#if !NO_RUNTIME_CODEGEN
            if (t.IsInterface)
                return InterfaceImplementor.Generate<T>(init);
            else
                return AbstractClassImplementor.Wrap<T>(init);
#else
            Debug.Assert(t.GetTypeInfo().GetCustomAttribute<TypeOverrideAttribute>() != null, "Attribute 'TypeOverrideAttribute' missing on type '{0}'".FormatWith(t.Name));
            return CompileTimeMapping.New<T>(init);
#endif
        }

        public static Type Map(Type type)
        {
#if !NO_RUNTIME_CODEGEN
            if (type.IsInterface)
                return InterfaceImplementor.Map(type);
            else
                return AbstractClassImplementor.Map(type);
#else
            Debug.Assert(type.GetTypeInfo().GetCustomAttribute<TypeOverrideAttribute>() != null, "Attribute 'TypeOverrideAttribute' missing on type '{0}'".FormatWith(type.Name));
            return CompileTimeMapping.Map(type);
#endif
        }

        public static void Copy<TCopy>(object source, object destination) where TCopy : class
        {
            Contract.Requires(typeof(TCopy).GetTypeInfo().IsInterface);
            Contract.Requires(source != null);
            Contract.Requires(destination != null);

            InterfaceCopy.Copy(typeof(TCopy), source, destination);
        }

        public static void Copy(Type tCopy, object source, object destination)
        {
            Contract.Requires(tCopy.GetTypeInfo().IsInterface);
            Contract.Requires(source != null);
            Contract.Requires(destination != null);

            InterfaceCopy.Copy(tCopy, source, destination);
        }
    }
}
