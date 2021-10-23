using Sels.Core.Wpf.Components.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Wpf.Components.Display
{
    public class ProgressMessage : BaseModel
    {
        public ProgressMessage(DateTime date, string message)
        {
            Date = date;
            Message = message;
        }
        public ProgressMessage()
        {

        }
        public DateTime Date {
            get
            {
                return GetValue<DateTime>(nameof(Date));
            }
            set
            {
                SetValue(nameof(Date), value);
            }
        }
        public string Message {
            get
            {
                return GetValue<string>(nameof(Message));
            }
            set
            {
                SetValue(nameof(Message), value);
            }
        }
    }
}
