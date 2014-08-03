using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mvvm
{
    public interface IEventTriggerCommand : ICommand
    {
        void Execute(object eventArg, object triggerArg);
    }
}
