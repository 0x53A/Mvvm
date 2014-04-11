using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    internal static class CodeGenHelper
    {
        internal static MethodInfo GetMethod<T>(Func<T> del) { return del.Method; }
        internal static MethodInfo GetMethod<T>(Action<T> del) { return del.Method; }
        internal static MethodInfo GetMethod<T1, T2>(Func<T1, T2> del) { return del.Method; }
        internal static MethodInfo GetMethod<T1, T2>(Action<T1, T2> del) { return del.Method; }
        internal static MethodInfo GetMethod<T1, T2, T3>(Func<T1, T2, T3> del) { return del.Method; }
        internal static MethodInfo GetMethod<T1, T2, T3>(Action<T1, T2, T3> del) { return del.Method; }
        internal static MethodInfo GetMethod<T1, T2, T3, T4>(Func<T1, T2, T3, T4> del) { return del.Method; }
        internal static MethodInfo GetMethod<T1, T2, T3, T4>(Action<T1, T2, T3, T4> del) { return del.Method; }
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

        /// <summary>
        /// the Attributes for a normal, nonvirtual instance method
        /// </summary>
        internal const MethodAttributes PrivateStaticMethodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static;


        #region INotifyPropertyChanged

        /// <summary>
        /// Implements the interface INotifyPropertyChanged on the specified type
        /// and returns a handle to the raise method
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="full">whether the full INPC (event,delegate,add,remove) should be implemented or only the raise-method</param>
        /// <returns>MethodBuilder for the raise-method</returns>
        internal static MethodInfo ImplementInpcFull(TypeBuilder tb)
        {
            var eventField = CreatePropertyChangedEvent(tb);
            var raiseMethod = CreateRaisePropertyChanged(tb, eventField);
            return raiseMethod;
        }

        internal static MethodBuilder ImplementBaseRaise(TypeBuilder typeBuilder)
        {
            //var dummyRaiseBasePropertyChanged = typeBuilder.DefineMethod("RaiseBasePropertyChanged", PrivateStaticMethodAttributes, null, new Type[] { typeof(object), typeof(string) });
            //var ilRaise = dummyRaiseBasePropertyChanged.GetILGenerator();
            /***************/
            // Declaring method builder
            // Method attributes
            System.Reflection.MethodAttributes methodAttributes =
                  System.Reflection.MethodAttributes.Private
                | System.Reflection.MethodAttributes.HideBySig
                | System.Reflection.MethodAttributes.Static;
            MethodBuilder method = typeBuilder.DefineMethod("RaiseBasePropertyChanged", methodAttributes);
            // Preparing Reflection instances
            MethodInfo method1 = typeof(Object).GetMethod(
                "GetType",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            },
                null
                );
            MethodInfo method2 = typeof(Type).GetMethod(
                "get_BaseType",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            },
                null
                );
            MethodInfo method3 = typeof(Type).GetMethod(
                "GetField",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(String),
            typeof(BindingFlags)
            },
                null
                );
            MethodInfo method4 = typeof(FieldInfo).GetMethod(
                "op_Equality",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(FieldInfo),
            typeof(FieldInfo)
            },
                null
                );
            MethodInfo method5 = typeof(Type).GetMethod(
                "GetTypeFromHandle",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(RuntimeTypeHandle)
            },
                null
                );
            MethodInfo method6 = typeof(Type).GetMethod(
                "op_Inequality",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Type),
            typeof(Type)
            },
                null
                );
            ConstructorInfo ctor7 = typeof(InvalidOperationException).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(String)
            },
                null
                );
            MethodInfo method8 = typeof(FieldInfo).GetMethod(
                "GetValue",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object)
            },
                null
                );
            ConstructorInfo ctor9 = typeof(PropertyChangedEventArgs).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(String)
            },
                null
                );
            MethodInfo method10 = typeof(PropertyChangedEventHandler).GetMethod(
                "Invoke",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
            typeof(Object),
            typeof(PropertyChangedEventArgs)
            },
                null
                );
            // Setting return type
            method.SetReturnType(typeof(void));
            // Adding parameters
            method.SetParameters(
                typeof(Object),
                typeof(String)
                );
            // Parameter self
            ParameterBuilder self = method.DefineParameter(1, ParameterAttributes.None, "self");
            // Parameter property
            ParameterBuilder property = method.DefineParameter(2, ParameterAttributes.None, "property");
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder type = gen.DeclareLocal(typeof(Type));
            LocalBuilder info = gen.DeclareLocal(typeof(FieldInfo));
            LocalBuilder handler = gen.DeclareLocal(typeof(PropertyChangedEventHandler));
            // Preparing labels
            Label label46 = gen.DefineLabel();
            Label label64 = gen.DefineLabel();
            Label label16 = gen.DefineLabel();
            Label label84 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, method1);
            gen.Emit(OpCodes.Callvirt, method2);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Br_S, label46);
            gen.MarkLabel(label16);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldstr, "PropertyChanged");
            gen.Emit(OpCodes.Ldc_I4_S, 36);
            gen.Emit(OpCodes.Callvirt, method3);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Call, method4);
            gen.Emit(OpCodes.Brfalse_S, label64);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Callvirt, method2);
            gen.Emit(OpCodes.Stloc_0);
            gen.MarkLabel(label46);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ldtoken, typeof(Object));
            gen.Emit(OpCodes.Call, method5);
            gen.Emit(OpCodes.Call, method6);
            gen.Emit(OpCodes.Brtrue_S, label16);
            gen.MarkLabel(label64);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Call, method4);
            gen.Emit(OpCodes.Brfalse_S, label84);
            gen.Emit(OpCodes.Ldstr, "Could not find the event field");
            gen.Emit(OpCodes.Newobj, ctor7);
            gen.Emit(OpCodes.Throw);
            gen.MarkLabel(label84);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Callvirt, method8);
            gen.Emit(OpCodes.Castclass, typeof(PropertyChangedEventHandler));
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Newobj, ctor9);
            gen.Emit(OpCodes.Callvirt, method10);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

            /***************/
            //return dummyRaiseBasePropertyChanged;
        }

        //private static void Swap(Type type, MethodBuilder dummy, MethodInfo replacement)
        //{
        //    var body = replacement.GetMethodBody();
        //    var bytesBody = body.GetILAsByteArray();
        //    var ms = new MemoryStream();
        //    byte flag = 0x03;
        //    if (body.InitLocals)
        //        flag |= 0x10;
        //    if (body.ExceptionHandlingClauses.Any())
        //        flag |= 0x08;
        //    ms.WriteByte(flag);
        //    ms.WriteByte(0x30);
        //    ms.Write(BitConverter.GetBytes((short)body.MaxStackSize));
        //    ms.Write(BitConverter.GetBytes((Int32)bytesBody.Length));
        //    ms.Write(BitConverter.GetBytes((Int32)body.LocalSignatureMetadataToken));            
        //    ms.Write(bytesBody);
        //    var blob = ms.ToArray();
        //    var handle = GCHandle.Alloc(blob, GCHandleType.Pinned);
        //    var ptr = handle.AddrOfPinnedObject();
        //    MethodRental.SwapMethodBody(type, dummy.GetToken().Token, ptr, blob.Length, MethodRental.JitImmediate);
        //    handle.Free();
        //}

        //internal static void SwapDummies(Type type, MethodBuilder dummyfindEventField, MethodBuilder dummyRaiseBasePropertyChanged)
        //{
        //    var methodRaise = CodeGenHelper.GetMethod((Action<object, string>)RaiseBasePropertyChanged);
        //    Swap(type, dummyRaiseBasePropertyChanged, methodRaise);
        //}

        /************************************************************************/
        /* This needs to be implemented inside the dynamic class*****************/
        /************************************************************************/
        
        private static void RaiseBasePropertyChanged(object self, string property)
        {
            var type = self.GetType().BaseType;
            FieldInfo eventField = null;
            while (type != typeof(object))
            {
                eventField = type.GetField("PropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                if (eventField == null)
                {
                    type = type.BaseType;
                    continue;
                }
                else
                    break;
            }
            if (eventField == null)
                throw new InvalidOperationException("Could not find the event field");
            var eventFieldValue = (PropertyChangedEventHandler)eventField.GetValue(self);
            eventFieldValue.Invoke(self, new PropertyChangedEventArgs(property));
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
