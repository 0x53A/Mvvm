using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mvvm
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Func<object, bool> _canExecute;
        Action<object> _execute;

        public RelayCommand(Action execute)
            : this((o) => execute(), (o) => true)
        {

        }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(o => execute(), o => canExecute())
        {

        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public void FireExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    public class RelayCommand<T> : RelayCommand
    {
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
            : base((o) => execute((T)o), (o) => canExecute((T)o))
        {

        }
    }
}
