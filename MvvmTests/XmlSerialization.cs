using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Mvvm.CodeGen;
using Mvvm;

namespace MvvmTests
{
    [TestClass]
    public class XmlSerialization
    {
        public interface IA
        {
            int FooA { get; set; }
            int FooB { get; set; }
            IB B { get; set; }
        }

        public interface IB
        {
            int FooA { get; set; }
            int FooB { get; set; }
            List<IC> CList { get; set; }
        }

        public interface IC
        {
            int FooC { get; set; }
            int BarC { get; set; }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var ia = CG.New<IA>();
            ia.FooA = 1;
            ia.FooB = 2;
            ia.B = CG.New<IB>();
            ia.B.FooA = 3;
            ia.B.FooB = 4;
            ia.B.CList = new List<IC>();
            ia.B.CList.Add(CG.New<IC>((x)=>x.BarC=x.FooC=5));
            Assert.Fail();
            //var xml = CG.SerializeInterface(ia);
            //var obj = CG.DeserializeInterface<IA>(xml);
        }
    }
}
