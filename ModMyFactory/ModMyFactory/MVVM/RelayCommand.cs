using System;
using System.Windows.Input;

namespace ModMyFactory.MVVM
{
    /// <summary>
    /// A relay command that does not take parameters.
    /// </summary>
    sealed class RelayCommand : ICommand
    {
        readonly Action methodToExecute;
        readonly Func<bool> canExecuteEvaluator;

        /// <summary>
        /// Is raised when the ability of this command to be executed changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Creates a relay command.
        /// </summary>
        /// <param name="methodToExecute">A delegate that is invoked when the command is executed.</param>
        /// <param name="canExecuteEvaluator">A delegate that specifies if the command can be executed.</param>
        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
        {
            this.methodToExecute = methodToExecute;
            this.canExecuteEvaluator = canExecuteEvaluator;
        }

        /// <summary>
        /// Creates a relay command.
        /// </summary>
        /// <param name="methodToExecute">A delegate that is invoked when the command is executed.</param>
        public RelayCommand(Action methodToExecute)
            : this(methodToExecute, () => true)
        { }

        /// <summary>
        /// Evaluates if this command can be executed.
        /// </summary>
        /// <returns>Returns true if this command can be executed, otherwise false.</returns>
        public bool CanExecute()
        {
            return canExecuteEvaluator.Invoke();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        public void Execute()
        {
            methodToExecute.Invoke();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }
    }

    /// <summary>
    /// A relay command that takes one parameter.
    /// </summary>
    /// <typeparam name="T">The type of the commands parameter.</typeparam>
    sealed class RelayCommand<T> : ICommand
    {
        readonly Action<T> methodToExecute;
        readonly Func<T, bool> canExecuteEvaluator;

        /// <summary>
        /// Is raised when the ability of this command to be executed changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Creates a relay command.
        /// </summary>
        /// <param name="methodToExecute">A delegate that is invoked when the command is executed.</param>
        /// <param name="canExecuteEvaluator">A delegate that specifies if the command can be executed.</param>
        public RelayCommand(Action<T> methodToExecute, Func<T, bool> canExecuteEvaluator)
        {
            this.methodToExecute = methodToExecute;
            this.canExecuteEvaluator = canExecuteEvaluator;
        }

        /// <summary>
        /// Creates a relay command.
        /// </summary>
        /// <param name="methodToExecute">A delegate that is invoked when the command is executed.</param>
        /// <param name="canExecuteEvaluator">A delegate that specifies if the command can be executed.</param>
        public RelayCommand(Action<T> methodToExecute, Func<bool> canExecuteEvaluator)
            : this(methodToExecute, parameter => canExecuteEvaluator.Invoke())
        { }

        /// <summary>
        /// Creates a relay command.
        /// </summary>
        /// <param name="methodToExecute">A delegate that is invoked when the command is executed.</param>
        public RelayCommand(Action<T> methodToExecute)
            : this(methodToExecute, parameter => true)
        { }

        /// <summary>
        /// Evaluates if this command can be executed.
        /// </summary>
        /// <param name="parameter">The parameter passed to the command.</param>
        /// <returns>Returns true if this command can be executed, otherwise false.</returns>
        public bool CanExecute(T parameter)
        {
            return canExecuteEvaluator.Invoke(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">The parameter passed to the command.</param>
        public void Execute(T parameter)
        {
            methodToExecute.Invoke(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }
    }
}
