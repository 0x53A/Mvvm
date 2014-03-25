using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mvvm.Math3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MvvmTests
{
    [TestClass]
    public class Math3D
    {
        [TestMethod]
        public void TestPlane()
        {
            var v3 = new Vector3(1, 2, 3);

            var plane = new Plane(new Point3(0, 0, -10), new Vector3(0, 1, 0), new Vector3(1, 0, 0));
            var mapped = plane.Map(new Point2(2, 5));
            Assert.IsTrue(mapped.Z == -10);
            Assert.IsTrue(mapped.X == 5);
            Assert.IsTrue(mapped.Y == 2);
        }
    }
}
