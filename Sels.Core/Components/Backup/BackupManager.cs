using Sels.Core.Extensions;
using Sels.Core.Extensions.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sels.Core.Components.Backup.Exceptions;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.Components.Backup
{
    public abstract class BackupManager<TBackupContainer> where TBackupContainer : Backup
    {
        // Properties
        public List<TBackupContainer> Backups { get; set; }

        public BackupRetentionMode RetentionMode { get; } 
        public int RetentionValue { get; }
        public DirectoryInfo SourceDirectory { get; }


        protected BackupManager(DirectoryInfo directory, BackupRetentionMode retentionMode, int retentionValue) : this(directory)
        {
            retentionValue.ValidateVariable(nameof(retentionValue));
            RetentionMode = retentionMode;
            RetentionValue = retentionValue;
        }
        protected BackupManager(DirectoryInfo directory)
        {
            directory.CreateIfNotExistAndValidate(nameof(directory));
            SourceDirectory = directory;         

            RetentionMode = BackupRetentionMode.Null;

            Backups = new List<TBackupContainer>();      
        }

        public void DeleteAll()
        {
            if (Backups.HasValue())
            {
                Backups.Execute(x => x.Delete());
            }
        }

        // Virtuals
        public virtual void CheckRetention()
        {
            if (RetentionMode == BackupRetentionMode.Null) return;

            if (Backups.HasValue())
            {
                bool backUpsDeleted = false;
                switch (RetentionMode)
                {
                    // Delete oldest backups if total count exceeds retention days
                    case BackupRetentionMode.Amount:
                        if(Backups.Count > RetentionValue)
                        {
                            var counter = 1;

                            foreach (var backUp in Backups.OrderByDescending(x => x.BackupDate))
                            {
                                if(counter > RetentionValue)
                                {
                                    backUp.Delete();
                                    backUpsDeleted = true;
                                }
                                counter++;
                            }
                        }
                        break;
                    // Delete backups past the retention days
                    case BackupRetentionMode.Days:
                        foreach(var backUp in Backups)
                        {
                            if(backUp.BackupDate.GetDayDifference() > RetentionValue)
                            {
                                backUp.Delete();
                                backUpsDeleted = true;
                            }
                        }
                        break;
                }

                if (backUpsDeleted)
                {
                    Backups = Backups.Where(x => x.Succesful).ToList();
                }
            }
        }

        public virtual TBackupContainer RestoreLatestBackup(DirectoryInfo directoryToRestoreTo) {
            if (Backups.HasValue())
            {
                var backUp = Backups.OrderByDescending(x => x.BackupDate).FirstOrDefault();

                backUp.RestoreBackup(directoryToRestoreTo);

                return backUp;
            }
            else
            {
                throw new NoBackupsPresentException(BackupSourceIdentifier, SourceDirectory.FullName);
            }
        }

        public virtual TBackupContainer RestoreEarliestBackup(DirectoryInfo directoryToRestoreTo) {
            if (Backups.HasValue())
            {
                var backUp = Backups.OrderBy(x => x.BackupDate).FirstOrDefault();

                backUp.RestoreBackup(directoryToRestoreTo);

                return backUp;
            }
            else
            {
                throw new NoBackupsPresentException(BackupSourceIdentifier, SourceDirectory.FullName);
            }
        }

        public bool TryRestoreLatestBackup(DirectoryInfo directoryToRestoreTo, out TBackupContainer backUp)
        {
            backUp = null;
            try
            {               
                backUp = RestoreLatestBackup(directoryToRestoreTo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryRestoreEarliestBackup(DirectoryInfo directoryToRestoreTo, out TBackupContainer backUp)
        {
            backUp = null;
            try
            {
                backUp = RestoreEarliestBackup(directoryToRestoreTo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Abstractions
        protected abstract string BackupSourceIdentifier { get; }

        protected abstract IEnumerable<TBackupContainer> LoadBackupsFromDirectory(DirectoryInfo directory);

        public abstract TBackupContainer CreateBackup();
    }
}
