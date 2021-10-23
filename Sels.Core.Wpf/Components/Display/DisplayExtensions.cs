using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sels.Core.Wpf.Components.Display
{
    public static class DisplayExtensions
    {
        public static void AddMessage(this ObservableCollection<ProgressMessage> source, string message)
        {
            source.AddMessage(DateTime.Now, message);
        }

        public static void AddMessage(this ObservableCollection<ProgressMessage> source, DateTime date, string message)
        {
            var progressMessage = new ProgressMessage(date, message);

            source.Add(progressMessage);
        }
    }
}
