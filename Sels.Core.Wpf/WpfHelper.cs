using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Sels.Core.Extensions;

namespace Sels.Core.Wpf
{
    public static class WpfHelper
    {
        #region Async
        public static class Async
        {
            public static void Do(Action action, object lockObject)
            {
                action.ValidateVariable(nameof(action));
                lockObject.ValidateVariable(nameof(lockObject));

                lock (lockObject)
                {
                    Application.Current.Dispatcher.Invoke(action);
                }
            }
        }
        #endregion
    }
}
