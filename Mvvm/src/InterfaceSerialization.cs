using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using Mvvm.CodeGen;

namespace Mvvm
{
    public static class InterfaceSerialization
    {
        static Dictionary<Type, Type[]> mappings = new Dictionary<Type, Type[]>();
        static object _lock = new object();

        public static void EnumerateAllTypes(Type t, IList<Type> knownTypes)
        {
            if (knownTypes.Contains(t))
                return;

            knownTypes.Add(t);

            if (t.GetIsConstructedGenericType())
                foreach (var gt in t.GetGenericTypeArguments())
                    EnumerateAllTypes(gt, knownTypes);

            foreach (var p in t.GetRuntimeProperties())
                EnumerateAllTypes(p.PropertyType, knownTypes);

            foreach (var f in t.GetRuntimeFields())
                EnumerateAllTypes(f.FieldType, knownTypes);
        }

        static Type[] GetMapping(Type type)
        {
            return mappings.GetFromKeyOrCreate(type, _lock, () =>
            {
                var knownTypes = new List<Type>();
                EnumerateAllTypes(type, knownTypes);
                var overridden = knownTypes.Where(t => t.GetTypeInfo().GetCustomAttribute<TypeOverrideAttribute>() != null)
                                           .Select(t => CG.Map(t)).ToArray();
                return overridden;
            });
        }

        public static string Serialize<T>(T o)
        {
            var overridden = GetMapping(typeof(T));
            DataContractSerializer serializer = new DataContractSerializer(typeof(T), overridden);
            var sb = new StringBuilder();
            using (var xWriter = XmlWriter.Create(sb))
                serializer.WriteObject(xWriter, o);
            return sb.ToString();
        }

        public static T Deserialize<T>(string s)
        {
            var overridden = GetMapping(typeof(T));
            DataContractSerializer serializer = new DataContractSerializer(typeof(T), overridden);
            using (var sr = new StringReader(s))
            using (var xReader = XmlReader.Create(sr))
                return (T)serializer.ReadObject(xReader);
        }

        public static Type[] KnownTypesOf<T>()
        {
            return GetMapping(typeof(T));
        }
    }
}
