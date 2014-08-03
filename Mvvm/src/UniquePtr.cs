using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public static class UniquePtr
    {
        public static UniquePtr<T> Create<T>(T handle, Action<T> release)
        {
            return new UniquePtr<T>(handle, release);
        }
    }

    public sealed class UniquePtr<T> : IDisposable
    {
        Action<T> _release;
        T _handle;

        public T Handle { get { return _handle; } }

        public UniquePtr(T handle, Action<T> release)
        {
            _handle = handle;
            _release = release;
        }

        public void Dispose()
        {
            if (!_handle.Equals(default(T)))
            {
                var h = _handle;
                _handle = default(T);
                _release(h);
            }
        }
    }
}
