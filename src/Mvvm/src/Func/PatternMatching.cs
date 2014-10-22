//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Linq;

//namespace Mvvm.Func
//{
//    public class Unit
//    {
//        private Unit(){}
//        private static Unit _u = new Unit();
//        public static Unit U {get{return _u;}}
//    }

//    public class MatchExpression<TSource, TResult>
//    {
//        protected class MatchStep
//        {
//            public List<Func<TSource, bool>> conditions {get;set;}
//            public Func<TSource, TResult> action {get;set;}
//        }

//        protected List<MatchStep> _steps = new List<MatchStep>();
//        protected MatchStep currentStep;
//        protected TSource _obj;

//        public MatchExpression<TSource,TResult> With(Func<TSource, bool> expr)
//        {

//        }

//        public TResult Result()
//        {
//            var firstMatch = _steps.FirstOrDefault(s => s.conditions.Any(cond => cond(_obj)));
//            if (firstMatch == null)
//                throw new InvalidOperationException();
//            else
//                return firstMatch.action(_obj);
//        }
//    }

//    public class OpenMatchExpression<TSource, TResult> : MatchExpression<TSource, TResult>
//    {
//        public MatchExpression<TSource, TResult> Do(Func<TSource, TResult> action)
//        {
//            return this;
//        }
//    }

//    public static class Pattern<TSource>
//    {
//        public static MatchExpression<TSource,TResult> Match<TResult>(TSource o)
//        {

//        }
//    }
//}
