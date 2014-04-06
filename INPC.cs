using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/* These classes enable a safe way to interact with INotifyPropertyChanged.
 * The goal is to eliminate all uses of hardcoded strings.
 * In some cases, all generic types can be inferred.
 * These methods are in the non-generic INPC class.
 * In other cases, the type of the source object must be supplied.
 * These methods are in INPC<T>.
 * 
 * */

namespace Mvvm
{
    public class OnPropertyChanged<TSource>
    {
        string _prop;

        public OnPropertyChanged(string propertyName)
        {
            _prop = propertyName;
        }

        public bool Is<TProp>(Expression<Func<TSource, TProp>> expr)
        {
            return INPC<TSource>.Is<TProp>(expr, _prop);
        }

        public string PropertyName { get { return _prop; } }
    }

    public enum BindingMode { SourceToDestination, DestinationToSource, TwoWay }


    /// <summary>
    /// Enables Binding between two objects which implement INPC
    /// </summary>
    public class INPCBinding<TSource, TDestination, TProperty>
    {
        object source;
        object destination;

        List<MemberInfo> stepsSource = new List<MemberInfo>();
        List<MemberInfo> stepsDestination = new List<MemberInfo>();
        List<Tuple<INotifyPropertyChanged, PropertyChangedEventHandler>> subscribedSource = new List<Tuple<INotifyPropertyChanged, PropertyChangedEventHandler>>();
        List<Tuple<INotifyPropertyChanged, PropertyChangedEventHandler>> subscribedDestination = new List<Tuple<INotifyPropertyChanged, PropertyChangedEventHandler>>();
        BindingMode mode;

        public INPCBinding(TSource source, Expression<Func<TSource, TProperty>> sourceSelector,
            TDestination destination, Expression<Func<TDestination, TProperty>> destinationSelector,
            BindingMode mode)
        {
            this.mode = mode;
            this.source = source;
            this.destination = destination;

            RecursiveScan(sourceSelector.Body as MemberExpression, stepsSource);
            RecursiveScan(destinationSelector.Body as MemberExpression, stepsDestination);

            if (mode == BindingMode.SourceToDestination || mode == BindingMode.TwoWay)
                Bind(true);
            if (mode == BindingMode.DestinationToSource || mode == BindingMode.TwoWay)
                Bind(false);

            if (mode == BindingMode.SourceToDestination || mode == BindingMode.TwoWay)
                SetIfDifferent(true);
            else
                SetIfDifferent(false);
        }

        void RecursiveScan(MemberExpression exp, List<MemberInfo> list)
        {
            if (exp is MemberExpression)
            {
                RecursiveScan(exp.Expression as MemberExpression, list);
                list.Add(exp.Member);
            }
        }

        public void Unbind()
        {
            Unbind(true);
            Unbind(false);
        }

        void Unbind(bool isFromSource)
        {
            if (isFromSource)
            {
                foreach (var x in subscribedSource)
                    x.Item1.PropertyChanged -= x.Item2;
            }
            else
            {
                foreach (var x in subscribedDestination)
                    x.Item1.PropertyChanged -= x.Item2;
            }
        }

        void Bind(bool isSource)
        {
            object current = isSource ? source : destination;
            var list = isSource ? stepsSource : stepsDestination;

            for (int i = 0; i < list.Count; i++)
            {
                var o = list[i];
                if (current is INotifyPropertyChanged)
                {
                    var tuple = Tuple.Create<INotifyPropertyChanged, PropertyChangedEventHandler>(current as INotifyPropertyChanged, (a, b) =>
                    {
                        if (b.PropertyName == o.Name)
                        {
                            if (i == list.Count - 1)
                            {
                                SetIfDifferent(isSource);
                            }
                            else
                            {
                                Unbind(isSource);
                                Bind(isSource);
                                SetIfDifferent(isSource);
                            }
                        }
                    });
                    (current as INotifyPropertyChanged).PropertyChanged += tuple.Item2;
                    if (isSource)
                        subscribedSource.Add(tuple);
                    else
                        subscribedDestination.Add(tuple);
                }
                current = Get(o, current);
            }
        }

        object Get(MemberInfo info, object o)
        {
            if (info is FieldInfo)
                return (info as FieldInfo).GetValue(o);
            else if (info is PropertyInfo)
                return (info as PropertyInfo).GetValue(o);
            else
                throw new InvalidOperationException();
        }

