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
    public static class DBCGenerator
    {
        static IDictionary<Type, Type> mappedTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// Constructs a new object of the Interface T,
        /// all get/set properties fire INotifyPropertyChanged.PropertyChanged
        /// all get-only properties are lazy initialized
        /// The Type is dynamically generated and cached
        /// </summary>
        /// <typeparam name="T">The Interface which should be implemented</typeparam>
        /// <returns>a newly constructed object</returns>
        public static T Generate<T>() where T : class
        {
            //T must be a property-only interface
            Contract.Requires(typeof(T).IsInterface);
            Contract.Requires(typeof(T).GetMembers().All(_ => _.MemberType == MemberTypes.Property));
            //all get-only properties must have default constructors
            Contract.Requires(typeof(T).GetProperties().Where(p => p.CanRead && !p.CanWrite).All(p => p.PropertyType.GetConstructor(Type.EmptyTypes) != null));
            //The returned value implements the interface INotifyPropertyChanged (but it is impossible to note that in the type system)
            Contract.Ensures(Contract.Result<T>() is INotifyPropertyChanged);

            Type targetType = typeof(T);
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
                        mappedType = CreateType(targetType);
                        mappedTypes.Add(targetType, mappedType);
                    }
                }
            }
            return (T)Activator.CreateInstance(mappedType);
        }

        /// <summary>
        /// the attributes of the generated type
        /// </summary>
        const TypeAttributes GeneratedTypeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout;

        /// <summary>
        /// the attributes of a normal public .ctor
        /// </summary>
        const MethodAttributes ConstructorAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;

        /// <summary>
        /// Attributes for a method implementing an interface
        /// </summary>
        const MethodAttributes InterfaceImplementationAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.NewSlot |
               MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;

        /// <summary>
        /// the Attributes for a normal, nonvirtual instance method
        /// </summary>
        const MethodAttributes PrivateInstanceMethodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;

        static Type CreateType(Type targetType)
        {
            var assemblyName = "assembly_{0}_{1}".FormatWith(targetType.FullName, Guid.NewGuid());
#if DEBUG
            var access = AssemblyBuilderAccess.RunAndSave;
#else
            var access = AssemblyBuilderAccess.Run
#endif
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), access);
#if DEBUG
            var mb = ab.DefineDynamicModule("generated", "{0}.mod.dll".FormatWith(assemblyName), true);
#else
            var mb = ab.DefineDynamicModule("generated");
#endif
            var tb = mb.DefineType("dbc_{0}".FormatWith(targetType.FullName), GeneratedTypeAttributes, null, new[] { targetType, typeof(INotifyPropertyChanged) });

            var constructor = tb.DefineConstructor(ConstructorAttributes, CallingConventions.HasThis, null);
            var ctorIL = constructor.GetILGenerator();

            var raiseMethod = ImplementINPC(tb);

            foreach (var property in targetType.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                    CreateReadWriteProperty(tb, property);
                else if (property.CanRead)
                    CreateReadOnlyLazyProperty(tb, property, ctorIL);
            }

            //emit the default constructor
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, typeof(Object).GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Nop);
            ctorIL.Emit(OpCodes.Ret);

            //create and return type
            var type = tb.CreateType();
#if DEBUG
            ab.Save("{0}.dll".FormatWith(assemblyName));
