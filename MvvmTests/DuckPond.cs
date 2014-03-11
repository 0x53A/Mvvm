using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var iAsd = DuckTypingFS.DuckTyping.Cast<IDuck>(asd);
            var foo = iAsd.Foo;
            var bar = iAsd.Bar;
        }
    }
}
