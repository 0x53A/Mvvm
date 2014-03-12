using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    [AttributeUsage(AttributeTargets.Interface| AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class GeneratedTypeNameAttribute : Attribute
    {
        readonly string name;

        // This is a positional argument
        public GeneratedTypeNameAttribute(string name)
        {
            this.name = name;
        }

        public string TypeName
        {
            get { return name; }
        }
    }
}
