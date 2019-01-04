using System;
using System.Windows.Input;

namespace SocketClientTestWpf.Frame
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        Func<object, bool> canExecuteFunc;
        Action<object> executeAction;

        public RelayCommand(Action<object> executeAction) : this(o => true, executeAction)
        {
        }

        public RelayCommand(Func<object, bool> canExecuteFunc, Action<object> executeAction)
        {
            this.canExecuteFunc = canExecuteFunc;
            this.executeAction = executeAction;
        }

        public bool CanExecute(object parameter) => canExecuteFunc(parameter);

        public void Execute(object parameter) => executeAction(parameter);
    }
}
