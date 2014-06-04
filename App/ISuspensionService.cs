using Mvvm.FirstClassEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm.App
{
    public interface ISuspensionService
    {
        IEvent<ISuspensionService> Suspending { get; }
        IEvent<ISuspensionService> Resuming { get; }
    }
}
