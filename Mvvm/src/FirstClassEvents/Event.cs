using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.FirstClassEvents
{
    /* EventSource */

    public class EventSource
    {
        EventToken _token = new EventToken();
        public EventToken Token { get { return _token; } }
        public Task FireAsync()
        {
            return Token.Fire();
        }
        public void Fire()
        {
            FireAsync().Wait();
        }
    }

    public class EventSource<T>
    {
        EventToken<T> _token = new EventToken<T>();
        public EventToken<T> Token { get { return _token; } }
        public Task FireAsync(T data)
        {
            return Token.Fire(data);
        }
        public void Fire(T data)
        {
            FireAsync(data).Wait();
        }
    }

    public class EventSource<TSource, TData>
    {
        EventToken<TSource, TData> _token = new EventToken<TSource, TData>();
        public EventToken<TSource, TData> Token { get { return _token; } }
        public Task FireAsync(TSource source, TData data)
        {
            return Token.Fire(source, data);
        }
        public void Fire(TSource source, TData data)
        {
            FireAsync(source, data).Wait();
        }
    }

    /* EventToken */

    public class EventToken : IEvent
    {
        object _lock = new object();
        List<Action> _synchronousCallbacks = new List<Action>();
        List<Func<Task>> _asyncCallbacks = new List<Func<Task>>();

        public void Subscribe(Action callback)
        {
            lock (_lock)
                _synchronousCallbacks.Add(callback);
        }

        public void Subscribe(Func<Task> asyncCallback)
        {
            lock (_lock)
                _asyncCallbacks.Add(asyncCallback);
        }

        public void Unsubscribe(Action callback)
        {
            lock (_lock)
                _synchronousCallbacks.Remove(callback);
        }

        public void Unsubscribe(Func<Task> asyncCallback)
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
        internal async Task Fire()
        {
            Action[] synchronous;
            Func<Task>[] asynchronous;
            lock (_lock)
            {
                synchronous = new Action[_synchronousCallbacks.Count];
                _synchronousCallbacks.CopyTo(synchronous);
                asynchronous = new Func<Task>[_asyncCallbacks.Count];
                _asyncCallbacks.CopyTo(asynchronous);
            }
            List<Task> tasks = new List<Task>();
            foreach (var async in asynchronous)
            {
                var t = async();
                tasks.Add(t);
            }
            foreach (var sync in synchronous)
                sync();
            foreach (var t in tasks)
                await t;
        }
    }

    /* EventToken<T> */

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

    /* EventToken<TSource, TData> */

    public class EventToken<TSource, TData> : IEvent<TSource, TData>
    {
        object _lock = new object();
        List<Action<TSource, TData>> _synchronousCallbacks = new List<Action<TSource, TData>>();
        List<Func<TSource, TData, Task>> _asyncCallbacks = new List<Func<TSource, TData, Task>>();

        public void Subscribe(Action<TSource, TData> callback)
        {
            lock (_lock)
                _synchronousCallbacks.Add(callback);
        }

        public void Subscribe(Func<TSource, TData, Task> asyncCallback)
        {
            lock (_lock)
                _asyncCallbacks.Add(asyncCallback);
        }

        public void Unsubscribe(Action<TSource, TData> callback)
        {
            lock (_lock)
                _synchronousCallbacks.Remove(callback);
        }

        public void Unsubscribe(Func<TSource, TData, Task> asyncCallback)
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
        internal async Task Fire(TSource source, TData data)
        {
            Action<TSource, TData>[] synchronous;
            Func<TSource, TData, Task>[] asynchronous;
            lock (_lock)
            {
                synchronous = new Action<TSource, TData>[_synchronousCallbacks.Count];
                _synchronousCallbacks.CopyTo(synchronous);
                asynchronous = new Func<TSource, TData, Task>[_asyncCallbacks.Count];
                _asyncCallbacks.CopyTo(asynchronous);
            }
            List<Task> tasks = new List<Task>();
            foreach (var a in asynchronous)
            {
                var t = a(source, data);
                tasks.Add(t);
            }
            foreach (var sync in synchronous)
                sync(source, data);
            foreach (var t in tasks)
                await t;
        }
    }
}
