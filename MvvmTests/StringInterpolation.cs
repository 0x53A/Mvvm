using System;
using Mvvm;
#if UNIVERSAL
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

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
