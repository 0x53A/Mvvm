using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    public class CodeGenHelper
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

    }
}
