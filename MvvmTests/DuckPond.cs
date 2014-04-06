using Mvvm.CodeGen;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mvvm.Codegen;

namespace MvvmTests
{
    public interface IDuck
    {
        int Foo { get; set; }
        int Bar { get; set; }
    }


    public class Thing
    {
        public int Foo { get; set; }
        public int Bar { get; set; }
    }

    [TestClass]
    public class DuckPond
    {
        [TestMethod]
        public void TestDucks()
        {
            var asd = new Thing();
            var iAsd = DuckTyping.Cast<IDuck>(asd);
            var foo = iAsd.Foo;
            var bar = iAsd.Bar;
        }

        public interface IAnonDuck
        {
            int Foo { get; }
            int Bar { get; }
        }

        [TestMethod]
        public void TestAnonymousDuck()
        {
            var thing = new { Foo = 5, Bar = 66 };
            IAnonDuck iDuck = DuckTyping.Cast<IAnonDuck>(thing);
            var foo = iDuck.Foo;
            var bar = iDuck.Bar;
            Assert.AreEqual(thing.Foo, foo);
            Assert.AreEqual(thing.Bar, bar);
            Assert.AreEqual(thing.Foo, 5);
            Assert.AreEqual(thing.Bar, 66);
        }
    }
}
