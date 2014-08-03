using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mvvm;
using System.IO;
using System.Reflection;

namespace Mvvm.CodeGen
{
    internal static class CompileTimeMapping
    {

#if UNIVERSAL
        private sealed class AppDomain
        {
            public static AppDomain CurrentDomain { get; private set; }

            static AppDomain()
            {
                CurrentDomain = new AppDomain();
            }

            public Assembly[] GetAssemblies()
            {
                return GetAssemblyListAsync().Result.ToArray();
            }

            private async System.Threading.Tasks.Task<IEnumerable<Assembly>> GetAssemblyListAsync()
            {
                var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;

                List<Assembly> assemblies = new List<Assembly>();
                foreach (Windows.Storage.StorageFile file in await folder.GetFilesAsync())
                {
                    if (file.FileType == ".dll" || file.FileType == ".exe")
                    {
                        AssemblyName name = new AssemblyName() { Name = Path.GetFileNameWithoutExtension(file.Name) };
                        Assembly asm = Assembly.Load(name);
                        assemblies.Add(asm);
                    }
                }

                return assemblies;
            }
        }
#endif

        static Dictionary<Type, Type> mapping = new Dictionary<Type, Type>();
        static object _lock = new object();
        static Assembly[] _allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        internal static Type Map(Type type)
        {
            return mapping.GetFromKeyOrCreate(type, _lock, () =>
            {
                var lookingFor = "{0}.Generated.{1}".FormatWith(type.Namespace, type.Name);
                var mappedType = _allAssemblies.Select(a => a.GetType(lookingFor)).First(t => t != null);
                mapping[type] = mappedType;
                return mappedType;
            });
        }

        internal static object New(Type t)
        {
            var mappedType = Map(t);
            var instance = Activator.CreateInstance(mappedType);
            return instance;
        }

        internal static T New<T>(Action<T> init = null)
        {
            var mappedType = Map(typeof(T));
            var instance = (T)Activator.CreateInstance(mappedType);
            if (init != null)
                init(instance);
            return instance;
        }
    }
}
