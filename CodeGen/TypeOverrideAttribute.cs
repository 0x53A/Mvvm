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
        readonly string name;

        public TypeOverrideAttribute(string name = null)
        {
            this.name = name;
        }

        public string TypeName
        {
            get { return name; }
        }
    }
}
