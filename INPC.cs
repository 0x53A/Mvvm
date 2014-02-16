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
            Contract.Requires((expression.Body as MemberExpression).Member.MemberType == MemberTypes.Property);
            Contract.Requires(source is INotifyPropertyChanged);

            var exp = expression.Body as MemberExpression;
            var prop = exp.Member as PropertyInfo;
            var getter = prop.GetGetMethod();
            var inpc = source as INotifyPropertyChanged;
            inpc.PropertyChanged += (a, b) => { if (b.PropertyName == prop.Name) callback((TSource)a, (TProperty)getter.Invoke(a, null)); };
        }

        public static void Unsubscribe<TSource, TProperty>(TSource source, Action<TSource, TProperty> callback)
        {
            //TODO
        }
    }

    public static class INPC<TSource>
    {
        internal static string ExtractMemberName<TProperty>(Expression<Func<TSource, TProperty>> expression)
        {
            Contract.Requires(expression.Body.NodeType == ExpressionType.MemberAccess);
            Contract.Requires(expression.Body is MemberExpression);
            Contract.Requires((expression.Body as MemberExpression).Member.MemberType == MemberTypes.Property);

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
            Contract.Requires((expression.Body as MemberExpression).Member.MemberType == MemberTypes.Property);

            return INPC<TSource>.ExtractMemberName(expression) == toMatch;
        }
    }
}
