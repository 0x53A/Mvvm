using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Mvvm
{
    //TODO: clean up implementation without changing the interface
    public class EventToCommand : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null));
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null));
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            if (Command == null)
                return;

            if (Command is IEventTriggerCommand)
                (Command as IEventTriggerCommand).Execute(parameter, CommandParameter);
            else
            {
                if (CommandParameter == null)
                {
                    if (Command.CanExecute(parameter))
                        Command.Execute(parameter);
                }
                else
                    if (Command.CanExecute(CommandParameter))
                        Command.Execute(CommandParameter);
            }
        }
    }
}
