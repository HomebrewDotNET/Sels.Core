using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Sels.Core.Wpf.Components.Behaviours
{
    public static class LoadedCommandBehaviour
    {
        public static object GetLoadedCommand(DependencyObject obj)
        {
            return (object)obj.GetValue(LoadedCommandProperty);
        }

        public static void SetLoadedCommand(DependencyObject obj, string value)
        {
            obj.SetValue(LoadedCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for LoadedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadedCommandProperty =
            DependencyProperty.RegisterAttached("LoadedCommand", typeof(object), typeof(LoadedCommandBehaviour), new PropertyMetadata(null, LoadedCommandChanged));

        private static void LoadedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;


            if (d is FrameworkElement frameworkElement)
            {
                frameworkElement.Loaded
                  += (o, args) =>
                  {
                      if(e.NewValue is ICommand command && command != null)
                      {
                          command.Execute(null);
                      }
                      
                  };
            }
        }
    }
}
