using Sels.Core.Extensions;
using Sels.Core.Wpf.Components.Command.DelegateCommand;
using Sels.Core.Wpf.Components.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Sels.Core.Extensions.Reflection;
using System.Collections.ObjectModel;
using System.Linq;
using Sels.Core;

namespace Sels.Core.Wpf.Templates.MainWindow.Navigation
{
    public abstract class NavigatableViewModel<TDefaultPage> : BaseViewModel where TDefaultPage : BaseViewModel
    {
        // Constants
        private const char NavigateOptionSplit = ',';

        // Properties       
        public BaseViewModel CurrentControl {
            get
            {
                return GetValue<BaseViewModel>(nameof(CurrentControl));
            }
            set
            {
                SetValue(nameof(CurrentControl), value, () => { SubscribeToExceptionOccuredEvents(CurrentControl); SubscribeToNavigatorEvents(CurrentControl); });
            }
        }
        public ObservableCollection<(BaseViewModel ViewModel, object Context)> NavigationHistory {
            get
            {
                return GetValue<ObservableCollection<(BaseViewModel ViewModel, object Context)>>(nameof(NavigationHistory));
            }
            set
            {
                SetValue(nameof(NavigationHistory), value);
            }
        }

        // Commands
        public ICommand InitializeCommand { get; }
        public ICommand NavigateCommand { get; }

        public NavigatableViewModel()
        {
            NavigationHistory = new ObservableCollection<(BaseViewModel ViewModel, object Context)>();

            // Setup commands
            NavigateCommand = CreateCommand<string>(Navigate);

            // Setup Exception Handler on main window
            this.ExceptionOccured += ExceptionHandler;

            // Dispose current control when app closes
            Helper.App.RegisterApplicationClosingAction(() => CurrentControl?.Dispose());
        }

        protected override Task InitializeControl()
        {
            Initialize();

            Navigate(ResolveViewModel<TDefaultPage>());

            return Task.CompletedTask;
        }

        private void SubscribeToExceptionOccuredEvents(BaseViewModel viewModel)
        {
            if (viewModel.HasValue())
            {
                viewModel.ExceptionOccured += ExceptionHandler;

                foreach(var property in viewModel.GetProperties())
                {
                    var propertyValue = property.GetValue(viewModel);

                    if(propertyValue.HasValue() && propertyValue is BaseViewModel subViewModel)
                    {
                        SubscribeToExceptionOccuredEvents(subViewModel);
                    }
                }
            }
        }

        private void SubscribeToNavigatorEvents(BaseViewModel viewModel)
        {
            if (viewModel.HasValue())
            {
                if(viewModel is INavigator navigator)
                {
                    navigator.NavigationRequest += NavigationRequestHandler;
                }

                if (viewModel is IHomeNavigator homeNavigator)
                {
                    homeNavigator.HomeNavigationRequest += HomeNavigationRequestHandler;
                }

                foreach (var property in viewModel.GetProperties())
                {
                    var propertyValue = property.GetValue(viewModel);

                    if (propertyValue.HasValue() && propertyValue is BaseViewModel subViewModel)
                    {
                        SubscribeToNavigatorEvents(subViewModel);
                    }
                }
            }
        }

        private void SetNavigationContext(BaseViewModel viewModel, object context)
        {
            if (viewModel.HasValue())
            {
                if (viewModel is INavigatable navigatable)
                {
                    navigatable.SetNavigationContext(context);
                }

                foreach (var property in viewModel.GetProperties())
                {
                    var propertyValue = property.GetValue(viewModel);

                    if (propertyValue.HasValue() && propertyValue is BaseViewModel subViewModel)
                    {
                        SetNavigationContext(subViewModel, context);
                    }
                }
            }
        }

        #region Navigation
        private void Navigate(string options)
        {
            options.ValidateVariable(nameof(options));

            var splitOptions = options.Split(NavigateOptionSplit);

            Navigate(splitOptions[0], splitOptions.Length > 1 ? splitOptions[1] : null);
        }

        private void Navigate(string viewModelName, string contextName = null)
        {
            viewModelName.ValidateVariable(nameof(viewModelName));

            Navigate(ResolveViewModel(viewModelName), contextName.HasValue() ? ResolveNavigationContext(contextName) : null);
        }

        private void Navigate(BaseViewModel viewToNavigateTo, object context = null)
        {
            try
            {
                viewToNavigateTo.ValidateVariable(nameof(viewToNavigateTo));

                var lastNavigation = NavigationHistory.LastOrDefault();

                CurrentControl?.Dispose();
                                
                SetNavigationContext(viewToNavigateTo, context);

                CurrentControl = viewToNavigateTo;

                NavigationHistory.Add((viewToNavigateTo, context));
            }
            catch(Exception ex)
            {
                RaiseExceptionOccured(ex);
            }         
        }
        #endregion

        // Event Handlers
        private void NavigationRequestHandler(string viewModelName, object context)
        {
            try
            {
                var viewModel = ResolveViewModel(viewModelName);
                Navigate(viewModel, context);
            }
            catch(Exception ex)
            {
                RaiseExceptionOccured(ex);
            }
        }

        private void HomeNavigationRequestHandler()
        {
            try
            {
                Navigate(ResolveViewModel<TDefaultPage>());
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }
        }

        // Abstractions
        /// <summary>
        /// Initialize view model
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// Resolves context object by name
        /// </summary>
        /// <param name="contextName">Name of context</param>
        /// <returns></returns>
        public abstract object ResolveNavigationContext(string contextName);
        /// <summary>
        /// Used to resolve the view model
        /// </summary>
        /// <param name="viewModelName">View model to resolve</param>
        /// <returns></returns>
        public abstract BaseViewModel ResolveViewModel(string viewModelName);
        /// <summary>
        /// Resolve view model by type
        /// </summary>
        /// <typeparam name="TViewModel">Type of view model to resolve</typeparam>
        /// <returns></returns>
        public abstract BaseViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel;
        public abstract void ExceptionHandler(object sender, string senderMessage, Exception exceptionToHandle);
    }
}
