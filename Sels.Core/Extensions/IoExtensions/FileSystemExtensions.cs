using Sels.Core.Extensions.Execution;
using Sels.Core.Extensions.Execution.Linq;
using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static bool CreateIfNotExistAndValidate(this FileInfo file, string parameterName)
        {
            return file.CreateIfNotExistAndValidate(parameterName);
        }

        public static bool CreateIfNotExistAndValidate(this FileInfo file, object content, string parameterName)
        {
            file.ValidateVariable(parameterName);

            if (!file.Exists)
            {
                using (var stream = File.CreateText(file.FullName))
                {
                    if (content.HasValue())
                    {
                        stream.Write(content);
                    }

                    return true;
                }
            }

            return false;
        }

        public static string GetExtensionName(this FileInfo file)
        {
            if(file != null && file.Exists)
            {
                return file.Extension.TrimStart('.');
            }

            return string.Empty;
        }

        public static void OpenWithWindowsExplorer(this FileInfo file)
        {
            if (file != null && file.Exists)
            {
                Process.Start("explorer.exe", file.FullName);
            }

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
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));

            var newFileName = Path.Combine(destinationDirectory.FullName, file.Name);

            return file.CopyTo(newFileName, overwrite);
        }

        public static FileInfo CopyTo(this FileInfo file, DirectoryInfo destinationDirectory, string fileName, bool overwrite = false)
        {
            file.ValidateVariable(nameof(file));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));
            fileName.ValidateVariable(nameof(fileName));

            var newFileName = Path.Combine(destinationDirectory.FullName, fileName);

            return file.CopyTo(newFileName, overwrite);
        }
        #endregion
        #endregion

        #region Directory
        public static bool CreateIfNotExistAndValidate(this DirectoryInfo directory, string parameterName)
        {
            directory.ValidateVariable(nameof(directory));
            parameterName.ValidateVariable(nameof(parameterName));

            if (directory != null && !directory.Exists)
            {
                directory.Create();

                return true;
            }

            return false;
        }

        public static void CreateIfNotExist(this DirectoryInfo directory)
        {
            directory.ValidateVariable(nameof(directory));

            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        public static bool IsEmpty(this DirectoryInfo directory)
        {
            directory.ValidateVariable(nameof(directory));

            return directory.GetDirectories().Length == 0 && directory.GetFiles().Length == 0;
        }

        public static bool DeleteIfEmpty(this DirectoryInfo directory)
        {
            directory.ValidateVariable(nameof(directory));

            if (directory.IsEmpty())
            {
                directory.Delete();
                return true;
            }

            return false;
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
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));

            var newDirectory = destinationDirectory.CreateSubdirectory(directory.Name);

            directory.GetDirectories().Execute(x => x.CopyTo(newDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(newDirectory, overwrite));

            return newDirectory;
        }

        public static DirectoryInfo CopyContentTo(this DirectoryInfo directory, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            directory.ValidateVariable(nameof(directory));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));

            directory.GetDirectories().Execute(x => x.CopyTo(destinationDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(destinationDirectory, overwrite));

            return destinationDirectory;
        }
        #endregion
        #endregion
    }
}
