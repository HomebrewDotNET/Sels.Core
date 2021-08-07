using Sels.Core.Components.FileSizes.Byte;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.IO
{
    public static class FileInfoExtensions
    {
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
            file.ValidateArgument(parameterName);

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
            if (file != null && file.Exists)
            {
                return file.Extension.TrimStart('.');
            }

            return string.Empty;
        }

        public static string GetNameWithoutExtension(this FileInfo file)
        {
            if (file != null && file.Exists)
            {
                return Path.GetFileNameWithoutExtension(file.Name);
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

        public static TSize GetFileSize<TSize>(this FileInfo file) where TSize : FileSize
        {
            file.ValidateArgumentExists(nameof(file));

            var sizeType = typeof(TSize).Is<FileSize>() ? typeof(SingleByte) : typeof(TSize);

            return FileSize.CreateFromBytes(file.Length, sizeType).As<TSize>();
        }

        #region Backup
        private const string DefaultBackupFormat = "{0}.Backup";

        public static FileInfo Backup(this FileInfo file, string backupFormat = DefaultBackupFormat)
        {
            file.ValidateArgumentExists(nameof(file));

            var newName = backupFormat.FormatString(file.GetNameWithoutExtension());
            var fullNewName = Path.Combine(file.Directory.FullName, newName + file.Extension);

            var counter = 0;
            while (File.Exists(fullNewName))
            {
                var numberedNewName = $"{newName}.{counter}";

                fullNewName = Path.Combine(file.Directory.FullName, numberedNewName + file.Extension);

                counter++;
            }

            return file.CopyTo(fullNewName);
        }
        #endregion

        #region Io Operations
        /// <summary>
        /// Reads the file content from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">File to read content from</param>
        /// <returns>File content of <paramref name="file"/></returns>
        public static string Read(this FileInfo file)
        {
            return File.ReadAllText(file.FullName);
        }

        /// <summary>
        /// Creates a new file using the filename of <paramref name="file"/> and writes <paramref name="content"/> into the file.
        /// </summary>
        /// <param name="file">File to write</param>
        /// <param name="content">Content to write into the file</param>
        /// <param name="overwrite">If we can overwrite the file if it already exists</param>
        /// <returns>Boolean indicating if the file was created</returns>
        public static bool Create(this FileInfo file, string content, bool overwrite = false)
        {
            file.ValidateArgument(nameof(file));

            if (file.Exists && overwrite)
            {
                file.Delete();
            }

            if (!file.Exists)
            {
                File.WriteAllText(file.FullName, content);
                file.Refresh();
                return file.Exists;
            }

            return false;
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
        /// <summary>
        /// Copies <paramref name="file"/> to directory <paramref name="destinationDirectory"/>.
        /// </summary>
        /// <param name="file">File to copy</param>
        /// <param name="destinationDirectory">Directory to copy file to</param>
        /// <returns>FileInfo of copied file</returns>
        public static FileInfo CopyTo(this FileInfo file, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            file.CreateIfNotExistAndValidate(nameof(file));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));

            var newFileName = Path.Combine(destinationDirectory.FullName, file.Name);

            return file.CopyTo(newFileName, overwrite);
        }

        /// <summary>
        /// Copies <paramref name="file"/> to directory <paramref name="destinationDirectory"/> with <paramref name="fileName"/> as the new file name.
        /// </summary>
        /// <param name="file">File to copy</param>
        /// <param name="destinationDirectory">Directory to copy file to</param>
        /// <param name="fileName">Filename of copied file</param>
        /// <returns>FileInfo of copied file</returns>
        public static FileInfo CopyTo(this FileInfo file, DirectoryInfo destinationDirectory, string fileName, bool overwrite = false)
        {
            file.ValidateArgument(nameof(file));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));
            fileName.ValidateArgumentNotNullOrWhitespace(nameof(fileName));

            var newFileName = Path.Combine(destinationDirectory.FullName, fileName);

            return file.CopyTo(newFileName, overwrite);
        }
        #endregion

        #region Moving 
        /// <summary>
        /// Moves <paramref name="file"/> to directory <paramref name="destinationDirectory"/>.
        /// </summary>
        /// <param name="file">File to move</param>
        /// <param name="destinationDirectory">Directory to move file to</param>
        /// <returns>FileInfo of moved file</returns>
        public static FileInfo MoveTo(this FileInfo file, DirectoryInfo destinationDirectory)
        {
            file.CreateIfNotExistAndValidate(nameof(file));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));

            var newFileName = Path.Combine(destinationDirectory.FullName, file.Name);

            file.MoveTo(newFileName);

            return new FileInfo(newFileName);
        }

        /// <summary>
        /// Moves <paramref name="file"/> to directory <paramref name="destinationDirectory"/> with <paramref name="fileName"/> as the new file name.
        /// </summary>
        /// <param name="file">File to move</param>
        /// <param name="destinationDirectory">Directory to move file to</param>
        /// <param name="fileName">Filename of moved file</param>
        /// <returns>FileInfo of moved file</returns>
        public static FileInfo MoveTo(this FileInfo file, DirectoryInfo destinationDirectory, string fileName)
        {
            file.ValidateArgument(nameof(file));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));
            fileName.ValidateArgumentNotNullOrWhitespace(nameof(fileName));

            var newFileName = Path.Combine(destinationDirectory.FullName, fileName);

            file.MoveTo(newFileName);

            return new FileInfo(newFileName);
        }
        #endregion
    }
}
