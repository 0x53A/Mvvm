using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public struct TGuid<T>
    {
        Guid _id;
        public Guid Id { get { return _id; } }

        public TGuid(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(TGuid<T> id)
        {
            return id.Id;
        }

        public static implicit operator TGuid<T>(Guid id)
        {
            return new TGuid<T>(id);
        }
    }
}
