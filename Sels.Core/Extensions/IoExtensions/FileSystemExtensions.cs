using Sels.Core.Extensions.Execution;
using Sels.Core.Extensions.Execution.Linq;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Extensions.Io.FileSystem
{
    public static class FileSystemExtensions
    {
        #region File
        public static bool IsLocked(this FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        #region Io Operations
        public static string Read(this FileInfo file)
        {
            return File.ReadAllText(file.FullName);
        }

        public static void Write(this FileInfo file, string content)
        {
            File.WriteAllText(file.FullName, content);
        }

        public static void Clear(this FileInfo file)
        {
            File.WriteAllText(file.FullName, string.Empty);
        }
        #endregion

        #region Copying 
        public static FileInfo CopyTo(this FileInfo file, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            file.ValidateVariable(nameof(file));
            destinationDirectory.EnsureExistsAndValidate(nameof(destinationDirectory));

            var newFileName = Path.Combine(destinationDirectory.FullName, file.Name);

            return file.CopyTo(newFileName, overwrite);
        }
        #endregion
        #endregion

        #region Directory
        public static void EnsureExistsAndValidate(this DirectoryInfo directory, string parameterName)
        {
            if(directory != null && !directory.Exists)
            {
                directory.Create();
            }

            directory.ValidateVariable(parameterName);
        }

        public static bool IsEmpty(this DirectoryInfo directory)
        {
            directory.ValidateVariable(nameof(directory));

            return directory.GetDirectories().Length == 0 && directory.GetFiles().Length == 0;
        }

        #region Copying 
        public static DirectoryInfo CopyTo(this DirectoryInfo directory, string destinationDirectory, bool overwrite = false)
        {
            var destination = new DirectoryInfo(destinationDirectory);
            return directory.CopyTo(destination, overwrite);
        }

        public static DirectoryInfo CopyTo(this DirectoryInfo directory, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            directory.ValidateVariable(nameof(directory));
            destinationDirectory.EnsureExistsAndValidate(nameof(destinationDirectory));

            var newDirectory = destinationDirectory.CreateSubdirectory(directory.Name);

            directory.GetDirectories().Execute(x => x.CopyTo(newDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(newDirectory, overwrite));

            return newDirectory;
        }

        public static DirectoryInfo CopyContentTo(this DirectoryInfo directory, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            directory.ValidateVariable(nameof(directory));
            destinationDirectory.EnsureExistsAndValidate(nameof(destinationDirectory));

            directory.GetDirectories().Execute(x => x.CopyTo(destinationDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(destinationDirectory, overwrite));

            return destinationDirectory;
        }
        #endregion
        #endregion
    }
}
