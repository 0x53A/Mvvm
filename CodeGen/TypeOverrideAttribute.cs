using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TypeOverrideAttribute : Attribute
    {
        public TypeOverrideAttribute()
        {
        }

        public bool OverrideAllMembers { get; set; }

        public string TypeName { get; set; }
    }
}
