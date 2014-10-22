using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public string TypeName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RuntimeGeneratedTypeAttribute : Attribute
    {
        public static ConstructorInfo CtorInfo = typeof(RuntimeGeneratedTypeAttribute).GetTypeInfo().GetConstructor(new []{typeof(Type)});

        Type _generatedFrom;
        public RuntimeGeneratedTypeAttribute(Type generatedFrom)
        {
            _generatedFrom = generatedFrom;
        }

        public Type GeneratedFrom { get { return _generatedFrom; } }
    }
}
