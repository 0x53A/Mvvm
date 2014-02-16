using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mvvm;
using Mvvm.CodeGen;
using System.Collections.Generic;

namespace MvvmTests
{
    [TestClass]
    public class VMWrapperTests
    {
        public abstract class MyViewModel : ViewModelBase
        {
            [Inpc]
            public abstract int Foo { get; set; }
            [Inpc]
            public abstract object Bar { get; set; }
            [Lazy]
            public abstract List<int> FooBar { get; }
            [Lazy]
            public abstract List<object> BarFoo { get; }
        }

        [TestMethod]
        public void TestVMWrapper()
        {
            var obj = VMWrapper.Wrap<MyViewModel>();
            int nFoo = 0;
            int nBar = 0;
            INPC.Subscribe(obj, o => o.Foo, (a, b) => nFoo += 1);
            INPC.Subscribe(obj, o => o.Bar, (a, b) => nBar += 1);
            Assert.AreEqual(nFoo, 0);
            Assert.AreEqual(nBar, 0);

            obj.Bar = 55;
            Assert.AreEqual(nBar, 1);
            obj.Bar = 66;
            Assert.AreEqual(nBar, 2);

            obj.Foo = 99;
            Assert.AreEqual(nFoo, 1);
            obj.Foo = 88;
            Assert.AreEqual(nFoo, 2);
            obj.Foo = 77;
            Assert.AreEqual(nFoo, 3);
        }
    }
}
