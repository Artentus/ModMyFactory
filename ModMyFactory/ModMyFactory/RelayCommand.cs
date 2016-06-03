using System;
using System.Windows.Input;

namespace ModMyFactory
{
    class RelayCommand : ICommand
    {
        readonly Action methodToExecute;
        readonly Func<bool> canExecuteEvaluator;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
        {
            this.methodToExecute = methodToExecute;
            this.canExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action methodToExecute)
            : this(methodToExecute, () => true)
        { }

        public bool CanExecute(object parameter)
        {
            return canExecuteEvaluator.Invoke();
        }

        public void Execute(object parameter)
        {
            methodToExecute.Invoke();
        }
    }
}
