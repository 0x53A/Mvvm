using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    /// <summary>
    /// Notifies the VMWrapper that this Property should be overridden and implement INotifyPropertyChanged.
    /// It can only be applied to properties with getter and setter
    /// </summary>
    public class InpcAttribute : Attribute
    {

    }

    /// <summary>
    /// Notifies the VMWrapper that this property should be overridden and implemented using Lazy initialization.
    /// It can only be applied to get-only properties
    /// </summary>
    public class LazyAttribute : Attribute
    {

    }

    public static class VMWrapper
    {
        static IDictionary<Type, Type> mappedTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// DO NOT USE
        /// </summary>
        public static void ClearCache()
        {
            lock (mappedTypes)
            {
                mappedTypes.Clear();
            }
        }

        public static Type Map<T>()
        {
            //TODO: Contract.Requires

            Type targetType = typeof(T);
            return Map(targetType);
        }

        public static Type Map(Type targetType)
        {
            //TODO: Contract.Requires

            Type mappedType;

            if (mappedTypes.ContainsKey(targetType))
                mappedType = mappedTypes[targetType];
            else
            {
                lock (mappedTypes)
                {
                    //it may have been added between the first check and the lock....
                    if (mappedTypes.ContainsKey(targetType))
                    {
                        mappedType = mappedTypes[targetType];
                    }
                    else
                    {
                        mappedType = CreateClassMap(targetType);
                        mappedTypes.Add(targetType, mappedType);
                    }
                }
            }

            return mappedType;
        }

        public static T Wrap<T>(Action<T> initializer = null)
        {
            //TODO: Contract.Requires

            var mappedType = Map<T>();
            var obj = (T)Activator.CreateInstance(mappedType);
            if (initializer != null)
                initializer(obj);
            return obj;
        }

        public static object Wrap(Type targetType)
        {
            //TODO: Contract.Requires

            var mappedType = Map(targetType);
            var obj = Activator.CreateInstance(mappedType);
            return obj;
        }

        private static Type CreateClassMap(Type targetType, bool? dump = null)
        {
            //TODO: Contract.Requires

            if (!dump.HasValue)
            {
#if DEBUG
                dump = true;
#else
                dump = false;
#endif
            }

            var access = (dump == true) ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run;
            var assemblyName = "assembly_{0}_{1}".FormatWith(targetType.FullName, Guid.NewGuid());
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), access);

            ModuleBuilder mb;
            if (dump == true)
                mb = ab.DefineDynamicModule("generated", "{0}.mod.dll".FormatWith(assemblyName), true);
            else
                mb = ab.DefineDynamicModule("generated");

            bool hasINPC = targetType.GetInterfaces().Contains(typeof(INotifyPropertyChanged));
            var interfacesToImplement = hasINPC ? Type.EmptyTypes : new[] { typeof(INotifyPropertyChanged) };
            var tb = mb.DefineType("dbc_{0}".FormatWith(targetType.FullName), CodeGenInternal.GeneratedTypeAttributes, targetType, interfacesToImplement);

            var constructor = tb.DefineConstructor(CodeGenInternal.ConstructorAttributes, CallingConventions.HasThis, null);
            var ctorIL = constructor.GetILGenerator();

            var raiseMethod = CodeGenInternal.ImplementINPC(tb, !hasINPC);

            foreach (var property in targetType.GetProperties())
            {
                if (property.CanRead && property.CanWrite && property.CustomAttributes.Any(a=>a.AttributeType==typeof(InpcAttribute)))
                    CodeGenInternal.CreateReadWriteProperty(tb, property, raiseMethod, false);
                else if (property.CanRead && !property.CanWrite && property.CustomAttributes.Any(a => a.AttributeType == typeof(LazyAttribute)))
                    CodeGenInternal.CreateReadOnlyLazyProperty(tb, property, ctorIL, false);
            }

            //emit the default constructor
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, targetType.GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Nop);
            ctorIL.Emit(OpCodes.Ret);

            //create and return type
            var type = tb.CreateType();

            if (dump == true)
                ab.Save("{0}.dll".FormatWith(assemblyName));

            return type;
        }
    }
}
