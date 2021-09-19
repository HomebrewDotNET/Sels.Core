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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Logging;

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
                values.ValidateArgument(nameof(values));

                return values.JoinString();
            }

            public static string JoinStrings(string joinValue, params string[] strings)
            {
                joinValue.ValidateArgument(nameof(joinValue));
                strings.ValidateArgument(nameof(strings));

                return strings.JoinString(joinValue);
            }

            public static string JoinStringsNewLine(params string[] strings)
            {
                strings.ValidateArgument(nameof(strings));

                return strings.JoinStringNewLine();
            }

            public static string JoinStringsTab(params string[] strings)
            {
                strings.ValidateArgument(nameof(strings));

                return strings.JoinStringTab();
            }
        }
        #endregion

        #region List
        public static class Lists
        {
            /// <summary>
            /// Creates a new list using <paramref name="values"/>.
            /// </summary>
            /// <typeparam name="T">Type of values to add to list</typeparam>
            /// <param name="values">Values to add to list</param>
            /// <returns>List whose elements are equal to <paramref name="values"/></returns>
            public static List<T> Combine<T>(params T[] values)
            {
                var list = new List<T>();

                if (values.HasValue())
                {
                    list.AddRange(values);
                }

                return list;
            }

            /// <summary>
            /// Merges all elements from the collections in <paramref name="values"/> into a single list.
            /// </summary>
            /// <typeparam name="T">Type of values to add to list</typeparam>
            /// <param name="values">Collection of collections whose values to add to the list</param>
            /// <returns>List whose elements are equal to the elements in the <paramref name="values"/> collections</returns>
            public static List<T> Merge<T>(params IEnumerable<T>[] values)
            {
                var list = new List<T>();

                if (values.HasValue())
                {
                    foreach(var value in values)
                    {
                        if (value.HasValue())
                        {
                            list.AddRange(value);
                        }
                    }
                }

                return list;
            }
        }
        #endregion

        #region Program
        public static class Program
        {
            /// <summary>
            /// Runs program at <paramref name="programFileName"/> with <paramref name="arguments"/>.
            /// </summary>
            /// <param name="programFileName">Filename of program to run</param>
            /// <param name="arguments">Arguments for program</param>
            /// <param name="output">Standard output from program execution</param>
            /// <param name="error">Error output from program execution</param>
            /// <param name="killWaitTime">How long to wait for the process to exit after killing it. This is only applicable when the cancellation token is used</param>
            /// <param name="token">CancellationToken for stopping the execution of the process</param>
            /// <returns>Program exit code</returns>
            public static int Run(string programFileName, string arguments, out string output, out string error, CancellationToken token = default, IEnumerable<ILogger> loggers = null, int killWaitTime = 10000)
            {
                using var logger = loggers.CreateTimedLogger(LogLevel.Debug, $"Executing program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)}", x => $"Executed program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)} in {x.PrintTotalMs()}");
                programFileName.ValidateArgument(nameof(programFileName));

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo(programFileName, arguments)
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
                    logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Started process {process.Id} ({time.PrintTotalMs()})"));

                    // Wait for process to finish
                    while (!process.HasExited)
                    {
                        logger.Log((time, log) => log.LogMessage(LogLevel.Trace, $"Waiting for process {process.Id} to exit ({time.PrintTotalMs()})"));
                        Thread.Sleep(250);

                        // Wait for process to exit
                        if (token.IsCancellationRequested)
                        {
                            logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killing process {process.Id} ({time.PrintTotalMs()})"));
                            var killTask = Task.Run(process.Kill);
                            logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Sent kill signal to process {process.Id} and will now wait for maximum {killWaitTime}ms for it to exit ({time.PrintTotalMs()})"));

                            if (!process.WaitForExit(killWaitTime)) {
                                logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killed process {process.Id} could not gracefully exit within {killWaitTime}ms ({time.PrintTotalMs()})"));
                                throw new TaskCanceledException($"Process {process.Id} could not properly stop in {killWaitTime}ms");
                            }
                            else
                            {
                                logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killed process {process.Id} exited gracefully ({time.PrintTotalMs()})"));
                                killTask.Wait();
                                break;
                            }
                        }
                    }

                    logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Process {process.Id} has exited. Collecting output ({time.PrintTotalMs()})"));

                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();

                    logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Process {process.Id} output collected and has exited with code {process.ExitCode} ({time.PrintTotalMs()})"));

                    return process.ExitCode;
                }
                finally
                {
                    logger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Disposing process {process.Id} ({time.PrintTotalMs()})"));
                    process.Dispose();
                }
            }
        }
        #endregion
    }
}
