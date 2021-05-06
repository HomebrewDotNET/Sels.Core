using Sels.Core.Extensions.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileIo = System.IO.File;
using Sels.Core.Components.Backup.Exceptions;
using Sels.Core.Extensions;

namespace Sels.Core.Components.Backup
{
    public class FileBackup : Backup
    {
        // Properties
        public FileInfo BackedupFile { get; }
        public override DateTime BackupDate => BackedupFile.CreationTime;
        public override bool Succesful => BackedupFile.HasValue() && WasBackupSuccesful();

        internal FileBackup(DirectoryInfo sourceDirectory, FileInfo sourceFile) : base(sourceDirectory)
        {
            sourceFile.ValidateVariable(nameof(sourceFile));

            var backUpFileName = Path.Combine(sourceDirectory.FullName, sourceFile.Name);

            if (FileIo.Exists(backUpFileName))
            {
                BackedupFile = new FileInfo(backUpFileName);
            }
            else
            {
                throw new BackupNotSuccesfulException(sourceFile.FullName);
            }
        }

        public override void RestoreBackup(DirectoryInfo directoryToRestoreTo)
        {
            directoryToRestoreTo.ValidateVariable(nameof(directoryToRestoreTo));

            BackedupFile.CopyTo(directoryToRestoreTo, true);
        }

        public static FileBackup Create(DirectoryInfo backupLocation, FileInfo sourceFile)
        {
            backupLocation.CreateIfNotExistAndValidate(nameof(backupLocation));
            sourceFile.ValidateVariable(nameof(sourceFile));

            try
            {
                var backedUpFile = sourceFile.CopyTo(backupLocation);

                var backUp = new FileBackup(backupLocation, backedUpFile);
                backUp.BackupSuccesful();

                return backUp;
            }
            catch(BackupNotSuccesfulException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new BackupNotSuccesfulException(sourceFile.FullName, ex);
            }
            
        }

        public override void Delete()
        {
            BackedupFile.Delete();
            base.Delete();
        }
    }
}
