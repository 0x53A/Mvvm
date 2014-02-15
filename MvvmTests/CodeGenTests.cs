using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mvvm.CodeGen;
using System.ComponentModel;

namespace MvvmTests
{
    [TestClass]
    public class CodeGenTests
    {
        public interface TestInterfaceValid
        {
            string Asd1 { get; set; }
            int Asd2 { get; set; }
            object Asd3 { get; set; }
            string Asd4 { get; set; }
        }

        public interface TestInterfaceInvalid
        {
            void Foo();
            string Asd1 { get; set; }
            int Asd2 { get; set; }
        }

        [TestMethod]
        public void TestCreation()
        {
            TestInterfaceValid val = DBCGenerator.Generate<TestInterfaceValid>();
            Assert.IsNotNull(val);
            Assert.IsTrue(val is INotifyPropertyChanged);
            val.Asd1 = "Hello, World!";
            Assert.IsTrue(val.Asd1 == "Hello, World!");
        }

        [TestMethod]
        public void TestFailure()
        {
            try
            {
                var val = DBCGenerator.Generate<TestInterfaceInvalid>();
                Assert.Fail();
            }
            catch
            {
            }
        }
    }
}
