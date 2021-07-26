using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{
    public static class DirectoryInfoExtensions
    {
        public static bool CreateIfNotExistAndValidate(this DirectoryInfo directory, string parameterName)
        {
            directory.ValidateArgument(nameof(directory));
            parameterName.ValidateArgumentNotNullOrWhitespace(nameof(parameterName));

            if (directory != null && !directory.Exists)
            {
                directory.Create();

                return true;
            }

            return false;
        }

        public static void CreateIfNotExist(this DirectoryInfo directory)
        {
            directory.ValidateArgument(nameof(directory));

            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        public static bool IsEmpty(this DirectoryInfo directory)
        {
            directory.ValidateArgument(nameof(directory));

            return directory.GetDirectories().Length == 0 && directory.GetFiles().Length == 0;
        }

        public static bool DeleteIfEmpty(this DirectoryInfo directory)
        {
            directory.ValidateArgument(nameof(directory));

            if (directory.IsEmpty())
            {
                directory.Delete();
                return true;
            }

            return false;
        }

        public static void Clear(this DirectoryInfo directory)
        {
            directory.ValidateArgument(nameof(directory));

            directory.GetFiles().Execute(x => x.Delete());
            directory.GetDirectories().Execute(x => x.Clear());
        }

        #region Copying 
        public static DirectoryInfo CopyTo(this DirectoryInfo directory, string destinationDirectory, bool overwrite = false)
        {
            var destination = new DirectoryInfo(destinationDirectory);
            return directory.CopyTo(destination, overwrite);
        }

        public static DirectoryInfo CopyTo(this DirectoryInfo directory, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            directory.ValidateArgumentExists(nameof(directory));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));

            var newDirectory = destinationDirectory.CreateSubdirectory(directory.Name);

            directory.GetDirectories().Execute(x => x.CopyTo(newDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(newDirectory, overwrite));

            return newDirectory;
        }

        public static DirectoryInfo CopyContentTo(this DirectoryInfo directory, DirectoryInfo destinationDirectory, bool overwrite = false)
        {
            directory.ValidateArgumentExists(nameof(directory));
            destinationDirectory.CreateIfNotExistAndValidate(nameof(destinationDirectory));

            directory.GetDirectories().Execute(x => x.CopyTo(destinationDirectory, overwrite));
            directory.GetFiles().Execute(x => x.CopyTo(destinationDirectory, overwrite));

            return destinationDirectory;
        }
        #endregion
    }
}
