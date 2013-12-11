using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    public class EventTriggerCommand : RelayCommand, IEventTriggerCommand
    {
        Action<object, object> _execute;
        public EventTriggerCommand(Action<object,object> execute, Func<object,bool> canExecute) : base(o=>execute(o, null), canExecute)
        {
            _execute = execute;
        }
        void IEventTriggerCommand.Execute(object eventArg, object triggerArg)
        {
            _execute(eventArg, triggerArg);
        }
    }
}
