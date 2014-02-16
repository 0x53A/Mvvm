using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mvvm;
using Mvvm.CodeGen;

namespace MvvmTests
{
    [TestClass]
    public class INPCTests
    {
        public interface IASD
        {
            string Foo { get; set; }
            int Bar { get; set; }
        }

        [TestMethod]
        public void TestSubscribe()
        {
            int nFoo = 0;
            int nBar = 0;
            var obj = DBCGenerator.Generate<IASD>(o=>o.Foo = "");
            INPC.Subscribe(obj, o => o.Foo, (a, b) => { nFoo += 1; });

            Assert.AreEqual(nFoo, 0);
            obj.Foo = "Hello";
            Assert.AreEqual(nFoo, 1);
            obj.Foo = "World";
            Assert.AreEqual(nFoo, 2);
            obj.Foo = "Foobar";
            Assert.AreEqual(nFoo, 3);

            INPC.Subscribe(obj, o => o.Bar, (a, b) => nBar += 1);
            Assert.AreEqual(nBar, 0);
            obj.Bar = 101;
            Assert.AreEqual(nBar, 1);
            obj.Bar = 102;
            Assert.AreEqual(nBar, 2);
            obj.Bar = 103;
            Assert.AreEqual(nBar, 3);
        }

        [TestMethod]
        public void TestIs()
        {
            Assert.IsTrue(INPC<IASD>.Is(_ => _.Foo, "Foo"));
            Assert.IsFalse(INPC<IASD>.Is(_ => _.Foo, "Bar"));

            Assert.IsTrue(INPC<IASD>.Is(_ => _.Bar, "Bar"));
            Assert.IsFalse(INPC<IASD>.Is(_ => _.Bar, "Foo"));
        }
    }
}
