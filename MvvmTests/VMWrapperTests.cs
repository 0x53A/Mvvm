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
        [TypeOverride]
        public abstract class VmWithInpc : ViewModelBase
        {
            [Inpc]
            public abstract int Foo { get; set; }
            [Inpc]
            public abstract object Bar { get; set; }
            [Lazy]
            public abstract List<int> FooBar { get; }
            [Lazy]
            public abstract List<object> BarFoo { get; }

            int _asd;
            public int Asd { get { return _asd; } set { SetField(ref _asd, value); } }

            public VmWithInpc()
            {

            }
        }

        [TypeOverride]
        public abstract class VmWithoutInpc
        {
            [Inpc]
            public abstract int Foo { get; set; }
            [Inpc]
            public abstract object Bar { get; set; }
            [Lazy]
            public abstract List<int> FooBar { get; }
            [Lazy]
            public abstract List<object> BarFoo { get; }

            public VmWithoutInpc()
            {

            }
        }

        [TestMethod]
        public void TestVMWrapperWithInpc()
        {
            var obj = CG.New<VmWithInpc>();
            int nFoo = 0;
            int nBar = 0;
            int nAsd = 0;
            INPC.Subscribe(obj, o => o.Foo, (a, b) => nFoo += 1);
            INPC.Subscribe(obj, o => o.Bar, (a, b) => nBar += 1);
            INPC.Subscribe(obj, o => o.Asd, (a, b) => nAsd += 1);
            Assert.AreEqual(nFoo, 0);
            Assert.AreEqual(nBar, 0);
            Assert.AreEqual(nAsd, 0);

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

            obj.Asd = 99;
            Assert.AreEqual(nAsd, 1);
            obj.Asd = 88;
            Assert.AreEqual(nAsd, 2);
            obj.Asd = 77;
            Assert.AreEqual(nAsd, 3);
        }

        [TestMethod]
        public void TestVMWrapperWithoutInpc()
        {
            var obj = CG.New<VmWithoutInpc>();
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