        void SetIfDifferent(bool isSourceToDestination)
        {
            var sObj = source;
            var dObj = destination;

            for (int i = 0; i < stepsSource.Count - 1; i++)
                sObj = Get(stepsSource[i], sObj);
            for (int i = 0; i < stepsDestination.Count - 1; i++)
                dObj = Get(stepsDestination[i], dObj);

            var sProp = stepsSource.Last();
            var dProp = stepsDestination.Last();

            var sVal = Get(sProp, sObj);
            var dVal = Get(dProp, dObj);

            if (sVal != dVal)
            {
                if (isSourceToDestination)
                    (dProp as PropertyInfo).SetValue(dObj, sVal);
                else
                    (sProp as PropertyInfo).SetValue(sObj, dVal);
            }
        }
    }

    public static class INPC
    {
        /// <summary>
        /// Enables a typesafe and refactorsafe way to subscribe to a INotifyPropertyChanged event.      
        /// </summary>
        /// <example>Subscribe(obj, x=>x.Name, (self,value)=>Console.WriteLine("{0}", value));</example>
        /// <typeparam name="TSource">The type of the source object</typeparam>
        /// <typeparam name="TProperty">The type of the Property in Question</typeparam>
        /// <param name="source">the source object, must implement INotifyPropertyChanged</param>
        /// <param name="expression">An lambda expression selecting the Property</param>
        /// <param name="callback">The callback which should be called when the Property changes</param>
        public static void Subscribe<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> expression, Action<TSource, TProperty> callback)
        {
            Contract.Requires(expression.Body.NodeType == ExpressionType.MemberAccess);
            Contract.Requires(expression.Body is MemberExpression);
            Contract.Requires((expression.Body as MemberExpression).Member is PropertyInfo);
            Contract.Requires((expression.Body as MemberExpression).Expression is ParameterExpression);
            Contract.Requires(source is INotifyPropertyChanged);

            var exp = expression.Body as MemberExpression;
            var prop = exp.Member as PropertyInfo;
            var getter = prop.GetMethod;
            var inpc = source as INotifyPropertyChanged;
            inpc.PropertyChanged += (a, b) => { if (b.PropertyName == prop.Name) callback((TSource)a, (TProperty)getter.Invoke(a, null)); };
        }

        //public static void Subscribe<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> expression, Func<TSource, TProperty, Task> callback)
        //{
        //    Subscribe<TSource, TProperty>(source, expression, (a, b) => callback(a, b).Wait());
        //}

        public static void SubscribeAll<TSource>(TSource source, Action<TSource, OnPropertyChanged<TSource>> callback)
        {
            var inpc = source as INotifyPropertyChanged;
            inpc.PropertyChanged += (a, b) => callback((TSource)a, new OnPropertyChanged<TSource>(b.PropertyName));
        }

        //public static void SubscribeAll<TSource>(TSource source, Func<TSource, OnPropertyChanged<TSource>, Task> callback)
        //{
        //    var inpc = source as INotifyPropertyChanged;
        //    inpc.PropertyChanged += (a, b) => callback((TSource)a, new OnPropertyChanged<TSource>(b.PropertyName)).Wait();
        //}

        public static void Unsubscribe<TSource, TProperty>(TSource source, Action<TSource, TProperty> callback)
        {
            //TODO
        }
        public static INPCBinding<TSource, TDestination, TProperty> Bind<TSource, TDestination, TProperty>
            (TSource source, Expression<Func<TSource, TProperty>> sourceSelector,
            TDestination destination, Expression<Func<TDestination, TProperty>> destinationSelector,
            BindingMode mode)
        {
            return new INPCBinding<TSource, TDestination, TProperty>(source, sourceSelector, destination, destinationSelector, mode);
        }
    }

    public static class INPC<TSource>
    {
        internal static string ExtractMemberName<TProperty>(Expression<Func<TSource, TProperty>> expression)
        {
            Contract.Requires(expression.Body.NodeType == ExpressionType.MemberAccess);
            Contract.Requires(expression.Body is MemberExpression);
            Contract.Requires((expression.Body as MemberExpression).Member is PropertyInfo);

            var exp = expression.Body as MemberExpression;
            var prop = exp.Member as PropertyInfo;
            var name = prop.Name;
            return name;
        }

        /// <summary>
        /// To be used in the INotifyPropertyChanged Callback to check, whether the changed Property is a specific one
        /// </summary>
        public static bool Is<TProperty>(Expression<Func<TSource, TProperty>> expression, string toMatch)
        {
            Contract.Requires(expression.Body.NodeType == ExpressionType.MemberAccess);
            Contract.Requires(expression.Body is MemberExpression);
            Contract.Requires((expression.Body as MemberExpression).Member is PropertyInfo);

            return INPC<TSource>.ExtractMemberName(expression) == toMatch;
        }
    }
}
