using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Backup
{
    public class DirectoryBackupManager : BackupManager<DirectoryBackup>
    {
        // Properties
        protected override string BackupSourceIdentifier => BackupSource.FullName;
        public DirectoryInfo BackupSource { get; }

        public DirectoryBackupManager(DirectoryInfo directoryToBackUp, DirectoryInfo directory) : base(directory)
        {
            directoryToBackUp.ValidateVariable(nameof(directoryToBackUp));
            BackupSource = directoryToBackUp;

            Backups.AddRange(LoadBackupsFromDirectory(SourceDirectory));
        }

        public override DirectoryBackup CreateBackup()
        {
            var subDirectory = SourceDirectory.CreateSubdirectory(Guid.NewGuid().ToString().ToValidPath());

            var backup = DirectoryBackup.Create(subDirectory, BackupSource);

            Backups.Add(backup);
            CheckRetention();

            return backup;
        }

        protected override IEnumerable<DirectoryBackup> LoadBackupsFromDirectory(DirectoryInfo directory)
        {
            directory.ValidateVariable(nameof(directory));

            return SourceDirectory.GetDirectories().ForceSelect(x => new DirectoryBackup(x, BackupSource)).Where(x => x.HasValue() && x.Succesful).ToList();
        }
    }
}
