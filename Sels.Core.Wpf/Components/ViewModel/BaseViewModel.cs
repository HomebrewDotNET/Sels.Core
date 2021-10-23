using Sels.Core.Wpf.Components.Command.DelegateCommand;
using Sels.Core.Wpf.Components.Property;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.Wpf.Components.ViewModel
{
    public abstract class BaseViewModel : BasePropertyChangedNotifier, IDisposable
    {
        // Fields
        public bool Initialized {
            get
            {
                return GetValue<bool>(nameof(Initialized));
            }
            protected set
            {
                SetValue(nameof(Initialized), value);
            }
        }

        // Commands
        /// <summary>
        /// Command that gets called when the control loads
        /// </summary>
        public ICommand InitializeControlCommandAsync { get; }

        public BaseViewModel()
        {
            InitializeControlCommandAsync = CreateAsyncCommand(LoadControl, () => !Initialized, affectedProperties: nameof(Initialized));
        }

        // Manually trigger initialize if viewmodel was already loaded by ui and it won't trigger initialize via BindCommand behaviour
        public void Reinitialize()
        {
            InitializeControlCommandAsync.Execute(null);
        }

        #region Command Creation

        public ICommand CreateCommand(Action executeDelegate, Action<Exception> exceptionHandler, Func<bool> canExecuteDelegate = null, params string[] affectedProperties)
        {
            var command = new DelegateCommand(executeDelegate, canExecuteDelegate, exceptionHandler ?? RaiseExceptionOccured);

            affectedProperties.Execute(x => SubscribeToPropertyChanged<object>(x, (wasChanged, value) => command.RaiseCanExecuteChanged()));

            return command;
        }

        public ICommand CreateCommand<TParameter>(Action<TParameter> executeDelegate, Action<Exception> exceptionHandler, Predicate<TParameter> canExecuteDelegate = null, params string[] affectedProperties)
        {
            var command = new DelegateCommand<TParameter>(executeDelegate, canExecuteDelegate, exceptionHandler ?? RaiseExceptionOccured);

            affectedProperties.Execute(x => SubscribeToPropertyChanged<object>(x, (wasChanged, value) => command.RaiseCanExecuteChanged()));

            return command;
        }

        public ICommand CreateAsyncCommand(Func<Task> executeDelegate, Action<Exception> exceptionHandler, Func<bool> canExecuteDelegate = null, params string[] affectedProperties)
        {
            var command = new AsyncDelegateCommand(executeDelegate, canExecuteDelegate, exceptionHandler ?? RaiseExceptionOccured);

            affectedProperties.Execute(x => SubscribeToPropertyChanged<object>(x, (wasChanged, value) => command.RaiseCanExecuteChanged()));

            return command;
        }

        public ICommand CreateAsyncCommand<TParameter>(Func<TParameter, Task> executeDelegate, Predicate<TParameter> canExecuteDelegate = null, Action<Exception> exceptionHandler = null, params string[] affectedProperties)
        {
            var command = new AsyncDelegateCommand<TParameter>(executeDelegate, canExecuteDelegate, exceptionHandler ?? RaiseExceptionOccured);

            affectedProperties.Execute(x => SubscribeToPropertyChanged<object>(x, (wasChanged, value) => command.RaiseCanExecuteChanged()));

            return command;
        }

        public ICommand CreateCommand(Action executeDelegate, Func<bool> canExecuteDelegate = null, params string[] affectedProperties)
        {
            return CreateCommand(executeDelegate, null, canExecuteDelegate, affectedProperties);
        }

        public ICommand CreateCommand<TParameter>(Action<TParameter> executeDelegate, Predicate<TParameter> canExecuteDelegate = null, params string[] affectedProperties)
        {
            return CreateCommand(executeDelegate, null, canExecuteDelegate, affectedProperties);
        }

        public ICommand CreateAsyncCommand(Func<Task> executeDelegate, Func<bool> canExecuteDelegate = null, params string[] affectedProperties)
        {
            return CreateAsyncCommand(executeDelegate, null, canExecuteDelegate, affectedProperties);
        }

        public ICommand CreateAsyncCommand<TParameter>(Func<TParameter, Task> executeDelegate, Predicate<TParameter> canExecuteDelegate = null, params string[] affectedProperties)
        {
            return CreateAsyncCommand(executeDelegate, canExecuteDelegate, null, affectedProperties);
        }

        #endregion

        private Task LoadControl()
        {
            var task = InitializeControl(); 
            Initialized = true; 
            return task;
        }

        // Events
        /// <summary>
        /// Raised when an exception gets thrown by viewmodel. Can be used to send exceptions to upper viewmodels. First parameter is sender, second is message from viewmodel and third is the exception thrown.
        /// </summary>
        public event Action<object, string, Exception> ExceptionOccured = delegate { };
        protected void RaiseExceptionOccured(Exception exception)
        {
            ExceptionOccured.Invoke(this, string.Empty, exception);   
        }

        protected void RaiseExceptionOccured(string message, Exception exception)
        {
            ExceptionOccured.Invoke(this, message, exception);
        }

        // Virtuals
        /// <summary>
        /// Can be used to perform actions when the control gets loaded
        /// </summary>
        /// <returns></returns>
        protected virtual Task InitializeControl()
        {
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            
        }
    }
}
