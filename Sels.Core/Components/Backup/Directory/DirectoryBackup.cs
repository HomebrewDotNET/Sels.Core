using Sels.Core.Components.Backup.Exceptions;
using Sels.Core.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Io;
using Sels.Core.Extensions.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DirectoryIo = System.IO.Directory;

namespace Sels.Core.Components.Backup
{
    public class DirectoryBackup : Backup
    {
        // Properties
        public DirectoryInfo BackedupDirectory { get; set; }

        public override bool Succesful => BackedupDirectory.HasValue() && WasBackupSuccesful();

        public override DateTime BackupDate => BackedupDirectory.CreationTime;

        public DirectoryBackup(DirectoryInfo sourceDirectory, DirectoryInfo sourceDirectoryBackup) : base(sourceDirectory)
        {
            sourceDirectoryBackup.ValidateVariable(nameof(sourceDirectoryBackup));

            var backupDirectoryName = Path.Combine(sourceDirectory.FullName, sourceDirectoryBackup.Name);

            if (DirectoryIo.Exists(backupDirectoryName))
            {
                BackedupDirectory = new DirectoryInfo(backupDirectoryName);
            }
            else
            {
                throw new BackupNotSuccesfulException(sourceDirectoryBackup.FullName);
            }
        }

        public static DirectoryBackup Create(DirectoryInfo backupLocation, DirectoryInfo sourceDirectory)
        {
            backupLocation.CreateIfNotExistAndValidate(nameof(backupLocation));
            sourceDirectory.ValidateVariable(nameof(sourceDirectory));

            try
            {
                var backedupDirectory = sourceDirectory.CopyTo(backupLocation);

                var backUp = new DirectoryBackup(backupLocation, backedupDirectory);
                backUp.BackupSuccesful();

                return backUp;
            }
            catch (BackupNotSuccesfulException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BackupNotSuccesfulException(sourceDirectory.FullName, ex);
            }

        }

        public override void RestoreBackup(DirectoryInfo directoryToRestoreTo)
        {
            directoryToRestoreTo.ValidateVariable(nameof(directoryToRestoreTo));

            BackedupDirectory.CopyTo(directoryToRestoreTo, true);
        }

        public override void Delete()
        {
            BackedupDirectory.Delete(true);
            base.Delete();
        }
    }
}
