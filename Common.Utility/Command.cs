using System;
using System.Windows.Input;

namespace Common.Utility
{

    /// <inheritdoc cref="ICommand"/>
    public abstract class Command : MarkupExtension, ICommand
    {

        public event EventHandler CanExecuteChanged;

        /// <inheritdoc cref="ICommand.CanExecute(object?)"/>
        public virtual bool CanExecute() =>
            true;

        /// <inheritdoc cref="ICommand.Execute(object?)"/>
        public abstract void Execute();

        bool ICommand.CanExecute(object _) =>
            CanExecute();

        void ICommand.Execute(object _) =>
            Execute();

    }

    /// <inheritdoc cref="ICommand"/>
    public abstract class Command<T> : MarkupExtension, ICommand
    {

        public event EventHandler CanExecuteChanged;

        /// <inheritdoc cref="ICommand.CanExecute(object?)"/>
        public virtual bool CanExecute(T parameter) =>
            true;

        /// <inheritdoc cref="ICommand.Execute(object?)"/>
        public abstract void Execute(T parameter);

        bool ICommand.CanExecute(object parameter)
        {

            if (parameter is null || parameter is T)
                return CanExecute((T)parameter);
            return false;

        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is null || parameter is T)
                Execute((T)parameter);
        }

    }

}
