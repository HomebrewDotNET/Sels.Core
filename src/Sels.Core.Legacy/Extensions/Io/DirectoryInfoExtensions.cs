using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System.IO;

namespace Sels.Core.Extensions.IO
{
    /// <summary>
    /// Contains extension methods for working with <see cref="DirectoryInfo"/>.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Creates <paramref name="directory"/> if it does not exist.
        /// </summary>
        /// <param name="directory">The directory to create</param>
        public static void CreateIfNotExist(this DirectoryInfo directory)
        {
            directory.ValidateArgument(nameof(directory));

            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        /// <summary>
        /// Checks if <paramref name="directory"/> does not contain any sub directories and files.
        /// </summary>
        /// <param name="directory">The directory to check</param>
        /// <returns>True if <paramref name="directory"/> is empty, otherwise false</returns>
        public static bool IsEmpty(this DirectoryInfo directory)
        {
            directory.ValidateArgument(nameof(directory));

            return directory.GetDirectories().Length == 0 && directory.GetFiles().Length == 0;
        }

        /// <summary>
        /// Deletes all files and sub directories in <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory to clear</param>
        public static void Clear(this DirectoryInfo directory)
        {
            directory.ValidateArgument(nameof(directory));

            directory.GetFiles().Execute(x => x.Delete());
            directory.GetDirectories().Execute(x => x.Clear());
        }
        /// <summary>
        /// Returns the relative sub directory using <paramref name="relativePath"/> starting fron <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory to get the sub directory from</param>
        /// <param name="relativePath">The relative path of the sub directory</param>
        /// <returns>The sub directory relative to <paramref name="directory"/></returns>
        public static DirectoryInfo GetRelative(this DirectoryInfo directory, string relativePath)
        {
            directory.ValidateArgument(nameof(directory));
            relativePath.ValidateArgumentNotNullOrWhitespace(nameof(relativePath));

            return new DirectoryInfo(Path.Combine(directory.FullName, relativePath));
        }

        #region Copying 
        /// <summary>
        /// Copies <paramref name="directory"/> to <paramref name="destinationDirectory"/>.
        /// </summary>
        /// <param name="directory">The directory to copy</param>
        /// <param name="destinationDirectory">The directory to copy to</param>
        /// <param name="overwrite">If destination files can be overwritten if they already exists</param>
        /// <returns>The copied directory</returns>
        public static DirectoryInfo CopyTo(this DirectoryInfo directory, string destinationDirectory, bool overwrite = false)
        {
            directory.ValidateArgumentExists(nameof(directory));
            destinationDirectory.ValidateArgumentNotNullOrWhitespace(nameof(destinationDirectory));

            var destination = new DirectoryInfo(destinationDirectory);
            return directory.CopyTo(destination, overwrite);
        }
        /// <summary>
        /// Copies <paramref name="directory"/> to <paramref name="destinationDirectory"/>.
        /// </summary>
        /// <param name="directory">The directory to copy</param>
        /// <param name="destinationDirectory">The directory to copy to</param>
        /// <param name="overwrite">If destination files can be overwritten if they already exists</param>
        /// <returns>The copied directory</returns>
        public static DirectoryInfo CopyTo(this DirectoryInfo directory, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            directory.ValidateArgumentExists(nameof(directory));
            destinationDirectory.ValidateArgument(nameof(destinationDirectory));
            destinationDirectory.CreateIfNotExist();

            var newDirectory = destinationDirectory.CreateSubdirectory(directory.Name);

            directory.GetDirectories().Execute(x => x.CopyTo(newDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(newDirectory, overwrite));

            return newDirectory;
        }
        /// <summary>
        /// Copies all sub directories and files from <paramref name="directory"/> to <paramref name="destinationDirectory"/>.
        /// </summary>
        /// <param name="directory">The directory to copy the content from</param>
        /// <param name="destinationDirectory">The directory to copy to</param>
        /// <param name="overwrite">If destination files can be overwritten if they already exists</param>
        /// <returns>The copied directory</returns>
        public static DirectoryInfo CopyContentTo(this DirectoryInfo directory, string destinationDirectory, bool overwrite = false)
        {
            directory.ValidateArgumentExists(nameof(directory));
            destinationDirectory.ValidateArgumentNotNullOrWhitespace(nameof(destinationDirectory));
            
            return directory.CopyContentTo(new DirectoryInfo(destinationDirectory), overwrite);
        }
        /// <summary>
        /// Copies all sub directories and files from <paramref name="directory"/> to <paramref name="destinationDirectory"/>.
        /// </summary>
        /// <param name="directory">The directory to copy the content from</param>
        /// <param name="destinationDirectory">The directory to copy to</param>
        /// <param name="overwrite">If destination files can be overwritten if they already exists</param>
        /// <returns>The copied directory</returns>
        public static DirectoryInfo CopyContentTo(this DirectoryInfo directory, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            directory.ValidateArgumentExists(nameof(directory));
            destinationDirectory.ValidateArgument(nameof(destinationDirectory));
            destinationDirectory.CreateIfNotExist();

            directory.GetDirectories().Execute(x => x.CopyTo(destinationDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(destinationDirectory, overwrite));

            return destinationDirectory;
        }
        #endregion
    }
}
