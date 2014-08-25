//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace TestConsole
//{
//    class Program
//    {
//        internal const TypeAttributes GeneratedTypeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
//                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout;
//        internal const MethodAttributes ConstructorAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
//        internal const MethodAttributes PrivateStaticMethodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static;

//        static void Main(string[] args)
//        {
//            //create type
//            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("testAssembly"), AssemblyBuilderAccess.Run);
//            ModuleBuilder mb = ab.DefineDynamicModule("generated");
//            var tb = mb.DefineType("testClass", GeneratedTypeAttributes, typeof(object), Type.EmptyTypes);

//            //create constructor
//            var constructor = tb.DefineConstructor(ConstructorAttributes, CallingConventions.HasThis, null);
//            var ctorIL = constructor.GetILGenerator();

//            //emit the default constructor
//            ctorIL.Emit(OpCodes.Ldarg_0);
//            ctorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
//            ctorIL.Emit(OpCodes.Nop);
//            ctorIL.Emit(OpCodes.Ret);

//            //create dummy method with
//            var dummyMethod = tb.DefineMethod("DummyMethodToBeReplaced", PrivateStaticMethodAttributes, null, new Type[] { typeof(object), typeof(string) });
//            var ilRaise = dummyMethod.GetILGenerator();
//            ilRaise.Emit(OpCodes.Nop);
//            ilRaise.EmitWriteLine("dummy called");
//            ilRaise.Emit(OpCodes.Ret);

//            //create type
//            var type = tb.CreateType();

//            //swap
//            var methodHandle = typeof(Program).GetMethod("InjectedMethod", BindingFlags.NonPublic | BindingFlags.Static);
//            var bytesRaise = methodHandle.GetMethodBody().GetILAsByteArray();
//            var handleRaise = GCHandle.Alloc(bytesRaise, GCHandleType.Pinned);
//            var ptrRaise = handleRaise.AddrOfPinnedObject();
//            MethodRental.SwapMethodBody(tb, dummyMethod.GetToken().Token, ptrRaise, bytesRaise.Length, MethodRental.JitImmediate);
//            handleRaise.Free();

//            //try to call the method
//            var obj = Activator.CreateInstance(type);
//            var method = obj.GetType().GetMethod("DummyMethodToBeReplaced", BindingFlags.NonPublic | BindingFlags.Static);
//            method.Invoke(null, new object[] { null, "Replacement called" });

//            Console.WriteLine("==end of program==");
//            Console.ReadLine();
//        }

//        private static void InjectedMethod(object self, string property)
//        {
//            Console.WriteLine(property);
//        }
//    }
//}

public static class Asd
{
    public static string Asdd(string format, object o)
    {
        return Mvvm.FSharp.StringInterpolation.Do(format, o);
    }
}