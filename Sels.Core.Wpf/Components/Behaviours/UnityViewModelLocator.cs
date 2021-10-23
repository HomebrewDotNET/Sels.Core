using Sels.Core.Unity.Components.Containers;
using Sels.Core.Wpf.Components.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Wpf.Components.Behaviours
{
    public static class UnityViewModelLocator
    {
        private const string ViewToViewModelFormat = "{0}ViewModel";
        private const string MainWindowName = "MainWindow";
        private const string MainWindowViewModelTypeFormat = "{0}ViewModel";
        private const char TypeSplitChar = ',';

        public static int GetViewModel(DependencyObject obj)
        {
            return (int)obj.GetValue(ViewModelProperty);
        }

        public static void SetViewModel(DependencyObject obj, int value)
        {
            obj.SetValue(ViewModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.RegisterAttached("ViewModel", typeof(int), typeof(UnityViewModelLocator), new PropertyMetadata(0, ViewModelChanged));

        private static void ViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;
            var viewName = d.GetType().Name;

            // Manually construct MainWindowViewModel because container isn't built yet
            if (viewName.Equals(MainWindowName))
            {
                var splitViewModelTypeName = d.GetType().AssemblyQualifiedName.Split(TypeSplitChar);
                splitViewModelTypeName[0] = MainWindowViewModelTypeFormat.FormatString(splitViewModelTypeName[0]);
                var viewModelType = Type.GetType(splitViewModelTypeName.JoinString(TypeSplitChar.ToString()));
                var viewModel = viewModelType.Construct();

                ((FrameworkElement)d).DataContext = viewModel;
            }
            else
            {
                var viewModelName = ViewToViewModelFormat.FormatString(viewName);

                var viewModel = UnityFactory.Resolve<BaseViewModel>(viewModelName);

                ((FrameworkElement)d).DataContext = viewModel;
            }

            
        }
    }
}
