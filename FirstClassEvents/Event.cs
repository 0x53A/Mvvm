using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.FirstClassEvents
{
    public class EventSource<T>
    {
        EventToken<T> _token = new EventToken<T>();
        public EventToken<T> Token { get { return _token; } }
        public Task Fire(T data)
        {
            return Token.Fire(data);
        }
    }

    public class EventToken<T> : IEvent<T>
    {
        object _lock = new object();
        List<Action<T>> _synchronousCallbacks = new List<Action<T>>();
        List<Func<T, Task>> _asyncCallbacks = new List<Func<T, Task>>();

        public void Subscribe(Action<T> callback)
        {
            lock (_lock)
                _synchronousCallbacks.Add(callback);
        }

        public void Subscribe(Func<T, Task> asyncCallback)
        {
            lock (_lock)
                _asyncCallbacks.Add(asyncCallback);
        }

        public void Unsubscribe(Action<T> callback)
        {
            lock (_lock)
                _synchronousCallbacks.Remove(callback);
        }

        public void Unsubscribe(Func<T, Task> asyncCallback)
        {
            lock (_lock)
                _asyncCallbacks.Remove(asyncCallback);
        }

        internal EventToken()
        {

        }

        /// <summary>
        /// Notifies all subscribers.
        /// Synchronous callbacks are called synchronously
        /// Asynchronous callbacks are started synchronously and awaited
        /// </summary>
        internal async Task Fire(T data)
        {
            Action<T>[] synchronous;
            Func<T, Task>[] asynchronous;
            lock (_lock)
            {
                synchronous = new Action<T>[_synchronousCallbacks.Count];
                _synchronousCallbacks.CopyTo(synchronous);
                asynchronous = new Func<T, Task>[_asyncCallbacks.Count];
                _asyncCallbacks.CopyTo(asynchronous);
            }
            List<Task> tasks = new List<Task>();
            foreach (var async in asynchronous)
            {
                var t = async(data);
                tasks.Add(t);
            }
            foreach (var sync in synchronous)
                sync(data);
            foreach (var t in tasks)
                await t;
        }
    }
}