#endif
            return type;
        }

        #region INotifyPropertyChanged

        static MethodBuilder ImplementINPC(TypeBuilder tb)
        {
            var eventField = CreatePropertyChangedEvent(tb);
            var raiseMethod = CreateRaisePropertyChanged(tb, eventField);
            return raiseMethod;
        }

        static FieldBuilder CreatePropertyChangedEvent(TypeBuilder typeBuilder)
        {
            // public event PropertyChangedEventHandler PropertyChanged;

            FieldBuilder eventField = typeBuilder.DefineField("PropertyChanged", typeof(PropertyChangedEventHandler), FieldAttributes.Private);
            EventBuilder eventBuilder = typeBuilder.DefineEvent("PropertyChanged", EventAttributes.None, typeof(PropertyChangedEventHandler));

            eventBuilder.SetAddOnMethod(CreateAddRemoveMethod(typeBuilder, eventField, true));
            eventBuilder.SetRemoveOnMethod(CreateAddRemoveMethod(typeBuilder, eventField, false));

            return eventField;
        }

        static MethodBuilder CreateAddRemoveMethod(TypeBuilder typeBuilder, FieldBuilder eventField, bool isAdd)
        {
            string prefix = isAdd ? "add_" : "remove_";
            string delegateAction = isAdd ? "Combine" : "Remove";

            MethodBuilder addremoveMethod = typeBuilder.DefineMethod(prefix + "PropertyChanged", InterfaceImplementationAttributes, null, new[] { typeof(PropertyChangedEventHandler) });

            MethodImplAttributes eventMethodFlags = MethodImplAttributes.Managed | MethodImplAttributes.Synchronized;
            addremoveMethod.SetImplementationFlags(eventMethodFlags);

            ILGenerator ilGen = addremoveMethod.GetILGenerator();

            // PropertyChanged += value; // PropertyChanged -= value;
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, eventField);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.EmitCall(OpCodes.Call, typeof(Delegate).GetMethod(delegateAction, new[] { typeof(Delegate), typeof(Delegate) }), null);
            ilGen.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
            ilGen.Emit(OpCodes.Stfld, eventField);
            ilGen.Emit(OpCodes.Ret);

            MethodInfo intAddRemoveMethod = typeof(INotifyPropertyChanged).GetMethod(prefix + "PropertyChanged");
            typeBuilder.DefineMethodOverride(addremoveMethod, intAddRemoveMethod);

            return addremoveMethod;
        }

        private static MethodBuilder CreateRaisePropertyChanged(TypeBuilder typeBuilder, FieldBuilder eventField)
        {
            MethodBuilder raisePropertyChangedBuilder = typeBuilder.DefineMethod("RaisePropertyChanged", PrivateInstanceMethodAttributes, null, new Type[] { typeof(string) });

            ILGenerator raisePropertyChangedIl = raisePropertyChangedBuilder.GetILGenerator();
            Label labelExit = raisePropertyChangedIl.DefineLabel();

            // if (PropertyChanged == null)
            // {
            //      return;
            // }
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_0);
            raisePropertyChangedIl.Emit(OpCodes.Ldfld, eventField);
            raisePropertyChangedIl.Emit(OpCodes.Ldnull);
            raisePropertyChangedIl.Emit(OpCodes.Ceq);
            raisePropertyChangedIl.Emit(OpCodes.Brtrue, labelExit);

            // this.PropertyChanged(this,
            // new PropertyChangedEventArgs(propertyName));
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_0);
            raisePropertyChangedIl.Emit(OpCodes.Ldfld, eventField);
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_0);
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_1);
            raisePropertyChangedIl.Emit(OpCodes.Newobj, typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) }));
            raisePropertyChangedIl.EmitCall(OpCodes.Callvirt, typeof(PropertyChangedEventHandler).GetMethod("Invoke"), null);

            // return;
            raisePropertyChangedIl.MarkLabel(labelExit);
            raisePropertyChangedIl.Emit(OpCodes.Ret);

            return raisePropertyChangedBuilder;
        }

        #endregion

        static void CreateReadOnlyLazyProperty(TypeBuilder tb, PropertyInfo property, ILGenerator ctor)
        {
            var propertyName = property.Name;
            var propertyType = property.PropertyType;

            //backing field
            var lazyType = typeof(Lazy<>).MakeGenericType(propertyType);
            var fb = tb.DefineField("_backing_" + propertyName, lazyType, FieldAttributes.Private);

            //property
            var pb = tb.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

            //getter
            var getMethod = tb.DefineMethod("get_" + propertyName, InterfaceImplementationAttributes, propertyType, Type.EmptyTypes);
            var getIL = getMethod.GetILGenerator();
            getIL.DeclareLocal(propertyType);
            getIL.Emit(OpCodes.Nop);
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fb);
            getIL.EmitCall(OpCodes.Callvirt, lazyType.GetProperty("Value").GetGetMethod(), null);
            getIL.Emit(OpCodes.Stloc_0);
            getIL.Emit(OpCodes.Nop); //originally a br.s to the next line
            getIL.Emit(OpCodes.Ldloc_0);
            getIL.Emit(OpCodes.Ret);

            //combine getter and property
            pb.SetGetMethod(getMethod);

            //initialize Lazy<T> in .ctor
            ctor.Emit(OpCodes.Ldarg_0);
            ctor.Emit(OpCodes.Newobj, lazyType.GetConstructor(Type.EmptyTypes));
            ctor.Emit(OpCodes.Stfld, fb);
        }

        static void CreateReadWriteProperty(TypeBuilder tb, PropertyInfo property)
        {
            var propertyName = property.Name;
            var propertyType = property.PropertyType;

            //backing
            var fb = tb.DefineField("_backing_" + propertyName, propertyType, FieldAttributes.Private);

            //property
            var pb = tb.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

            //getter
            var getMethod = tb.DefineMethod("get_" + propertyName, InterfaceImplementationAttributes, propertyType, Type.EmptyTypes);
            var getIL = getMethod.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fb);
            getIL.Emit(OpCodes.Ret);

            //setter
            var setMethod = tb.DefineMethod("set_" + propertyName, InterfaceImplementationAttributes, null, new[] { propertyType });

            var setIL = setMethod.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fb);
            setIL.Emit(OpCodes.Ret);

            //combine property, getter, setter 
            pb.SetGetMethod(getMethod);
            pb.SetSetMethod(setMethod);
        }
    }
}
