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
            var source = CG.New<IStuff>();
            var destination = CG.New<IStuff>();

            source.Foo = CG.New<IStuff2>();
            destination.Foo = CG.New<IStuff2>();

            source.Foo.Bar = CG.New<IStuff3>();
            destination.Foo.Bar = CG.New<IStuff3>();

            var binding = INPC.Bind(source, _ => _.Foo.Bar.FooBar, destination, _ => _.Foo.Bar.FooBar, BindingMode.TwoWay);
            source.Foo.Bar.FooBar = "Hello, World!";
            Assert.IsTrue(source.Foo.Bar.FooBar == destination.Foo.Bar.FooBar);
        }
    }
}
