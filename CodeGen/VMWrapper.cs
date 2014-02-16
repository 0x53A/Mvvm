using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.CodeGen
{
    /// <summary>
    /// Notifies the VMWrapper that this Property should be overridden and implement INotifyPropertyChanged.
    /// It can only be applied to properties with getter and setter
    /// </summary>
    public class InpcAttribute : Attribute
    {

    }

    /// <summary>
    /// Notifies the VMWrapper that this property should be overridden and implemented using Lazy initialization.
    /// It can only be applied to get-only properties
    /// </summary>
    public class LazyAttribute : Attribute
    {

    }

    public static class VMWrapper
    {
        public static T Wrap<T>()
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}
