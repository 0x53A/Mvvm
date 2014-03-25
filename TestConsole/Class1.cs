using Mvvm;
using Mvvm.Codegen;
using Mvvm.CodeGen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
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

    public class Program
    {
        static void Main(string[] args)
        {
            var asd = new Thing();
            dynamic iAsd = DuckTyping.Cast<IDuck>(asd);
            var foo = iAsd.Foo;
            var bar = iAsd.Bar;
        }
    }
}