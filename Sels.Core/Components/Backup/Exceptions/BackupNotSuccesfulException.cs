using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Backup.Exceptions
{
    public class BackupNotSuccesfulException : Exception
    {
        private const string _messageFormat = "Backup for {0} was not succesful.";
        private const string _messageFormatReason = "Backup for {0} was not succesful. Reason: {1}";

        public BackupNotSuccesfulException(string identifier) : base(_messageFormat.FormatString(identifier))
        {

        }

        public BackupNotSuccesfulException(string identifier, Exception reason) : base(_messageFormat.FormatString(identifier, reason), reason)
        {

        }
    }
}
