using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using FileIo = System.IO.File;
using System.Text;
using System.IO;
using Sels.Core.Extensions.Io;
namespace Sels.Core.Components.Backup
{
    public abstract class Backup
    {
        // Constants
        private string _succesFileName = "BackUpSuccesful.Backup";

        // Properties
        public DirectoryInfo SourceDirectory { get; }

        protected Backup(DirectoryInfo sourceDirectory)
        {
            sourceDirectory.ValidateVariable(nameof(sourceDirectory));
            SourceDirectory = sourceDirectory;
        }

        public bool TryRestoreBackup(DirectoryInfo directoryToRestoreTo)
        {
            try
            {
                RestoreBackup(directoryToRestoreTo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected void BackupSuccesful()
        {
            FileIo.Create(Path.Combine(SourceDirectory.FullName, _succesFileName)).Dispose();
        }

        protected bool WasBackupSuccesful()
        {
            return FileIo.Exists(Path.Combine(SourceDirectory.FullName, _succesFileName));
        }

        // Abstraction
        public abstract bool Succesful { get; }
        public abstract DateTime BackupDate { get; }

        public abstract void RestoreBackup(DirectoryInfo directoryToRestoreTo);
        public virtual void Delete()
        {
            var okFile = new FileInfo(Path.Combine(SourceDirectory.FullName, _succesFileName));

            if (okFile.HasValue())
            {
                okFile.Delete();
            }

            if (SourceDirectory.IsEmpty())
            {
                SourceDirectory.Delete();
            }
        }

    }
}
