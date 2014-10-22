using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mvvm
{
    public static class Net40Extensions
    {
#if NET40
        public static T GetCustomAttribute<T>(this MemberInfo t) where T : Attribute
        {
            return t.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        public static Type GetTypeInfo(this Type t)
        {
            return t;
        }

        public static MethodInfo GetDeclaredMethod(this Type t, string name)
        {
            return t.GetMethod(name);
        }

        public static IList<PropertyInfo> GetRuntimeProperties(this Type t)
        {
            return t.GetProperties();
        }

        public static PropertyInfo GetRuntimeProperty(this Type t, string name)
        {
            return t.GetProperty(name);
        }

        public static IList<FieldInfo> GetRuntimeFields(this Type t)
        {
            return t.GetFields();
        }

        public static object GetValue(this PropertyInfo pi, object o)
        {
            return pi.GetValue(o, null);
        }

        public static void SetValue(this PropertyInfo pi, object o, object val)
        {
            pi.SetValue(o, val, null);
        }

        public static Task<TResult> ContinueWith<TResult>(this Task t, Func<Task, Object, TResult> continuationFunction, Object state, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
        {
            return t.ContinueWith((_) => continuationFunction(_, state), cancellationToken, continuationOptions, scheduler);
        }
#endif

        public static IList<Type> GetGenericTypeArguments(this Type t)
        {
#if NET40
            return t.GetGenericArguments();
#else
            return t.GenericTypeArguments;
#endif
        }

        public static bool GetIsConstructedGenericType(this Type t)
        {
#if NET40
            return !t.ContainsGenericParameters;
#else
            return t.IsConstructedGenericType;
#endif
        }

#if NET40
        public static IList<ConstructorInfo> GetDeclaredConstructors(this Type t)
#else
        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this TypeInfo t)
#endif
        {
#if NET40
            return t.GetConstructors();
#else
            return t.DeclaredConstructors;
#endif
        }

#if NET40
        public static IList<PropertyInfo> GetDeclaredProperties(this Type t)
#else
        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this TypeInfo t)
#endif
        {
#if NET40
            return t.GetProperties();
#else
            return t.DeclaredProperties;
#endif
        }

#if UNIVERSAL
        public static MethodInfo GetGetMethod(this PropertyInfo ti)
        {
            return ti.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo ti)
        {
            return ti.SetMethod;
        }

        public static IEnumerable<Type> GetInterfaces(this TypeInfo ti)
        {
            return ti.ImplementedInterfaces;
        }

        public static ConstructorInfo GetConstructor(this TypeInfo ti, Type[] args)
        {
            return ti.DeclaredConstructors.SingleOrDefault(c => c.GetParameters().Select(p=>p.ParameterType).SequenceEqual(args));
        }
#endif
    }

    public static class TaskHelper
    {
        public static Task<T> FromResult<T>(T result)
        {
#if NET40
            return new Task<T>(() => result);
#else
            return Task.FromResult(result);
#endif
        }
    }

#if UNIVERSAL
    public static class UniversalExtensions
    {
        public static string GetString(this Encoding encoding, byte[] bytes)
        {
            return encoding.GetString(bytes, 0, bytes.Length);
        }
    }
#endif
}
