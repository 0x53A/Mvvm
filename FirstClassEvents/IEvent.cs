using System;
namespace Mvvm.FirstClassEvents
{
    public interface IEvent<T>
    {
        void Subscribe(Action<T> callback);
        void Subscribe(Func<T, System.Threading.Tasks.Task> asyncCallback);
        void Unsubscribe(Action<T> callback);
        void Unsubscribe(Func<T, System.Threading.Tasks.Task> asyncCallback);
    }
}
