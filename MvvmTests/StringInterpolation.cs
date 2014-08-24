using System;
using Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmTests
{
    [TestClass]
    public class StringInterpolation
    {
        [TestMethod]
        public void Interpolation()
        {
            Assert.AreEqual("Hello World", "{hello} {world}".Interpolate(new{hello="Hello", world="World"}));
        }
    }
}
