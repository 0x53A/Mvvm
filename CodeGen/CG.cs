using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Mvvm;
using System.IO;
using System.Reflection;

namespace Mvvm.CodeGen
{
    public class CG
    {
        public static T New<T>(Action<T> init = null)
        {
            var t = typeof(T);
            if (t.IsInterface)
                return DBCGenerator.Generate<T>(init);
            else
                return VMWrapper.Wrap<T>(init);
        }

        public static string SerializeInterface<T>(T value)
        {
            return value.XmlSerialize();
        }

        static IEnumerable<Type> AllInterfaces(Type type)
        {
            List<Type> knownTypes = new List<Type>();
            Action<Type> forType = null;
            forType = (t) =>
            {

                foreach (var prop in t.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var pt = prop.PropertyType;
                    if (!knownTypes.Contains(pt))
                    {
                        knownTypes.Add(pt);
                        forType(pt);
                    }
                }
            };
            knownTypes.Add(type);
            forType(type);
            return knownTypes.Where(t => t.IsInterface);
        }

        static object _mappingLock = new object();
        static Dictionary<Type, Type[]> _mapping = new Dictionary<Type, Type[]>();

        public static T DeserializeInterface<T>(string xml)
        {
            var targetType = typeof(T);
            Type[] mappedTypes;
            if (_mapping.ContainsKey(targetType))
                mappedTypes = _mapping[targetType];
            else
            {
                lock (_mappingLock)
                {
                    if (_mapping.ContainsKey(targetType))
                        mappedTypes = _mapping[targetType];
                    else
                    {
                        var types = AllInterfaces(targetType);
                        mappedTypes = types.Select(t => DBCGenerator.Map(t)).ToArray();
                    }
                }
            }
            var serializer = new XmlSerializer(targetType, mappedTypes);
            return (T)serializer.Deserialize(new StringReader(xml));
        }
    }
}
