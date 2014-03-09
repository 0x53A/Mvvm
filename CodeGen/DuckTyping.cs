using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    public static class DuckTyping
    {
        static MemberInfo TryFindMember(Type thingType, MemberInfo duckMember)
        {
            return null;
        }

        public static bool CanCast<T>(object thing)
        {
            Contract.Requires(thing != null);
            Contract.Requires(typeof(T).IsInterface);

            var duckType = typeof(T);
            var thingType = thing.GetType();
            var members = FindAllMembers(duckType);
            Contract.Assume(members.All(m => m is MethodInfo || m is PropertyInfo));
            var thingMembers = members.Select(m => TryFindMember(thingType, m));

            return true;
        }

        private static IEnumerable<MemberInfo> FindAllMembers(Type duckType)
        {
            var interfaces = duckType.Flatten(t => t.GetInterfaces()).Distinct();
            var members = from i in interfaces
                          from ii in i.GetMembers()
                          select ii;
            return members.Distinct();
        }

        public static T Cast<T>(object thing)
        {
            Contract.Requires(CanCast<T>(thing));
            return default(T);
        }
    }
}
