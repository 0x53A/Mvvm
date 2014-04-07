﻿#if WINDOWS_PHONE || UNIVERSAL
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
using System.Diagnostics;

namespace Mvvm.CodeGen
{
    public class CG
    {
        public static T New<T>(Action<T> init = null)
        {
#if !NO_RUNTIME_CODEGEN
            var t = typeof(T);
            Debug.Assert(t.GetCustomAttribute<TypeOverrideAttribute>() != null, "Attribute 'TypeOverrideAttribute' missing on type '{0}'".FormatWith(t.Name));
            if (t.IsInterface)
                return DBCGenerator.Generate<T>(init);
            else
                return VMWrapper.Wrap<T>(init);
#else
            throw new NotImplementedException();
#endif
        }

        public static Type Map(Type type)
        {
#if !NO_RUNTIME_CODEGEN
            Debug.Assert(type.GetCustomAttribute<TypeOverrideAttribute>() != null, "Attribute 'TypeOverrideAttribute' missing on type '{0}'".FormatWith(type.Name));
            if (type.IsInterface)
                return DBCGenerator.Map(type);
            else
                return VMWrapper.Map(type);
#else
            throw new NotImplementedException();
#endif
        }
    }
}
