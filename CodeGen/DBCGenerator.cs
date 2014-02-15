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
        /// with all properties implementing INotifyPropertyChanged
        /// </summary>
        /// <typeparam name="T">The Interface which should be implemented</typeparam>
        /// <returns>a newly constructed object</returns>
        public static T Generate<T>() where T : class
        {
            //T must be an property-only interface
            Contract.Requires(typeof(T).IsInterface);
            Contract.Requires(typeof(T).GetMembers().All(_ => _.MemberType == MemberTypes.Property));
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

        static Type CreateType(Type targetType)
        {
            var assemblyName = "assembly_{0}_{1}".FormatWith(targetType.FullName, Guid.NewGuid());
#if DEBUG
            var access = AssemblyBuilderAccess.RunAndSave;
#else
            var access = AssemblyBuilderAccess.Run
#endif
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), access);
            var mb = ab.DefineDynamicModule("generated");
            var tb = mb.DefineType("dbc_{0}".FormatWith(targetType.FullName),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                null, new[] { targetType, typeof(INotifyPropertyChanged) });

            var constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            var raiseMethod = ImplementINPC(tb);

            foreach (var property in targetType.GetProperties())
                CreateProperty(tb, property);

            var type = tb.CreateType();
#if DEBUG
            ab.Save("{0}.dll".FormatWith(assemblyName));
#endif
            return type;
        }

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

            MethodBuilder addremoveMethod = typeBuilder.DefineMethod(prefix + "PropertyChanged",
               MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.NewSlot |
               MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final,
               null, new[] { typeof(PropertyChangedEventHandler) });

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
            MethodBuilder raisePropertyChangedBuilder = typeBuilder.DefineMethod("RaisePropertyChanged",
                MethodAttributes.Family | MethodAttributes.Virtual, null, new Type[] { typeof(string) });

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

        static void CreateProperty(TypeBuilder tb, PropertyInfo property)
        {
            var propertyName = property.Name;
            var propertyType = property.PropertyType;

            var fb = tb.DefineField("_backing_" + propertyName, propertyType, FieldAttributes.Private);

            var pb = tb.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);
            var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            var getMethod = tb.DefineMethod("get_" + propertyName, attributes, propertyType, Type.EmptyTypes);
            var getIL = getMethod.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fb);
            getIL.Emit(OpCodes.Ret);

            var setMethod = tb.DefineMethod("set_" + propertyName, attributes, null, new[] { propertyType });

            var setIL = setMethod.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fb);
            setIL.Emit(OpCodes.Ret);

            pb.SetGetMethod(getMethod);
            pb.SetSetMethod(setMethod);
        }
    }
}
