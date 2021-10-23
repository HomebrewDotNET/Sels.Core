using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Wpf.Templates.MainWindow.Navigation
{
    public interface INavigator
    {
        event Action<string, object> NavigationRequest;
    }
}
