using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public enum ComparisonMethod { ObjectEquals, EqualityComparer, ReferenceEquals }

    internal abstract class Comparison
    {
        internal abstract bool Do(object a, object b);
    }

    internal class DefaultComparison<T, T1> : Comparison where T : class
    {
        Func<T, T1> _get;
        ComparisonMethod _comp;
        internal DefaultComparison(Func<T, T1> getter, ComparisonMethod comp = ComparisonMethod.EqualityComparer)
        {
            _get = getter;
            _comp = comp;
        }

        internal override bool Do(object a, object b)
        {
            return EqualityComparer<T1>.Default.Equals(_get((T)a), _get((T)b));
        }
    }

    public class ComparisonSequence<T> where T : class
    {
        private List<Comparison> _comparisons;

        public ComparisonSequence<T> Add<T1>(Func<T, T1> c1)
        {
            _comparisons.Add(new DefaultComparison<T, T1>(c1));
            return this;
        }

        public Func<T, T, bool> Compile()
        {
            return (a, b) => this.Compare(a, b);
        }

        private bool Compare(T a, T b)
        {
            foreach (var c in _comparisons)
                if (false == c.Do(a, b))
                    return false;
            return true;
        }
    }

    public static class ComparisonBuilder<T> where T : class
    {
        public static ComparisonSequence<T> Create()
        {
            var seq = new ComparisonSequence<T>();
            return seq;
        }
    }
}
