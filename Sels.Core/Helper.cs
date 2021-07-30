using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sels.Core.Extensions;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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
                action.ValidateArgument(nameof(action));

                AppDomain.CurrentDomain.ProcessExit += (x, y) => action();
            }

            public static void RegisterApplicationClosingAction(Action<object, EventArgs> action)
            {
                action.ValidateArgument(nameof(action));

                AppDomain.CurrentDomain.ProcessExit += (x, y) => action(x, y);
            }

            /// <summary>
            /// Sets the current directory to the directory of the executing process. This is to fix the config files when publishing as a self-contained app.
            /// </summary>
            public static void SetCurrentDirectoryToProcess()
            {
                var baseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                // Used for published configs
                Directory.SetCurrentDirectory(baseDir);
            }

            /// <summary>
            /// Sets the current directory to the directory of the executing process. This is to fix the config files when publishing as a self-contained app.
            /// </summary>
            public static void SetCurrentDirectoryToExecutingAssembly()
            {
                var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Used for published configs
                Directory.SetCurrentDirectory(baseDir);
            }

            /// <summary>
            /// Builds a new instance of <see cref="IConfiguration"/> using the default AppSettings.json file that resides besides the application exe.
            /// </summary>
            /// <returns></returns>
            public static IConfiguration BuildDefaultConfigurationFile()
            {
                var currentDirectory = Directory.GetCurrentDirectory();

                return new ConfigurationBuilder().SetBasePath(currentDirectory).AddJsonFile("appsettings.json").Build();
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
            public static int Run(string processFileName, string arguments, out string output, out string error, CancellationToken token = default)
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

                try
                {
                    process.Start();

                    // Wait for process to finish
                    while (!process.HasExited)
                    {
                        Thread.Sleep(250);

                        // Kill process
                        if (token.IsCancellationRequested)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }                     
                    }

                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();

                    return process.ExitCode;
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        #endregion
    }
}
