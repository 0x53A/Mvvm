#if !(UNIVERSAL||WINDOWS_PHONE)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    internal static class InterfaceImplementor
    {
        static IDictionary<Type, Type> mappedTypes = new Dictionary<Type, Type>();
        static object _lock = new object();

        /// <summary>
        /// See Generate
        /// </summary>
        internal static Type Map<T>()
        {
            //T must be a property-only interface
            Contract.Requires(typeof(T).IsInterface);
            Contract.Requires(typeof(T).GetMembers().All(_ => _.MemberType == MemberTypes.Property));
            //all get-only properties must have default constructors
            Contract.Requires(typeof(T).GetProperties().Where(p => p.CanRead && !p.CanWrite).All(p => p.PropertyType.GetConstructor(Type.EmptyTypes) != null));

            Type targetType = typeof(T);
            return Map(targetType);
        }

        /// <summary>
        /// See Generate
        /// </summary>
        internal static Type Map(Type targetType)
        {
            //T must be a property-only interface
            Contract.Requires(targetType.IsInterface);
            Contract.Requires(targetType.GetMembers().All(_ => _.MemberType == MemberTypes.Property));
            //all get-only properties must have default constructors
            Contract.Requires(targetType.GetProperties().Where(p => p.CanRead && !p.CanWrite).All(p => p.PropertyType.GetConstructor(Type.EmptyTypes) != null));

            return mappedTypes.GetFromKeyOrCreate(targetType, _lock, () => CreateInterfaceMap(targetType));
        }

        /// <summary>
        /// Constructs a new object of the Interface T,
        /// all get/set properties fire INotifyPropertyChanged.PropertyChanged
        /// all get-only properties are lazy initialized
        /// The Type is dynamically generated and cached, 
        /// so subsequent calls do not need to construct a new type (which is potentially costly)
        /// </summary>
        /// <typeparam name="T">The Interface which should be implemented</typeparam>
        /// <param name="initializer">optional: a function which initializes the newly constructed object</param>
        /// <returns>a newly constructed object</returns>
        internal static T Generate<T>(Action<T> initializer = null)
        {
            //T must be a property-only interface
            Contract.Requires(typeof(T).IsInterface);
            Contract.Requires(typeof(T).GetMembers().All(_ => _.MemberType == MemberTypes.Property));
            //all get-only properties must have default constructors
            Contract.Requires(typeof(T).GetProperties().Where(p => p.CanRead && !p.CanWrite).All(p => p.PropertyType.GetConstructor(Type.EmptyTypes) != null));
            //The returned value implements the interface INotifyPropertyChanged (but it is impossible to note that in the type system)
            Contract.Ensures(Contract.Result<T>() is INotifyPropertyChanged);

            var mappedType = Map<T>();
            var obj = (T)Activator.CreateInstance(mappedType);
            if (initializer != null)
                initializer(obj);
            return obj;
        }

        internal static object Generate(Type targetType)
        {
            //T must be a property-only interface
            Contract.Requires(targetType.IsInterface);
            Contract.Requires(targetType.GetMembers().All(_ => _.MemberType == MemberTypes.Property));
            //all get-only properties must have default constructors
            Contract.Requires(targetType.GetProperties().Where(p => p.CanRead && !p.CanWrite).All(p => p.PropertyType.GetConstructor(Type.EmptyTypes) != null));
            //The returned value implements the interface INotifyPropertyChanged (but it is impossible to note that in the type system)
            Contract.Ensures(Contract.Result<object>() is INotifyPropertyChanged);

            var mappedType = Map(targetType);
            var obj = Activator.CreateInstance(mappedType);
            return obj;
        }

        private static Type CreateInterfaceMap(Type targetType, bool? dump = false)
        {
            //T must be a property-only interface
            Contract.Requires(targetType.IsInterface);
            Contract.Requires(targetType.GetMembers().All(_ => _.MemberType == MemberTypes.Property));
            //all get-only properties must have default constructors
            Contract.Requires(targetType.GetProperties().Where(p => p.CanRead && !p.CanWrite).All(p => p.PropertyType.GetConstructor(Type.EmptyTypes) != null));

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

            var attr = targetType.GetCustomAttribute<TypeOverrideAttribute>();
            var name = attr != null ? attr.TypeName : targetType.Namespace;
            name = name ?? targetType.FullName;
            var tb = mb.DefineType(name, CodeGenInternal.GeneratedTypeAttributes, null, new[] { targetType, typeof(INotifyPropertyChanged) });

            var constructor = tb.DefineConstructor(CodeGenInternal.ConstructorAttributes, CallingConventions.HasThis, null);
            var ctorIL = constructor.GetILGenerator();

            var raiseMethod = CodeGenInternal.ImplementInpcFull(tb);

            foreach (var property in targetType.Flatten(t => t.GetInterfaces()).SelectMany(i => i.GetProperties()))
            {
                if (property.CanRead && property.CanWrite)
                    CodeGenInternal.CreateReadWriteProperty(tb, property, raiseMethod);
                else if (property.CanRead)
                    CodeGenInternal.CreateReadOnlyLazyProperty(tb, property, ctorIL);
            }

            //emit the default constructor
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, typeof(Object).GetConstructor(Type.EmptyTypes));
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
#endif