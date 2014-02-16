using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
}
