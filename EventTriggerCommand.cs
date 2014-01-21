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

        public EventTriggerCommand(Action<object,object> execute) : base(o=>execute(o, null), o=>true)
        {
            _execute = execute;
        }
        void IEventTriggerCommand.Execute(object eventArg, object triggerArg)
        {
            _execute(eventArg, triggerArg);
        }
    }

    public class EventTriggerCommand<T_EV, T_P> : EventTriggerCommand
    {
        public EventTriggerCommand(Action<T_EV, T_P> execute) : base((a,b)=>execute((T_EV)a, (T_P)b))
        {

        }
    }
}
