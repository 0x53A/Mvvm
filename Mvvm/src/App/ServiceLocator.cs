using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mvvm;
using Mvvm.CodeGen;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Threading;


namespace Mvvm.App
{
    public class SimpleContainer
    {
        static Lazy<SimpleContainer> _default = new Lazy<SimpleContainer>(() => new SimpleContainer(), LazyThreadSafetyMode.ExecutionAndPublication);
        public static SimpleContainer Default { get { return _default.Value; } }

        Dictionary<Type, object> mapping = new Dictionary<Type, object>();

        private SimpleContainer() { }

        public void Register<T>(Func<T> getter)
        {
            lock (mapping)
                mapping[typeof(T)] = getter;
        }

        public T Get<T>()
        {
            lock (mapping)
                return (mapping[typeof(T)] as Func<T>)();
        }

        public object Get(Type t)
        {
            lock (mapping)
            {
                var ti = typeof(Func<>).GetTypeInfo();
                var genericFunc_t = ti.MakeGenericType(t);
                var funcObj = mapping[t];
                return genericFunc_t.GetTypeInfo().GetDeclaredMethod("Invoke").Invoke(funcObj, null);
            }
        }

        public bool Has<T>()
        {
            return Has(typeof(T));
        }

        public bool Has(Type t)
        {
            lock (mapping)
                return mapping.ContainsKey(t);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyResolve : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class CtorResolve : Attribute
    {

    }

    /// <summary>
    /// Represents a 
    /// </summary>
    public static class SimpleContainerEx
    {
        /// <summary>
        /// Registers a type which requires custom handling.
        /// </summary>
        /// <typeparam name="T">The Type to register</typeparam>
        /// <param name="ctor">called to create the type. Looks for a ctor on the type, if this is null.</param>
        /// <param name="postResolveInit">A init function to call which initialises the new object. May be null.</param>
        /// <param name="isSingleton">Specifies whether the returned object is cached or a new instance is created.</param>
        public static void RegisterWithResolveEx<T>(this SimpleContainer container, Func<T> ctor = null, Action<T> preResolveInit = null, Action<T> postResolveInit = null, bool isSingleton = false)
        {
            Contract.Requires(!isSingleton || typeof(T).GetTypeInfo().IsClass);

            var ti = typeof(T).GetTypeInfo();
            var props = ti
                .AsSingleLinkedList(n => n.BaseType.NP(_ => _.GetTypeInfo())) // get all base classes
                .SelectMany(t => t.GetDeclaredProperties().Where(p => p.GetCustomAttribute<PropertyResolve>() != null))
                .ToArray();
            if (ctor == null)
            {

                var ci = ti.GetDeclaredConstructors().SingleOrDefault(c => c.GetCustomAttribute<CtorResolve>() != null);
                if (ci == null)
                    ci = ti.GetDeclaredConstructors().Single(c => c.GetParameters().Length == 0);
                ctor = () =>
                {
                    object o;
                    var parameters = ci.GetParameters().Select(p => p.ParameterType);
                    var paramValues = parameters.Select(p => container.Get(p)).ToArray();
                    o = ci.Invoke(paramValues);
                    return (T)o;
                };
            }

            Func<T> create = () =>
            {
                var o = ctor();
                if (preResolveInit != null)
                    preResolveInit(o);
                foreach (var p in props)
                    p.GetSetMethod().Invoke(o, new[] { container.Get(p.PropertyType) });
                if (postResolveInit != null)
                    postResolveInit(o);
                return o;
            };

            Lazy<T> singleton = new Lazy<T>(create, LazyThreadSafetyMode.ExecutionAndPublication);

            if (isSingleton)
                container.Register<T>(() => singleton.Value);
            else
                container.Register<T>(create);
        }
    }
}
