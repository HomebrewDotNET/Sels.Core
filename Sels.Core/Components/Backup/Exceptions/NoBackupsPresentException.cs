using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Backup.Exceptions
{
    public class NoBackupsPresentException : Exception
    {
        private const string _messageFormat = "No BackUps present for {0} in {1}";

        public NoBackupsPresentException(string backUp, string backUpDirectory) : base(_messageFormat.FormatString(backUp, backUpDirectory))
        {

        }
    }
}
