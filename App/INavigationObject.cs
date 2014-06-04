using Mvvm.FirstClassEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public interface INavigationObject
    {
        IEvent<INavigationObject> NavigatedFrom { get; }
        IEvent<INavigationObject> NavigatedTo { get; }
    }
}
