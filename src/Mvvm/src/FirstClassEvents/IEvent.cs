using System;

namespace Mvvm.FirstClassEvents
{
    public interface IEvent
    {
        void Subscribe(Action callback);
        //void Subscribe(Func<System.Threading.Tasks.Task> asyncCallback);
        void Unsubscribe(Action callback);
        //void Unsubscribe(Func<System.Threading.Tasks.Task> asyncCallback);
    }

    public interface IEvent<T>
    {
        void Subscribe(Action<T> callback);
        //void Subscribe(Func<T, System.Threading.Tasks.Task> asyncCallback);
        void Unsubscribe(Action<T> callback);
        //void Unsubscribe(Func<T, System.Threading.Tasks.Task> asyncCallback);
    }

    public interface IEvent<TSource, TArg>
    {
        void Subscribe(Action<TSource, TArg> callback);
        //void Subscribe(Func<TSource, TArg, System.Threading.Tasks.Task> asyncCallback);
        void Unsubscribe(Action<TSource, TArg> callback);
        //void Unsubscribe(Func<TSource, TArg, System.Threading.Tasks.Task> asyncCallback);
    }
}
