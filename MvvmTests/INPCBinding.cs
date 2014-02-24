using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mvvm;
using Mvvm.CodeGen;

namespace MvvmTests
{

    public interface IStuff
    {
        IStuff2 Foo { get; set; }
    }

    public interface IStuff2
    {
        IStuff3 Bar { get; set; }
    }

    public interface IStuff3
    {
        string FooBar { get; set; }
    }

    [TestClass]
    public class INPCBindingTests
    {
        [TestMethod]
        public void TestBinding()
        {
            var source = DBCGenerator.Generate<IStuff>();
            var destination = DBCGenerator.Generate<IStuff>();

            source.Foo = DBCGenerator.Generate<IStuff2>();
            destination.Foo = DBCGenerator.Generate<IStuff2>();

            source.Foo.Bar = DBCGenerator.Generate<IStuff3>();
            destination.Foo.Bar = DBCGenerator.Generate<IStuff3>();

            var binding = INPC.Bind(source, _ => _.Foo.Bar.FooBar, destination, _ => _.Foo.Bar.FooBar, BindingMode.TwoWay);
            source.Foo.Bar.FooBar = "Hello, World!";
            Assert.IsTrue(source.Foo.Bar.FooBar == destination.Foo.Bar.FooBar);
        }
    }
}
