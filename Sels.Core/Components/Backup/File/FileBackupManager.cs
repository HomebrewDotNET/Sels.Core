using Sels.Core.Extensions;
using Sels.Core.Extensions.Io;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Backup
{
    public class FileBackupManager : BackupManager<FileBackup>
    {
        // Constants

        // Properties
        protected override string BackupSourceIdentifier => BackupSource.FullName;

        public FileInfo BackupSource { get; }

        public FileBackupManager(FileInfo fileToBackUp, DirectoryInfo directory) : base(directory)
        {
            fileToBackUp.ValidateVariable(nameof(fileToBackUp));
            BackupSource = fileToBackUp;

            Backups.AddRange(LoadBackupsFromDirectory(SourceDirectory));
        }

        public FileBackupManager(FileInfo fileToBackUp, DirectoryInfo directory, BackupRetentionMode retentionMode, int retentionValue) : base(directory, retentionMode, retentionValue)
        {
            fileToBackUp.ValidateVariable(nameof(fileToBackUp));
            BackupSource = fileToBackUp;

            Backups.AddRange(LoadBackupsFromDirectory(SourceDirectory));
            CheckRetention();
        }



        public override FileBackup CreateBackup()
        {
            var subDirectory = SourceDirectory.CreateSubdirectory(Guid.NewGuid().ToString().ToValidPath());

            var backup = FileBackup.Create(subDirectory, BackupSource);

            Backups.Add(backup);
            CheckRetention();

            return backup;
        }

        protected override IEnumerable<FileBackup> LoadBackupsFromDirectory(DirectoryInfo directory)
        {
            directory.ValidateVariable(nameof(directory));

            return SourceDirectory.GetDirectories().ForceSelect(x => new FileBackup(x, BackupSource)).Where(x => x.HasValue() && x.Succesful).ToList();
        }
    }
}
