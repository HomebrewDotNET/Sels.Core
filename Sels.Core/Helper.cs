using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sels.Core.Extensions;
using System.IO;
using System.Diagnostics;

namespace Sels.Core
{
    public static class Helper
    {
        #region Enums
        public static class Enums
        {
            public static TEnum[] GetList<TEnum>() where TEnum : Enum
            {
                return (TEnum[])Enum.GetValues(typeof(TEnum));
            }
        }
        #endregion

        #region FileSystem
        public static class FileSystem
        {
            public static bool IsValidDirectoryPath(string path) 
            {
                if(path.HasValue() && ContainsValidDrive(path))
                {
                    return !path.ToCharArray().Any(x => Path.GetInvalidPathChars().Contains(x));
                }

                return false;
            }

            public static bool IsValidFileName(string fileName)
            {
                if (fileName.HasValue() && ContainsValidDrive(fileName))
                {
                    return !fileName.ToCharArray().Any(x => Path.GetInvalidFileNameChars().Contains(x));
                }

                return false;
            }

            public static bool ContainsValidDrive(string path)
            {
                if (path.HasValue())
                {
                    var fullPath = Path.GetFullPath(path);

                    if (Path.IsPathRooted(fullPath))
                    {
                        var drive = Path.GetPathRoot(fullPath);
                        var logicalDrives = Environment.GetLogicalDrives();
                        return logicalDrives.Any(x => x == drive);
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region App
        public static class App
        {
            public static void RegisterApplicationClosingAction(Action action)
            {
                action.ValidateVariable(nameof(action));

                AppDomain.CurrentDomain.ProcessExit += (x, y) => action();
            }

            public static void RegisterApplicationClosingAction(Action<object, EventArgs> action)
            {
                action.ValidateVariable(nameof(action));

                AppDomain.CurrentDomain.ProcessExit += (x, y) => action(x, y);
            }
        }
        #endregion

        #region String
        public static class Strings
        {
            public static string JoinStrings(params object[] values)
            {
                values.ValidateVariable(nameof(values));

                return values.JoinString();
            }

            public static string JoinStrings(string joinValue, params string[] strings)
            {
                strings.ValidateVariable(nameof(strings));

                return strings.JoinString(joinValue);
            }

            public static string JoinStringsNewLine(params string[] strings)
            {
                strings.ValidateVariable(nameof(strings));

                return strings.JoinStringNewLine();
            }

            public static string JoinStringsTab(params string[] strings)
            {
                strings.ValidateVariable(nameof(strings));

                return strings.JoinStringTab();
            }
        }
        #endregion

        #region List
        public static class Lists
        {
            public static List<T> Combine<T>(params T[] values)
            {
                var list = new List<T>();

                if (values.HasValue())
                {
                    list.AddRange(values);
                }

                return list;
            }
        }
        #endregion

        #region Program
        public static class Program
        {
            /// <summary>
            /// Runs program at <paramref name="processFileName"/> with arguments <paramref name="arguments"/>.
            /// </summary>
            /// <param name="processFileName">Filename of program to run</param>
            /// <param name="arguments">Arguments for program</param>
            /// <param name="output">Standard output from program execution</param>
            /// <param name="error">Error output from program execution</param>
            /// <returns>Program exit code</returns>
            public static int Run(string processFileName, string arguments, out string output, out string error)
            {
                processFileName.ValidateArgument(nameof(processFileName));

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo(processFileName, arguments)
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                process.WaitForExit();

                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                return process.ExitCode;
            }
        }
        #endregion
    }
}
