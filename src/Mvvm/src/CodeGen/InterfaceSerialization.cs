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

            if (t.Namespace.StartsWith("System.Reflection"))
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
                List<Type> types = new List<Type>();
                foreach (var t in knownTypes)
                {
                    if (t.GetTypeInfo().GetCustomAttribute<RuntimeGeneratedTypeAttribute>() != null)
                        types.Add(t);
                    else if (t.GetTypeInfo().IsAbstract && t.GetTypeInfo().IsClass)
                        types.Add(CG.Map(t));
                    else if (t.GetTypeInfo().IsInterface)
                    {
                        if (t.GetTypeInfo().GetCustomAttribute<TypeOverrideAttribute>() != null)
                            types.Add(CG.Map(t));
                        else if (t.Namespace == "System.Collections.Generic" && t.Name == "IList`1")
                            types.Add(typeof(List<>).MakeGenericType(t.GetGenericTypeArguments().ToArray()));
                        else
                            types.Add(CG.Map(t));
                    }
                }

                return types.ToArray();
            });
        }

        class GeneratedTypeDataContractResolver : DataContractResolver
        {
            public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            {
                return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
            }

            public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
            {
                //if (dataContractType.IsInterface)
                //{
                //    // Type was a proxy type, so map the name to the non-proxy name
                //    XmlQualifiedName qualifiedName = _exporter.GetSchemaTypeName(nonProxyType);
                //    XmlDictionary dictionary = new XmlDictionary(2);
                //    typeName = new XmlDictionaryString(dictionary, qualifiedName.Name, 0);
                //    typeNamespace = new XmlDictionaryString(dictionary, qualifiedName.Namespace, 1);
                //    return true;
                //}
                //else
                {
                    // Type was not a proxy type, so do the default
                    return knownTypeResolver.TryResolveType(dataContractType, declaredType, null, out typeName, out typeNamespace);
                }
            }
        }

        public static string Serialize<T>(T o)
        {
            var overridden = GetMapping(typeof(T));
            DataContractSerializer serializer = new DataContractSerializer(typeof(T), overridden);
            using (var ms = new MemoryStream())
            {
                using (var xWriter = XmlDictionaryWriter.CreateTextWriter(ms))
                    serializer.WriteObject(xWriter, o/*, new GeneratedTypeDataContractResolver()*/);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T Deserialize<T>(string s)
        {
            var overridden = GetMapping(typeof(T));
            DataContractSerializer serializer = new DataContractSerializer(typeof(T), overridden);
            using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(s)))
            using (var xReader = XmlDictionaryReader.CreateTextReader(sr, XmlDictionaryReaderQuotas.Max))
                return (T)serializer.ReadObject(xReader, true/*, new GeneratedTypeDataContractResolver()*/);
        }

        public static Type[] KnownTypesOf<T>()
        {
            return GetMapping(typeof(T));
        }
    }
}
