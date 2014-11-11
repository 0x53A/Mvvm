#if !UNIVERSAL

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;


namespace Mvvm
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AssemblyInitializerAttribute : Attribute
    {
        public AssemblyInitializerAttribute()
        {
        }
    }

    public interface IAssemblyInitializer
    {
        void Init();
    }

    public class  AssemblyInitializerMultipleTypesFoundException : Exception
    {

    }

    public static class AssemblyInitializer
    {
        public static IAssemblyInitializer Find()
        {
            return Find(System.Reflection.Assembly.GetEntryAssembly());
        }

        public static IAssemblyInitializer Find(Assembly asm)
        {
            var suitableTypes = asm.GetExportedTypes()
                .Where(t =>
                    t.GetTypeInfo().GetInterfaces().Contains(typeof(IAssemblyInitializer)) &&
                    t.GetTypeInfo().GetCustomAttribute<AssemblyInitializerAttribute>() != null)
                .ToArray();
            if (suitableTypes.Length == 0)
                return null;
            else if (suitableTypes.Length == 1)
            {
                var instance = (IAssemblyInitializer)Activator.CreateInstance(suitableTypes.Single());
                return instance;
            }
            else
                throw new AssemblyInitializerMultipleTypesFoundException();
        }
    }
}

#endif
