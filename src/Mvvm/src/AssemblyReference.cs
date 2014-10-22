using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public static class AssemblyReference<T>
    {
        public static void Do()
        {
            var t = typeof(T);
        }
    }
}
