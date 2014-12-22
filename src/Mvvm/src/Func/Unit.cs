using System;
using System.Collections.Generic;
using System.Text;

namespace Mvvm.Func
{
    public sealed class Unit
    {
        private readonly Unit _instance = new Unit();
        public Unit Instance { get { return _instance; } }
        private Unit() { }
    }
}
