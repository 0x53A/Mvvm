using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    public static class CodeGenHelper
    {
        public static MethodInfo GetMethod<T>(Func<T> del) { return del.Method; }
        public static MethodInfo GetMethod<T>(Action<T> del) { return del.Method; }
        public static MethodInfo GetMethod<T1, T2>(Func<T1, T2> del) { return del.Method; }
        public static MethodInfo GetMethod<T1, T2>(Action<T1, T2> del) { return del.Method; }
        public static MethodInfo GetMethod<T1, T2, T3>(Func<T1, T2, T3> del) { return del.Method; }
        public static MethodInfo GetMethod<T1, T2, T3>(Action<T1, T2, T3> del) { return del.Method; }
        public static MethodInfo GetMethod<T1, T2, T3, T4>(Func<T1, T2, T3, T4> del) { return del.Method; }
        public static MethodInfo GetMethod<T1, T2, T3, T4>(Action<T1, T2, T3, T4> del) { return del.Method; }
    }

    internal class CodeGenInternal
    {
        /// <summary>
        /// the attributes of the generated type
        /// </summary>
        internal const TypeAttributes GeneratedTypeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout;

        /// <summary>
        /// the attributes of a normal public .ctor
        /// </summary>
        internal const MethodAttributes ConstructorAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;

        /// <summary>
        /// Attributes for a method implementing an interface
        /// </summary>
        internal const MethodAttributes InterfaceImplementationAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.NewSlot |
               MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;

        /// <summary>
        /// Attributes for a method/property overriding a virtual/abstract base
        /// </summary>
        internal const MethodAttributes OverrideAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;

        /// <summary>
        /// the Attributes for a normal, nonvirtual instance method
        /// </summary>
        internal const MethodAttributes PrivateInstanceMethodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig;

        #region INotifyPropertyChanged

        /// <summary>
        /// Implements the interface INotifyPropertyChanged on the specified type
        /// and returns a handle to the raise method
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="full">whether the full INPC (event,delegate,add,remove) should be implemented or only the raise-method</param>
        /// <returns>MethodBuilder for the raise-method</returns>
        internal static MethodInfo ImplementINPC(TypeBuilder tb, bool full = true)
        {
            if (full)
            {
                var eventField = CreatePropertyChangedEvent(tb);
                var raiseMethod = CreateRaisePropertyChanged(tb, eventField);
                return raiseMethod;
            }
            else
            {
                throw new NotImplementedException();
                //TODO: implement
                var raiseMethod = CreateBaseRaise(tb);
                return raiseMethod;
            }
        }

        private static MethodBuilder CreateBaseRaise(TypeBuilder typeBuilder)
        {

            MethodBuilder raisePropertyChangedBuilder = typeBuilder.DefineMethod("RaiseBasePropertyChanged", PrivateInstanceMethodAttributes, null, new Type[] { typeof(string) });

            ILGenerator il = raisePropertyChangedBuilder.GetILGenerator();
            //TODO: implement
            il.Emit(OpCodes.Ret);

            return raisePropertyChangedBuilder;
        }

        /************************************************************************/
        /* This needs to be implemented inside the dynamic class*****************/
        /************************************************************************/

        private FieldInfo FindEventField(Type type)
        {
            var field = type.GetField("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                if (type.BaseType != typeof(object))
                    return FindEventField(type.BaseType);
                else
                    throw new Exception();
            }
            else
                return field;
        }

        private void RaiseBasePropertyChanged(string property)
        {
            var eventField = FindEventField(this.GetType());
            if (eventField == null)
                throw new InvalidOperationException("could not found the event field of the base class");
            var eventFieldValue = (PropertyChangedEventHandler)eventField.GetValue(this);
            eventFieldValue.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /************************************************************************/
        /************************************************************************/

        private static FieldBuilder CreatePropertyChangedEvent(TypeBuilder typeBuilder)
        {
            // public event PropertyChangedEventHandler PropertyChanged;

            FieldBuilder eventField = typeBuilder.DefineField("PropertyChanged", typeof(PropertyChangedEventHandler), FieldAttributes.Private);
            EventBuilder eventBuilder = typeBuilder.DefineEvent("PropertyChanged", EventAttributes.None, typeof(PropertyChangedEventHandler));

            eventBuilder.SetAddOnMethod(CreateAddRemoveMethod(typeBuilder, eventField, true));
            eventBuilder.SetRemoveOnMethod(CreateAddRemoveMethod(typeBuilder, eventField, false));

            return eventField;
        }

        private static MethodBuilder CreateAddRemoveMethod(TypeBuilder typeBuilder, FieldBuilder eventField, bool isAdd)
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

        private static MethodBuilder CreateRaisePropertyChanged(TypeBuilder typeBuilder, FieldInfo eventField)
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

        internal static void CreateReadOnlyLazyProperty(TypeBuilder tb, PropertyInfo property, ILGenerator ctor, bool isInterfaceImplementation = true)
        {
            var propertyName = property.Name;
            var propertyType = property.PropertyType;

            //backing field
            var lazyType = typeof(Lazy<>).MakeGenericType(propertyType);
            var fb = tb.DefineField("_backing_" + propertyName, lazyType, FieldAttributes.Private);

            //property
            var pb = tb.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

            //getter
            var getterAttributes = isInterfaceImplementation ? InterfaceImplementationAttributes : OverrideAttributes;
            var getMethod = tb.DefineMethod("get_" + propertyName, getterAttributes, propertyType, Type.EmptyTypes);
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

        /// <summary>
        /// creates a get+set property in the specified type which implements INPC
        /// </summary>
        /// <param name="tb">the typebuilder to use</param>
        /// <param name="property">the property which should be implemented</param>
        /// <param name="raiseMethod">a Method wich has a single parameter of type string and no return value</param>
        internal static void CreateReadWriteProperty(TypeBuilder tb, PropertyInfo property, MethodInfo raiseMethod, bool isInterfaceImplementation = true)
        {
            var propertyName = property.Name;
            var propertyType = property.PropertyType;
            var isValueType = propertyType.IsValueType;

            //backing
            var fb = tb.DefineField("_backing_" + propertyName, propertyType, FieldAttributes.Private);

            //property
            var pb = tb.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);


            var getsetAttributes = isInterfaceImplementation ? InterfaceImplementationAttributes : OverrideAttributes;

            //getter
            var getMethod = tb.DefineMethod("get_" + propertyName, getsetAttributes, propertyType, Type.EmptyTypes);
            var getIL = getMethod.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fb);
            getIL.Emit(OpCodes.Ret);

            //setter
            var setMethod = tb.DefineMethod("set_" + propertyName, getsetAttributes, null, new[] { propertyType });

            var setIL = setMethod.GetILGenerator();
            setIL.DeclareLocal(typeof(bool));
            var lExit = setIL.DefineLabel();
            var lAssignment = setIL.DefineLabel();
            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldfld, fb);
            if (isValueType) setIL.Emit(OpCodes.Box, propertyType);
            setIL.Emit(OpCodes.Ldarg_1);
            if (isValueType) setIL.Emit(OpCodes.Box, propertyType);
            setIL.EmitCall(OpCodes.Call, CodeGenHelper.GetMethod((Func<object, object, bool>)Object.Equals), null);
            setIL.Emit(OpCodes.Ldc_I4_0);
            setIL.Emit(OpCodes.Ceq);
            setIL.Emit(OpCodes.Stloc_0);
            setIL.Emit(OpCodes.Ldloc_0);
            setIL.Emit(OpCodes.Brtrue_S, lAssignment);
            setIL.Emit(OpCodes.Br_S, lExit);
            setIL.MarkLabel(lAssignment);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fb);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldstr, propertyName);
            setIL.EmitCall(OpCodes.Call, raiseMethod, null);
            setIL.Emit(OpCodes.Nop);
            setIL.MarkLabel(lExit);
            setIL.Emit(OpCodes.Ret);

            //combine property, getter, setter 
            pb.SetGetMethod(getMethod);
            pb.SetSetMethod(setMethod);
        }
    }
}
