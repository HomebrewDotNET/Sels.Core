using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>
    /// Contains extension methods for working with <see cref="FileInfo"/>.
    /// </summary>
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Checks if <paramref name="file"/> is locked and can't be written to.
        /// </summary>
        /// <param name="file">The files to check</param>
        /// <returns>True if <paramref name="file"/> can't be written to, otherwise false</returns>
        public static bool IsLocked(this FileInfo file)
        {
            file.ValidateArgumentExists(nameof(file));
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

        /// <summary>
        /// Gets the extension from <paramref name="file"/> without the dot.
        /// </summary>
        /// <param name="file">The file to get the extension from</param>
        /// <returns>The extension of <paramref name="file"/></returns>
        public static string GetExtensionName(this FileInfo file)
        {
            file.ValidateArgumentExists(nameof(file));
            return file.Extension.TrimStart('.');
        }

        /// <summary>
        /// Gets the file name without extension from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to get the name from</param>
        /// <returns>The file name without extension from <paramref name="file"/></returns>
        public static string GetNameWithoutExtension(this FileInfo file)
        {
            file.ValidateArgumentExists(nameof(file));

            return Path.GetFileNameWithoutExtension(file.Name);
        }
        /// <summary>
        /// Opens <paramref name="file"/> using the windows file explorer.
        /// </summary>
        /// <param name="file">The file to open</param>
        public static void OpenWithWindowsExplorer(this FileInfo file)
        {
            if (file != null && file.Exists)
            {
                Process.Start("explorer.exe", file.FullName);
            }

        }

        #region Io Operations
        /// <summary>
        /// Reads the file content from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">File to read content from</param>
        /// <returns>File content of <paramref name="file"/></returns>
        public static string Read(this FileInfo file)
        {
            file.ValidateArgumentExists(nameof(file));

            return File.ReadAllText(file.FullName);
        }
        /// <summary>
        /// Reads the file content from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">File to read content from</param>
        /// <returns>File content of <paramref name="file"/></returns>
        public static async Task<string> ReadAsync(this FileInfo file)
        {
            file.ValidateArgumentExists(nameof(file));

            using(var stream = file.OpenRead())
            {
                using(var reader = new StreamReader(stream, true))
                {
                    return await reader.ReadToEndAsync();
                }
            }
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
            content.ValidateArgument(nameof(content));

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
        /// <summary>
        /// Writes <paramref name="content"/> to <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <param name="content">The file content to write</param>
        public static void Write(this FileInfo file, string content)
        {
            file.ValidateArgument(nameof(file));
            content.ValidateArgument(nameof(content));

            File.WriteAllText(file.FullName, content);
        }
        /// <summary>
        /// Writes <paramref name="content"/> to <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <param name="content">The file content to write</param>
        /// <param name="fileShare">The file share options while reading the file</param>
        public static async Task WriteAsync(this FileInfo file, string content, FileShare fileShare = FileShare.None)
        {
            file.ValidateArgument(nameof(file));
            content.ValidateArgument(nameof(content));

            using(var fileStream = file.Open(file.Exists ? FileMode.Truncate : FileMode.CreateNew, FileAccess.Write, fileShare))
            {
                using(var writer = new StreamWriter(fileStream))
                {
                    await writer.WriteAsync(content);
                }
            }
        }
        /// <summary>
        /// Replaces the file content of <paramref name="file"/> with an empty string.
        /// </summary>
        /// <param name="file">The file to clear</param>
        public static void Clear(this FileInfo file)
        {
            file.ValidateArgument(nameof(file));

            File.WriteAllText(file.FullName, string.Empty);
        }
        #endregion

        #region Copying 
        /// <summary>
        /// Copies <paramref name="file"/> to directory <paramref name="destinationDirectory"/>.
        /// </summary>
        /// <param name="file">File to copy</param>
        /// <param name="destinationDirectory">Directory to copy file to</param>
        /// <param name="overwrite">If the target file can be overwritten if it already exists</param>
        /// <returns>The copied file</returns>
        public static FileInfo CopyTo(this FileInfo file, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            file.ValidateArgumentExists(nameof(file));
            destinationDirectory.ValidateArgument(nameof(destinationDirectory));

            var newFileName = Path.Combine(destinationDirectory.FullName, file.Name);

            return file.CopyTo(newFileName, overwrite);
        }
        /// <summary>
        /// Copies <paramref name="file"/> to directory <paramref name="destinationDirectory"/> with <paramref name="fileName"/> as the new file name.
        /// </summary>
        /// <param name="file">File to copy</param>
        /// <param name="destinationDirectory">Directory to copy file to</param>
        /// <param name="fileName">Filename of copied file</param>
        /// <param name="overwrite">If the target file can be overwritten if it already exists</param>
        /// <returns>The copied file</returns>
        public static FileInfo CopyTo(this FileInfo file, DirectoryInfo destinationDirectory, string fileName, bool overwrite = false)
        {
            file.ValidateArgumentExists(nameof(file));
            destinationDirectory.ValidateArgument(nameof(destinationDirectory));
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
        /// <returns>The moved file</returns>
        public static FileInfo MoveTo(this FileInfo file, DirectoryInfo destinationDirectory)
        {
            file.ValidateArgumentExists(nameof(file));
            destinationDirectory.ValidateArgument(nameof(destinationDirectory));

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
        /// <returns>The moved file</returns>
        public static FileInfo MoveTo(this FileInfo file, DirectoryInfo destinationDirectory, string fileName)
        {
            file.ValidateArgumentExists(nameof(file));
            destinationDirectory.ValidateArgument(nameof(destinationDirectory));
            fileName.ValidateArgumentNotNullOrWhitespace(nameof(fileName));

            var newFileName = Path.Combine(destinationDirectory.FullName, fileName);

            file.MoveTo(newFileName);

            return new FileInfo(newFileName);
        }
        #endregion
    }
}
