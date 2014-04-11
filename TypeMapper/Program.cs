using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mvvm;
using Mvvm.CodeGen;
using System.ComponentModel;
using System.Diagnostics;

namespace TypeMapper
{
    class Program
    {
        static void ImplementProperty(CodeTypeDeclaration type, PropertyInfo p, bool isAbstract, bool isLazy)
        {
            var fieldName = "backing_{0}".FormatWith(p.Name);
            if (isLazy)
            {
                var lazyType = typeof(Lazy<>).MakeGenericType(p.PropertyType);
                var field = new CodeMemberField(new CodeTypeReference(lazyType), fieldName);
                field.Attributes = MemberAttributes.Private;
                field.InitExpression = new CodeObjectCreateExpression(lazyType);
                type.Members.Add(field);

                var prop = new CodeMemberProperty();
                prop.Name = p.Name;
                prop.Attributes = MemberAttributes.Public;
                if (isAbstract)
                    prop.Attributes |= MemberAttributes.Override;
                prop.HasGet = true;
                prop.Type = new CodeTypeReference(p.PropertyType);
                prop.GetStatements.Add(
                    new CodeMethodReturnStatement(
                     new CodePropertyReferenceExpression(
                      new CodeFieldReferenceExpression(
                       new CodeThisReferenceExpression(), fieldName), "Value")));
                type.Members.Add(prop);
            }
            else
            {
                var typeEqComparer = typeof(EqualityComparer<>).MakeGenericType(p.PropertyType);
                var field = new CodeMemberField(new CodeTypeReference(p.PropertyType), fieldName);
                field.Attributes = MemberAttributes.Private;
                type.Members.Add(field);

                var prop = new CodeMemberProperty();
                prop.Name = p.Name;
                prop.Type = new CodeTypeReference(p.PropertyType);
                prop.Attributes = MemberAttributes.Public;
                if (isAbstract)
                    prop.Attributes |= MemberAttributes.Override;
                prop.HasGet = true;
                prop.HasSet = true;

                prop.GetStatements.Add(
                    new CodeMethodReturnStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(), fieldName)));

                prop.SetStatements.Add(
                    new CodeConditionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodePropertyReferenceExpression(
                                    new CodeTypeReferenceExpression(typeEqComparer),
                                    "Default"),
                                "Equals"),
                            new CodePropertySetValueReferenceExpression(),
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(),
                                fieldName)),
                        new CodeMethodReturnStatement()));

                prop.SetStatements.Add(
                     new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeThisReferenceExpression(), "OnPropertyChanged"),
                            new CodePrimitiveExpression(p.Name)));

                prop.SetStatements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(), fieldName),
                        new CodePropertySetValueReferenceExpression()));
                type.Members.Add(prop);
            }
        }

        static void ImplementType(CodeCompileUnit unit, Type t, bool isAbstract)
        {
            var nspace = new CodeNamespace(t.Namespace + ".Generated");
            nspace.Imports.Add(new CodeNamespaceImport("System"));
            var type = new CodeTypeDeclaration(t.Name);
            type.IsClass = true;
            type.TypeAttributes = TypeAttributes.Public;
            type.BaseTypes.Add(t);

            var props = t.GetProperties();

            Func<PropertyInfo, bool> lazyFilter = null;
            Func<PropertyInfo, bool> InpcFilter = null;
            if (isAbstract)
            {
                lazyFilter = p => (p.GetMethod != null && p.SetMethod == null && p.GetMethod.IsAbstract) || p.GetCustomAttributesData().Any(a => a.AttributeType.Name == typeof(LazyAttribute).Name);
                InpcFilter = p => (p.GetMethod != null && p.SetMethod != null && p.GetMethod.IsAbstract && p.SetMethod.IsAbstract) || p.GetCustomAttributesData().Any(a => a.AttributeType.Name == typeof(InpcAttribute).Name);
            }
            else
            {
                lazyFilter = p => p.GetMethod != null && p.SetMethod == null;
                InpcFilter = p => p.GetMethod != null && p.SetMethod != null;
            }

            var lazy = props.Where(lazyFilter);
            var inpc = props.Where(InpcFilter).ToArray();
            foreach (var p in lazy)
            {
                ImplementProperty(type, p, isAbstract, isLazy: true);
            }
            if (inpc.Any())
            {
                type.BaseTypes.Add(typeof(INotifyPropertyChanged));
                CodeMemberEvent PropertyChangedEvent = new CodeMemberEvent();
                PropertyChangedEvent.Name = "PropertyChanged";
                PropertyChangedEvent.Type = new CodeTypeReference(typeof(PropertyChangedEventHandler));
                PropertyChangedEvent.Attributes = MemberAttributes.Public;
                type.Members.Add(PropertyChangedEvent);

                CodeMemberMethod OnPropertyChanged = new CodeMemberMethod();
                OnPropertyChanged.Name = "OnPropertyChanged";
                OnPropertyChanged.Attributes = MemberAttributes.Family;
                OnPropertyChanged.Parameters.Add(new CodeParameterDeclarationExpression(
                    new CodeTypeReference(typeof(String)), "Property"));

                //Declare temp variable holding the event
                CodeVariableDeclarationStatement vardec = new CodeVariableDeclarationStatement(
                    new CodeTypeReference(typeof(PropertyChangedEventHandler)), "temp");
                vardec.InitExpression = new CodeEventReferenceExpression(
                    new CodeThisReferenceExpression(), "PropertyChanged");
                OnPropertyChanged.Statements.Add(vardec);

                //The part of the true, create the event and invoke it
                CodeObjectCreateExpression createArgs = new CodeObjectCreateExpression(
                    new CodeTypeReference(typeof(PropertyChangedEventArgs)),
                    new CodeArgumentReferenceExpression("Property"));
                CodeDelegateInvokeExpression raiseEvent = new CodeDelegateInvokeExpression(
                    new CodeVariableReferenceExpression("temp"),
                    new CodeThisReferenceExpression(), createArgs);

                //The conditino
                CodeExpression condition = new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("temp"),
                    CodeBinaryOperatorType.IdentityInequality,
                    new CodePrimitiveExpression(null));

                //The if condition
                CodeConditionStatement ifTempIsNull = new CodeConditionStatement();
                ifTempIsNull.Condition = condition;
                ifTempIsNull.TrueStatements.Add(raiseEvent);
                OnPropertyChanged.Statements.Add(ifTempIsNull);

                type.Members.Add(OnPropertyChanged);
                foreach (var p in inpc)
                    ImplementProperty(type, p, isAbstract, isLazy: false);
            }
            nspace.Types.Add(type);
            unit.Namespaces.Add(nspace);
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
                Environment.Exit(-1);

            var asmPath = args[0];
            if (!File.Exists(asmPath))
                Environment.Exit(-1);

            var outPath = args[1];

            var asm = Assembly.LoadFrom(asmPath);
            var types = asm.GetTypes().Where(t => t.GetCustomAttributesData()
                .Any(a => a.AttributeType.Name == typeof(TypeOverrideAttribute).Name)).ToArray();
            var abstractTypes = types.Where(t => t.IsAbstract && !t.IsInterface);
            var interfaceTypes = types.Where(t => t.IsInterface);

            var compileUnit = new CodeCompileUnit();

            foreach (var t in abstractTypes)
                ImplementType(compileUnit, t, isAbstract: true);

            foreach (var t in interfaceTypes)
                ImplementType(compileUnit, t, isAbstract: false);

            var codeDom = CodeDomProvider.CreateProvider("C#");
            using (var writer = new StreamWriter(outPath))
                codeDom.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
            Console.ReadLine();
        }
    }
}
