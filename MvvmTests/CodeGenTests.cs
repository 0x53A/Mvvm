using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mvvm.CodeGen;
using System.ComponentModel;
using System.Collections.Generic;

namespace MvvmTests
{
    [TestClass]
    public class CodeGenTests
    {
        public class ClassWithoutStandardConstructor
        {
            public ClassWithoutStandardConstructor(string someArgument)
            {

            }
        }

        [TypeOverride]
        public interface TestInterfaceValid
        {
            string Asd1 { get; set; }
            int Asd2 { get; set; }
        }

        [TypeOverride]
        public interface TestInterfaceLazy
        {
            List<int> FooBar { get; }
            List<string> BarFoo { get; }
        }

        [TestMethod]
        public void TestCreationReadWrite1()
        {
            var val = DBCGenerator.Generate<TestInterfaceValid>();
            Assert.IsNotNull(val);
            Assert.IsTrue(val is INotifyPropertyChanged);
            val.Asd1 = "Hello, World!";
            Assert.IsTrue(val.Asd1 == "Hello, World!");
            val.Asd2 = 55;
            Assert.IsTrue(val.Asd2 == 55);
        }

        [TestMethod]
        public void TestCreationReadWrite2()
        {
            var val = (TestInterfaceValid)DBCGenerator.Generate(typeof(TestInterfaceValid));
            Assert.IsNotNull(val);
            Assert.IsTrue(val is INotifyPropertyChanged);
            val.Asd1 = "Hello, World!";
            Assert.IsTrue(val.Asd1 == "Hello, World!");
            val.Asd2 = 55;
            Assert.IsTrue(val.Asd2 == 55);
        }
        
        [TestMethod]
        public void TestCreationLazy()
        {
            {
                var val = DBCGenerator.Generate<TestInterfaceLazy>();
                Assert.IsNotNull(val);
                Assert.IsTrue(val is INotifyPropertyChanged);
                Assert.IsTrue(val.FooBar != null);
                Assert.IsTrue(val.BarFoo != null);
                val.FooBar.Add(99);
                val.BarFoo.Add("Hello, World!");
            }

            {
                var val = (TestInterfaceLazy)DBCGenerator.Generate(typeof(TestInterfaceLazy));
                Assert.IsNotNull(val);
                Assert.IsTrue(val is INotifyPropertyChanged);
                Assert.IsTrue(val.FooBar != null);
                Assert.IsTrue(val.BarFoo != null);
                val.FooBar.Add(99);
                val.BarFoo.Add("Hello, World!");
            }
        }

        /* Test Failure */

        public interface TestInterfaceLazyInvalid
        {
            ClassWithoutStandardConstructor Foo { get; }
        }

        public interface TestInterfaceInvalid
        {
            void Foo();
            string Asd1 { get; set; }
            int Asd2 { get; set; }
        }

        [TestMethod]
        public void TestLazyFailure()
        {
            try
            {
                var val = DBCGenerator.Generate<TestInterfaceLazyInvalid>();
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void TestFailure()
        {
            try
            {
                var val = DBCGenerator.Generate<TestInterfaceInvalid>();
                Assert.Fail();
            }
            catch { }
        }
    }
}
