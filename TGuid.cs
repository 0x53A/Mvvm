using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mTIM.Models.DataModel
{
    public class TGuid<T>
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

        public override bool Equals(object obj)
        {
            if (obj is Guid)
            {
                return this.Id == (Guid)obj;
            }
            else if (obj is TGuid<T>)
                return this.Id == ((TGuid<T>)obj).Id;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public static bool operator ==(TGuid<T> a, TGuid<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator ==(Guid a, TGuid<T> b)
        {
            return b.Equals(a);
        }

        public static bool operator ==(TGuid<T> a, Guid b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TGuid<T> a, TGuid<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Guid a, TGuid<T> b)
        {
            return b.Equals(a);
        }

        public static bool operator !=(TGuid<T> a, Guid b)
        {
            return a.Equals(b);
        }
    }
}
