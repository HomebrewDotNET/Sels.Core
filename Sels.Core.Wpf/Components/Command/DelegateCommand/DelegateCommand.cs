using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Sels.Core.Extensions.Linq;
using Sels.Core.Components.Caching;
using Sels.Core.Components.Variable.VariableActions;

namespace Sels.Core.Wpf.Components.Command.DelegateCommand
{
    public class DelegateCommand : ICommand
    {
        // Properties
        public Func<bool> CanExecuteDelegate { get; set; }
        public Action ExecuteDelegate { get; set; }
        public Action<Exception> ExceptionHandler { get; set; }

        private ValueCache<bool> InProgress { get; }

        // Events
        public event EventHandler CanExecuteChanged = delegate{};
        public DelegateCommand(Action executeDelegate, Func<bool> canExecuteDelegate = null, Action<Exception> exceptionHandler = null)
        {
            executeDelegate.ValidateVariable(nameof(executeDelegate));

            ExecuteDelegate = executeDelegate;
            CanExecuteDelegate = canExecuteDelegate;
            ExceptionHandler = exceptionHandler;

            InProgress = new ValueCache<bool>(() => false);
        }

        public bool CanExecute(object parameter)
        {
            try
            {
                if (CanExecuteDelegate.HasValue())
                {
                    return !InProgress.Value && CanExecuteDelegate();
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ForceExecuteOrDefault(ex);
            }

            return !InProgress.Value;
        }

        public void Execute(object parameter)
        {
            try
            {
                if (CanExecute(parameter))
                {
                    using (new InProgressAction(x => InProgress.Set(x)))
                    {
                        ExecuteDelegate();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ForceExecuteOrDefault(ex);
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }
    }

    public class DelegateCommand<TParameter> : ICommand
    {
        // Properties
        public Predicate<TParameter> CanExecuteDelegate { get; set; }
        public Action<TParameter> ExecuteDelegate { get; set; }
        public Action<Exception> ExceptionHandler { get; set; }

        private ValueCache<bool> InProgress { get; }

        // Events
        public event EventHandler CanExecuteChanged = delegate { };
        public DelegateCommand(Action<TParameter> executeDelegate, Predicate<TParameter> canExecuteDelegate = null, Action<Exception> exceptionHandler = null)
        {
            executeDelegate.ValidateVariable(nameof(executeDelegate));

            ExecuteDelegate = executeDelegate;
            CanExecuteDelegate = canExecuteDelegate;
            ExceptionHandler = exceptionHandler;

            InProgress = new ValueCache<bool>(() => false);
        }

        public bool CanExecute(object parameter)
        {
            if(parameter is TParameter typedParameter)
            {
                try
                {
                    if (CanExecuteDelegate.HasValue())
                    {
                       return !InProgress.Value && CanExecuteDelegate(typedParameter);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler.ForceExecuteOrDefault(ex);
                }

                return !InProgress.Value;
            }
            return false;
        }

        public void Execute(object parameter)
        {
            try
            {               
                if (CanExecute(parameter))
                {
                    using (new InProgressAction(x => InProgress.Set(x)))
                    {
                        ExecuteDelegate((TParameter)parameter);
                    }                    
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ForceExecuteOrDefault(ex);
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }
    }
}
