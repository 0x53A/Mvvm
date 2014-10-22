using System;
#if UNIVERSAL
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System.Collections.Generic;
using Mvvm.CodeGen;
using Mvvm;

namespace MvvmTests
{
    [TypeOverride]
    public interface IA
    {
        int FooA { get; set; }
        int FooB { get; set; }
        IB B { get; set; }
    }

    [TypeOverride]
    public interface IB
    {
        int FooA { get; set; }
        int FooB { get; set; }
        List<IC> CList { get; set; }
    }

    [TypeOverride]
    public interface IC
    {
        int FooC { get; set; }
        int BarC { get; set; }
    }

    [TestClass]
    public class XmlSerialization
    {
        [TestMethod]
        public void TestSerialization()
        {
            int __i = 0;
            Func<int> next = () => __i++;
            var ia = CG.New<IA>();
            ia.FooA = next();
            ia.FooB = next();
            ia.B = CG.New<IB>();
            ia.B.FooA = next();
            ia.B.FooB = next();
            ia.B.CList = new List<IC>();
            for (int i = 0; i < 10; i++)
                ia.B.CList.Add(CG.New<IC>((x) => { x.BarC = next(); x.FooC = next(); }));

            var str = InterfaceSerialization.Serialize(ia);
            var ia_clone = InterfaceSerialization.Deserialize<IA>(str);

            Assert.AreEqual(ia.FooA, ia_clone.FooA);
            Assert.AreEqual(ia.FooB, ia_clone.FooB);
            Assert.AreEqual(ia.B.FooA, ia_clone.B.FooA);
            Assert.AreEqual(ia.B.FooB, ia_clone.B.FooB);
            Assert.AreEqual(ia.B.CList.Count, ia_clone.B.CList.Count);
            for (int i = 0; i < ia.B.CList.Count; i++)
            {
                Assert.AreEqual(ia.B.CList[i].BarC, ia_clone.B.CList[i].BarC);
                Assert.AreEqual(ia.B.CList[i].FooC, ia_clone.B.CList[i].FooC);
            }
        }
    }
}
