using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Backup
{
    public interface IBackupable
    {
        void CreateBackup();

        IEnumerable<Backup> GetBackups();

        void RestoreLatestBackup();
        void RestoreEarliestBackup();
        void RestoreBackup(Backup backup);
    }
}
