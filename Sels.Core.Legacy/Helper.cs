﻿using System;
using System.Collections.Generic;
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
using SystemConsole = System.Console;
using SystemRandom = System.Random;
using SystemProcess = System.Diagnostics.Process;
using Sels.Core.Extensions.Reflection;
using static Sels.Core.Delegates.Async;
using Newtonsoft.Json.Linq;
using Sels.Core.Process;
using Sels.Core.Extensions.Fluent;
using Sels.Core.Extensions.Linq;
using Sels.Core.Scope;
using Sels.Core.Models;
using Sels.Core.Scope.Actions;
using System.Linq.Expressions;
using Sels.Core.Extensions.IO;
using Sels.Core.Extensions.Threading;
using Sels.Core.Extensions.DateTimes;
using Sels.Core.Extensions.Text;
using System.Runtime.InteropServices;

namespace Sels.Core
{
    /// <summary>
    /// Static class with generic helper methods
    /// </summary>
    public static class Helper
    {
        #region Enums
        /// <summary>
        /// Helper methods for working with enums.
        /// </summary>
        public static class Enums
        {
            /// <summary>
            /// Returns all values for enumeration <typeparamref name="TEnum"/>.
            /// </summary>
            /// <typeparam name="TEnum">Type of the enum to return the values from</typeparam>
            /// <returns>All values for enumeration <typeparamref name="TEnum"/></returns>
            public static TEnum[] GetAll<TEnum>() where TEnum : Enum
            {
                return (TEnum[])Enum.GetValues(typeof(TEnum));
            }
        }
        #endregion

        #region FileSystem
        /// <summary>
        /// Contains static helper methods for working with the filesystem.
        /// </summary>
        public static class FileSystem
        {
            /// <summary>
            /// Checks if <paramref name="path"/> is a valid directory path.
            /// </summary>
            /// <param name="path">The path string to validate</param>
            /// <returns>If <paramref name="path"/> is a valid directory</returns>
            public static bool IsValidDirectoryPath(string path) 
            {
                if(path.HasValue())
                {
                    return !path.ToCharArray().Any(x => Path.GetInvalidPathChars().Contains(x));
                }

                return false;
            }

            /// <summary>
            /// Checks if <paramref name="fileName"/> is a vlid filename path.
            /// </summary>
            /// <param name="fileName">The filename string to check</param>
            /// <returns>If <paramref name="fileName"/> is a valid filename</returns>
            public static bool IsValidFileName(string fileName)
            {
                if (fileName.HasValue())
                {
                    return !fileName.ToCharArray().Any(x => Path.GetInvalidFileNameChars().Contains(x));
                }

                return false;
            }
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Contains helper methods for working with application configuration.
        /// </summary>
        public static class Configuration
        {
            /// <summary>
            /// Builds a path string from <paramref name="key"/> and optionally <paramref name="sections"/> representing the configuration location. Used for tracing.
            /// </summary>
            /// <param name="key">The config key or section name</param>
            /// <param name="sections">Optional parent sections for <paramref name="key"/></param>
            /// <returns>A path string representing <paramref name="key"/> and <paramref name="sections"/></returns>
            public static string BuildPathString(string key, params string[] sections)
            {
                key.ValidateArgumentNotNullOrWhitespace(nameof(key));

                return sections.HasValue() ? $"{sections.JoinString(":")}:{key}" : key;
            }

            /// <summary>
            /// Builds a new instance of <see cref="IConfiguration"/> using the default AppSettings.json file that resides besides the application exe.
            /// </summary>
            /// <returns>The IConfiguration created from the default configuration file</returns>
            public static IConfiguration BuildDefaultConfigurationFile()
            {
                return new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile(Constants.Configuration.DefaultAppSettingsFile).Build();
            }

            /// <summary>
            /// Creates configuration from all json files in <paramref name="directory"/> and optionally <paramref name="additionalDirectories"/>.
            /// </summary>
            /// <param name="directory">The directory to scan for json files</param>
            /// <param name="filter">Optional filter for defining which json files to include in the configuration</param>
            /// <param name="reloadOnChange">Whether the configuration needs to be reloaded when the file changes</param>
            /// <param name="additionalDirectories">Additional directories to scan</param>
            /// <returns>Configuration created from all json files in <paramref name="directory"/></returns>
            public static IConfiguration BuildConfigurationFromDirectory(DirectoryInfo directory, Predicate<FileInfo> filter = null, bool reloadOnChange = true, params DirectoryInfo[] additionalDirectories)
            {
                directory.ValidateArgument(nameof(directory));
                filter = filter != null ? filter : new Predicate<FileInfo>(x => true);

                var builder = new ConfigurationBuilder();
                Helper.Collection.Enumerate(directory, additionalDirectories).Where(x => x != null && x.Exists)
                                 .SelectMany(x => x.GetFiles())
                                 .Where(x => x.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase) && filter(x))
                                 .Execute(x => builder.AddJsonFile(x.FullName, false, reloadOnChange));

                return builder.Build();
            }

            /// <summary>
            /// Creates configuration from all json files in <see cref="AppContext.BaseDirectory"/> and optionally <paramref name="additionalDirectories"/>.
            /// </summary>
            /// <param name="filter">Optional filter for defining which json files to include in the configuration</param>
            /// <param name="reloadOnChange">Whether the configuration needs to be reloaded when the file changes</param>
            /// <param name="additionalDirectories">Additional directories to scan</param>
            /// <returns>Configuration created from all json files in <see cref="AppContext.BaseDirectory"/></returns>
            public static IConfiguration BuildConfigurationFromDirectory(Predicate<FileInfo> filter = null, bool reloadOnChange = true, params DirectoryInfo[] additionalDirectories)
            {
                return BuildConfigurationFromDirectory(new DirectoryInfo(AppContext.BaseDirectory), filter, reloadOnChange, additionalDirectories);
            }

            /// <summary>
            /// Creates configuration from all json files in <paramref name="directories"/>.
            /// </summary>
            /// <param name="directories">The directories to scan for json files</param>
            /// <param name="filter">Optional filter for defining which json files to include in the configuration</param>
            /// <param name="reloadOnChange">Whether the configuration needs to be reloaded when the file changes</param>
            /// <returns>Configuration created from all json files in <paramref name="directories"/></returns>
            public static IConfiguration BuildConfigurationFromDirectories(IEnumerable<DirectoryInfo> directories, Predicate<FileInfo> filter = null, bool reloadOnChange = true)
            {
                directories.ValidateArgument(nameof(directories));
                filter = filter != null ? filter : new Predicate<FileInfo>(x => true);

                var builder = new ConfigurationBuilder();
                directories.Where(x => x != null && x.Exists)
                                 .SelectMany(x => x.GetFiles())
                                 .Where(x => x.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase) && filter(x))
                                 .Execute(x => builder.AddJsonFile(x.FullName, false, reloadOnChange));

                return builder.Build();
            }
        }
        #endregion

        #region App
        /// <summary>
        /// Contains helper methods for running applications.
        /// </summary>
        public static class App
        {
            private static CancellationTokenSource _applicationTokenSource = CreateApplicationTokenSource();
            /// <summary>
            /// Cancellation token that will be cancelled when the current application is requested to exit.
            /// </summary>
            public static CancellationToken ApplicationToken => _applicationTokenSource.Token;

            private static CancellationTokenSource CreateApplicationTokenSource()
            {
                var source = new CancellationTokenSource();
                OnExit((s, a) =>
                {
                    source.Cancel();
                });
                OnCancel((s, a) =>
                {
                    source.Cancel();
                    a.Cancel = true;
                });

                return source;
            }

            /// <summary>
            /// Registers the <paramref name="action"/> delegate that will be executed when the application closes.
            /// </summary>
            /// <param name="action">The delegate to execute when the application closes</param>
            public static void OnExit(Action action)
            {
                action.ValidateArgument(nameof(action));

                AppDomain.CurrentDomain.ProcessExit += (x, y) => action();
            }
            /// <summary>
            /// Registers the <paramref name="action"/> delegate that will be executed when the application closes.
            /// </summary>
            /// <param name="action">The delegate to execute when the application closes</param>
            public static void OnExit(Action<object, EventArgs> action)
            {
                action.ValidateArgument(nameof(action));

                AppDomain.CurrentDomain.ProcessExit += (x, y) => action(x, y);
            }

            /// <summary>
            /// Registers the <paramref name="action"/> delegate that will be executed when the application is requested to cancel.
            /// </summary>
            /// <param name="action">The delegate to execute when the application is requested to cancel</param>
            public static void OnCancel(Action<object, ConsoleCancelEventArgs> action)
            {
                action.ValidateArgument(nameof(action));

                SystemConsole.CancelKeyPress += (x, y) => action(x,y);
            }

            /// <summary>
            /// Registers the <paramref name="action"/> delegate that will be executed when the application is requested to cancel.
            /// </summary>
            /// <param name="action">The delegate to execute when the application is requested to cancel</param>
            public static void OnCancel(Action action)
            {
                action.ValidateArgument(nameof(action));

                OnCancel((x, y) => action());
            }

            /// <summary>
            /// Sets the current directory to the directory of the executing process. This is to fix the config files when publishing as a self-contained app.
            /// </summary>
            public static void SetCurrentDirectoryToProcess()
            {
                var baseDir = Path.GetDirectoryName(SystemProcess.GetCurrentProcess().MainModule.FileName);

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
            /// Returns the current os platform.
            /// </summary>
            /// <returns>The current os platform</returns>
            /// <exception cref="NotSupportedException"></exception>
            public static OSPlatform GetCurrentOsPlatform()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return OSPlatform.OSX;
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return OSPlatform.Linux;
                }
#if NET6_0_OR_GREATER
                if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                {
                    return OSPlatform.FreeBSD;
                }
#endif
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return OSPlatform.Windows;
                }

                throw new NotSupportedException($"Could not determine the current os platform");
            }
        }
        #endregion

        #region String
        /// <summary>
        /// Contains helper methods for working with strings.
        /// </summary>
        public static class Strings
        {
            /// <summary>
            /// Joins all strings returned from calling <paramref name="values"/> by calling <see cref="object.ToString"/>.
            /// </summary>
            /// <param name="values">The objects the join</param>
            /// <returns>The joined string</returns>
            public static string JoinStrings(params object[] values)
            {
                values.ValidateArgument(nameof(values));

                return values.JoinString();
            }
            /// <summary>
            /// Joins all strings returned from calling <paramref name="values"/> by calling <see cref="object.ToString"/> using the <see cref="object.ToString"/> value from <paramref name="joinValue"/>.
            /// </summary>
            /// <param name="joinValue">The value to join <paramref name="values"/> with</param>
            /// <param name="values">The objects to join</param>
            /// <returns>The joined string</returns>
            public static string JoinStrings(object joinValue, params string[] values)
            {
                joinValue.ValidateArgument(nameof(joinValue));
                values.ValidateArgument(nameof(values));

                return values.JoinString(joinValue);
            }
            /// <summary>
            /// Joins all strings returned from calling <paramref name="values"/> by calling <see cref="object.ToString"/> using the environment new line character.
            /// </summary>
            /// <param name="values">The objects to join</param>
            /// <returns>The joined string</returns>
            public static string JoinStringsNewLine(params string[] values)
            {
                values.ValidateArgument(nameof(values));

                return values.JoinStringNewLine();
            }
            /// <summary>
            /// Joins all strings returned from calling <paramref name="values"/> by calling <see cref="object.ToString"/> using the tab character.
            /// </summary>
            /// <param name="values">The objects to join</param>
            /// <returns>The joined string</returns>
            public static string JoinStringsTab(params string[] values)
            {
                values.ValidateArgument(nameof(values));

                return values.JoinStringTab();
            }
        }
        #endregion

        #region List
        /// <summary>
        /// Contains helper methods for working with lists.
        /// </summary>
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

        #region Collection
        /// <summary>
        /// Contains static helper methods for working with collections.
        /// </summary>
        public static class Collection
        {
            /// <summary>
            /// Creates an enumerator returning all elements in <paramref name="enumerators"/>. Nulls in <paramref name="enumerators"/> are ignored.
            /// </summary>
            /// <typeparam name="T">Type of element to return</typeparam>
            /// <param name="enumerators">List of enumerators to returns the elements from</param>
            /// <returns>An enumerator returning all elements in <paramref name="enumerators"/></returns>
            public static IEnumerable<T> EnumerateAll<T>(params IEnumerable<T>[] enumerators)
            {
                if (enumerators.HasValue())
                {
                    foreach(var enumerator in enumerators.Where(x => x != null))
                    {
                        foreach(var element in enumerator)
                        {
                            yield return element;
                        }
                    }
                }
            }

            /// <summary>
            /// Create an enumerator returning <paramref name="value"/> and optionally all elements in <paramref name="values"/>.
            /// </summary>
            /// <typeparam name="T">Type of element to return</typeparam>
            /// <param name="value">The first value to return</param>
            /// <param name="values">Optional additional values to return</param>
            /// <returns>An enumerator returning <paramref name="value"/> and optionally all elements in <paramref name="values"/></returns>
            public static IEnumerable<T> Enumerate<T>(T value, params T[] values)
            {
                yield return value;
                if (values.HasValue())
                {
                    foreach (var otherValue in values)
                    {
                        yield return otherValue;
                    }
                }
            }

            /// <summary>
            /// Create an enumerator returning <paramref name="value"/> and optionally all elements in <paramref name="values"/>.
            /// </summary>
            /// <typeparam name="T">Type of element to return</typeparam>
            /// <param name="value">The first value to return</param>
            /// <param name="values">Optional additional values to return</param>
            /// <returns>An enumerator returning <paramref name="value"/> and optionally all elements in <paramref name="values"/></returns>
            public static IEnumerable<T> Enumerate<T>(T value, IEnumerable<T> values = null)
            {
                yield return value;
                if (values.HasValue())
                {
                    foreach (var otherValue in values)
                    {
                        yield return otherValue;
                    }
                }
            }
            /// <summary>
            /// Create an enumerator returning all elements in <paramref name="values"/> and optionally all elements in <paramref name="additionalValues"/>.
            /// </summary>
            /// <typeparam name="T">Type of element to return</typeparam>
            /// <param name="values">The first elements to return</param>
            /// <param name="additionalValues">Optional additional values to return</param>
            /// <returns>An enumerator returning all elements in <paramref name="values"/> and optionally all elements in <paramref name="values"/></returns>
            public static IEnumerable<T> Enumerate<T>(IEnumerable<T> values, params T[] additionalValues)
            {
                if (values.HasValue())
                {
                    foreach(var value in values)
                    {
                        yield return value;
                    }
                }
                if (additionalValues.HasValue())
                {
                    foreach (var otherValue in additionalValues)
                    {
                        yield return otherValue;
                    }
                }
            }
        }
        #endregion

        #region Program
        /// <summary>
        /// Contains helper methods for working with 
        /// </summary>
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
            /// <param name="loggers">Optional loggers for tracing</param>
            /// <returns>The program exit code</returns>
            public static int Run(string programFileName, string arguments, out string output, out string error, CancellationToken token = default, IEnumerable<ILogger> loggers = null, int killWaitTime = 10000)
            {
                using (var logger = loggers.CreateTimedLogger(LogLevel.Debug, $"Executing program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)}", x => $"Executed program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)} in {x.PrintTotalMs()}"))
                {
                    programFileName.ValidateArgumentNotNullOrWhitespace(nameof(programFileName));

                    var process = new SystemProcess()
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

                                if (!process.WaitForExit(killWaitTime))
                                {
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

            /// <summary>
            /// Runs program <paramref name="programFileName"/> with argument <paramref name="arguments"/>.
            /// </summary>
            /// <param name="programFileName">The filename of the program to run</param>
            /// <param name="arguments">Optional command line arguments for the process</param>
            /// <param name="outputHandler">Optional delegate that gets triggered for each line writter to the standard output</param>
            /// <param name="errorOutputHandler">Optional delegate that gets triggered for each line writter to the error output</param>
            /// <param name="token">Optional token for cancelling the executing of the process. Will try to make the process exit gracefully</param>
            /// <param name="logger">Optional logger for debugging the execution of the process</param>
            /// <param name="killWaitTime">How long in milliseconds to wait for the process to exit when <paramref name="token"/> receives it's cancellation request</param>
            /// <returns>The exit code of the process</returns>
            public static async Task<int> RunAsync(string programFileName, string arguments, Action<string> outputHandler, Action<string> errorOutputHandler, CancellationToken token = default, ILogger logger = null, int killWaitTime = 10000)
            {
                using (var timedLogger = logger.CreateTimedLogger(LogLevel.Debug, $"Executing program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)}", x => $"Executed program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)} in {x.PrintTotalMs()}"))
                {
                    programFileName.ValidateArgumentNotNullOrWhitespace(nameof(programFileName));
                    killWaitTime.ValidateArgumentLargerOrEqual(nameof(killWaitTime), 1);

                    var process = new SystemProcess()
                    {
                        StartInfo = new ProcessStartInfo(programFileName, arguments)
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.EnableRaisingEvents = true;

                    try
                    {
                        if (outputHandler != null) process.OutputDataReceived += (s, a) => outputHandler(a.Data);
                        if (errorOutputHandler != null) process.ErrorDataReceived += (s, a) => errorOutputHandler(a.Data);
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Started process {process.Id} ({time.PrintTotalMs()})"));

                        // Wait for process to finish
                        while (!process.HasExited)
                        {
                            timedLogger.Log((time, log) => log.LogMessage(LogLevel.Trace, $"Waiting for process {process.Id} to exit ({time.PrintTotalMs()})"));
                            await Task.Delay(100).ConfigureAwait(false);

                            // Wait for process to exit
                            if (token.IsCancellationRequested)
                            {
                                timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killing process {process.Id} ({time.PrintTotalMs()})"));
                                var killTask = Task.Run(process.Kill);
                                timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Sent kill signal to process {process.Id} and will now wait for maximum {killWaitTime}ms for it to exit ({time.PrintTotalMs()})"));

                                if (!process.WaitForExit(killWaitTime))
                                {
                                    timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killed process {process.Id} could not gracefully exit within {killWaitTime}ms ({time.PrintTotalMs()})"));
                                    throw new TaskCanceledException($"Process {process.Id} could not properly stop in {killWaitTime}ms");
                                }
                                else
                                {
                                    timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killed process {process.Id} exited gracefully ({time.PrintTotalMs()})"));
                                    await killTask.ConfigureAwait(false);
                                    break;
                                }
                            }
                        }

                        timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Process {process.Id} has exited with code {process.ExitCode} ({time.PrintTotalMs()})"));

                        return process.ExitCode;
                    }
                    finally
                    {
                        timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Disposing process {process.Id} ({time.PrintTotalMs()})"));
                        process.Dispose();
                    }
                }

            }
            /// <summary>
            /// Runs program <paramref name="programFileName"/> with argument <paramref name="arguments"/>.
            /// </summary>
            /// <param name="programFileName">The filename of the program to run</param>
            /// <param name="arguments">Optional command line arguments for the process</param>
            /// <param name="outputHandler">Optional delegate that gets triggered for each line writter to the standard/error output</param>
            /// <param name="token">Optional token for cancelling the executing of the process. Will try to make the process exit gracefully</param>
            /// <param name="logger">Optional logger for debugging the execution of the process</param>
            /// <param name="killWaitTime">How long in milliseconds to wait for the process to exit when <paramref name="token"/> receives it's cancellation request</param>
            /// <returns>The exit code of the process</returns>
            public static Task<int> RunAsync(string programFileName, string arguments, Action<string> outputHandler, CancellationToken token = default, ILogger logger = null, int killWaitTime = 10000)
            {
                return RunAsync(programFileName, arguments, x => outputHandler?.Invoke(x), x => outputHandler?.Invoke(x), token, logger, killWaitTime);
            }

            /// <summary>
            /// Returns a builder for running a program.
            /// </summary>
            /// <param name="programFileName">Filename of the process to run</param>
            /// <returns>Builder for configuring and executing a program</returns>
            public static IProcessRunner Run(string programFileName) => new ProcessRunner(programFileName.ValidateArgumentNotNullOrWhitespace(nameof(programFileName)));
        }
        #endregion

        #region Console
        /// <summary>
        /// Contains helper methods for working with the console.
        /// </summary>
        public static class Console
        {
            private const ConsoleColor _defaultForegroundColor = ConsoleColor.Gray;
            private const ConsoleColor _defaultBackgroundColor = ConsoleColor.Black;
            private static object _threadlock = new object();

            /// <summary>
            /// Helper method for running code in a console. Catches and logs exceptions and asks for a key press to exit.
            /// </summary>
            /// <param name="entryMethod">The action to execute</param>
            public static void Run(Action entryMethod)
            {
                entryMethod.ValidateArgument(nameof(entryMethod));

                try
                {
                    entryMethod();
                }
                catch (Exception ex)
                {
                    SystemConsole.WriteLine($"Something went wrong while execuring console app: {Environment.NewLine + ex.ToString()}");
                }
                finally
                {
                    SystemConsole.WriteLine("Press any key to close");
                    SystemConsole.Read();
                }
            }

            /// <summary>
            /// Helper method for running code in a console. Catches and logs exceptions and asks for a key press to exit.
            /// </summary>
            ///  <param name="entryMethod">The action to execute</param>
            /// <param name="exitHandler">The code to run when closing the console</param>
            public static void Run(Action entryMethod, Action exitHandler)
            {
                entryMethod.ValidateArgument(nameof(entryMethod));
                exitHandler.ValidateArgument(nameof(exitHandler));

                App.OnExit(exitHandler);

                Run(entryMethod);
            }

            /// <summary>
            /// Helper method for running code in a console. Catches and logs exceptions and asks for a key press to exit.
            /// </summary>
            /// <param name="entryMethod">The action to execute</param>
            public static async Task RunAsync(AsyncAction entryMethod)
            {
                entryMethod.ValidateArgument(nameof(entryMethod));

                try
                {
                    await entryMethod().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SystemConsole.WriteLine($"Something went wrong while execuring console app: {Environment.NewLine + ex.ToString()}");
                }
                finally
                {
                    SystemConsole.WriteLine("Press any key to close");
                    SystemConsole.Read();
                }
            }

            /// <summary>
            /// Helper method for running code in a console. Catches and logs exceptions and asks for a key press to exit.
            /// </summary>
            ///  <param name="entryMethod">The action to execute</param>
            /// <param name="exitHandler">The code to run when closing the console</param>
            public static Task RunAsync(AsyncAction entryMethod, Action exitHandler)
            {
                entryMethod.ValidateArgument(nameof(entryMethod));
                exitHandler.ValidateArgument(nameof(exitHandler));

                App.OnExit(exitHandler);

                return RunAsync(entryMethod);
            }

            /// <summary>
            /// Writes <paramref name="message"/> to the console using <paramref name="foregroundColor"/> as the text color and <paramref name="backgroundColor"/> as the background color.
            /// </summary>
            /// <param name="foregroundColor">The foreground color to use</param>
            /// <param name="backgroundColor">The background color to use</param>
            /// <param name="message">The message to write to the console</param>
            public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
            {
                lock (_threadlock)
                {
                    SystemConsole.ForegroundColor = foregroundColor;
                    SystemConsole.BackgroundColor = backgroundColor;

                    SystemConsole.WriteLine(message);

                    ResetColors();
                }
            }
            /// <summary>
            /// Writes <paramref name="message"/> to the console using <paramref name="foregroundColor"/> as the text color.
            /// </summary>
            /// <param name="foregroundColor">The foreground color to use</param>
            /// <param name="message">The message to write to the console</param>
            public static void WriteLine(ConsoleColor foregroundColor, string message)
            {
                WriteLine(foregroundColor, _defaultBackgroundColor, message);
            }

            /// <summary>
            /// Writes <paramref name="message"/> to the console using <paramref name="foregroundColor"/> as the text color and <paramref name="backgroundColor"/> as the background color.
            /// </summary>
            /// <param name="foregroundColor">The foreground color to use</param>
            /// <param name="backgroundColor">The background color to use</param>
            /// <param name="message">The message to write to the console</param>
            public static void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
            {
                lock (_threadlock)
                {
                    SystemConsole.ForegroundColor = foregroundColor;
                    SystemConsole.BackgroundColor = backgroundColor;

                    SystemConsole.Write(message);

                    ResetColors();
                }
            }
            /// <summary>
            /// Writes <paramref name="message"/> to the console using <paramref name="foregroundColor"/> as the text color.
            /// </summary>
            /// <param name="foregroundColor">The foreground color to use</param>
            /// <param name="message">The message to write to the console</param>
            public static void Write(ConsoleColor foregroundColor, string message)
            {
                Write(foregroundColor, _defaultBackgroundColor, message);
            }

            private static void ResetColors()
            {
                SystemConsole.ForegroundColor = _defaultForegroundColor;
                SystemConsole.BackgroundColor = _defaultBackgroundColor;
            }
        }
        #endregion

        #region Random
        /// <summary>
        /// Contains helper methods for generating random values.
        /// </summary>
        public static class Random
        {
            private static int _threadSeed = 0;
            private static object _threadLock = new object();
            private static ThreadLocal<SystemRandom> ThreadInstance => new ThreadLocal<SystemRandom>(() =>
            {
                lock(_threadLock)
                {
                    return new SystemRandom(++_threadSeed+ Environment.TickCount);
                }
            });
            /// <summary>
            /// The local random instance for the calling thread.
            /// </summary>
            public static SystemRandom Instance => ThreadInstance.Value;

            /// <summary>
            /// Returns a random int larger or equal to <paramref name="min"/> and smaller or equal to <paramref name="max"/>.
            /// </summary>
            /// <param name="min">The lowest possible value to generate</param>
            /// <param name="max">The highest possible value to generate</param>
            /// <returns>A random int in range of <paramref name="min"/> and <paramref name="max"/></returns>
            public static int GetRandomInt(int min, int max)
            {
                max.ValidateArgumentLarger(nameof(max), min);

                return Instance.Next(min, max+1);
            }
            /// <summary>
            /// Returns a random double larger or equal to <paramref name="min"/> and smaller or equal to <paramref name="max"/>.
            /// </summary>
            /// <param name="min">The lowest possible value to generate</param>
            /// <param name="max">The highest possible value to generate</param>
            /// <returns>A random double in range of <paramref name="min"/> and <paramref name="max"/></returns>
            public static double GetRandomDouble(double min, double max)
            {
                max.ValidateArgumentLarger(nameof(max), min);

                return Instance.NextDouble() * (max - min) + min;
            }
        }
        #endregion

        #region Expression
        /// <summary>
        /// Contains static helper methods for working with expressions.
        /// </summary>
        public static class Expression
        {
            /// <summary>
            /// Returns the method info extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <typeparam name="T">Type to select the method from</typeparam>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodInfo in <paramref name="methodExpression"/></returns>
            public static MethodInfo GetMethod<T>(Expression<Func<T, object>> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                if (!methodExpression.TryExtractMethod(out var method)) throw new InvalidOperationException($"{nameof(methodExpression)} does not point to a method");
                return method;
            }
            /// <summary>
            /// Returns the method info extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <typeparam name="T">Type to select the method from</typeparam>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodInfo in <paramref name="methodExpression"/></returns>
            public static MethodInfo GetMethod<T>(Expression<Action<T>> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                if (!methodExpression.TryExtractMethod(out var method)) throw new InvalidOperationException($"{nameof(methodExpression)} does not point to a method");
                return method;
            }
            /// <summary>
            /// Returns the method info extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodInfo in <paramref name="methodExpression"/></returns>
            public static MethodInfo GetMethod(Expression<Func<object>> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                if (!methodExpression.TryExtractMethod(out var method)) throw new InvalidOperationException($"{nameof(methodExpression)} does not point to a method");
                return method;
            }
            /// <summary>
            /// Returns the method info extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodInfo in <paramref name="methodExpression"/></returns>
            public static MethodInfo GetMethod(Expression<Action> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                if (!methodExpression.TryExtractMethod(out var method)) throw new InvalidOperationException($"{nameof(methodExpression)} does not point to a method");
                return method;
            }

            /// <summary>
            /// Returns the method call expression extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <typeparam name="T">Type to select the method from</typeparam>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodCallExpression in <paramref name="methodExpression"/></returns>
            public static MethodCallExpression GetMethodCallExpression<T>(Expression<Func<T, object>> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                return methodExpression.ExtractMethodCall();
            }
            /// <summary>
            /// Returns the method call expression extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <typeparam name="T">Type to select the method from</typeparam>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodCallExpression in <paramref name="methodExpression"/></returns>
            public static MethodCallExpression GetMethodCallExpression<T>(Expression<Action<T>> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                return methodExpression.ExtractMethodCall();
            }
            /// <summary>
            /// Returns the method call expression extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodCallExpression in <paramref name="methodExpression"/></returns>
            public static MethodCallExpression GetMethodCallExpression(Expression<Func<object>> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                return methodExpression.ExtractMethodCall();
            }
            /// <summary>
            /// Returns the method call expression extracted from <paramref name="methodExpression"/>
            /// </summary>
            /// <param name="methodExpression">The expression that selects the method</param>
            /// <returns>The MethodCallExpression in <paramref name="methodExpression"/></returns>
            public static MethodCallExpression GetMethodCallExpression(Expression<Action> methodExpression)
            {
                methodExpression.ValidateArgument(nameof(methodExpression));

                return methodExpression.ExtractMethodCall();
            }

            /// <summary>
            /// Returns the method info extracted from <paramref name="propertyExpression"/>
            /// </summary>
            /// <typeparam name="T">Type to select the method from</typeparam>
            /// <param name="propertyExpression">The expression that selects the method</param>
            /// <returns>The MethodInfo in <paramref name="propertyExpression"/></returns>
            public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> propertyExpression)
            {
                propertyExpression.ValidateArgument(nameof(propertyExpression));

                if (!propertyExpression.TryExtractProperty(out var property)) throw new InvalidOperationException($"{nameof(propertyExpression)} does not point to a property");
                return property;
            }

            /// <summary>
            /// Static helper methods for working with expression resolving around properties.
            /// </summary>
            public static class Property
            {
                /// <summary>
                /// Validates that <paramref name="nestedProperties"/> are selected from root object of type <paramref name="expectedRoot"/>.
                /// </summary>
                /// <param name="expectedRoot">The expected reflected type of the first property</param>
                /// <param name="nestedProperties">The nested properties to check</param>
                /// <exception cref="InvalidDataException"></exception>
                public static void ValidateNestedProperties(Type expectedRoot, IEnumerable<PropertyInfo> nestedProperties)
                {
                    expectedRoot.ValidateArgument(nameof(expectedRoot));
                    nestedProperties.ValidateArgumentNotNullOrEmpty(nameof(nestedProperties));

                    PropertyInfo currentProperty = null;

                    foreach(var property in nestedProperties)
                    {
                        if(currentProperty == null)
                        {                            
                            if (!expectedRoot.IsAssignableTo(property.ReflectedType)) throw new InvalidDataException($"Expected property <{property.Name}> to be reflected from <{expectedRoot}> but was <{property.ReflectedType}>");
                        }
                        else
                        {
                            if (currentProperty.ReflectedType.GetProperty(property.Name) == null) throw new InvalidDataException($"Property <{property.Name}> does not exist on type <{currentProperty.ReflectedType}>");
                        }
                        currentProperty = property;
                    }
                }
            }
        }
        #endregion

        #region Json
        /// <summary>
        /// Contains static extension methods for working with json.
        /// </summary>
        public static class Json
        {
            #region Modifying 
            /// <summary>
            /// Sets a value in a json using a json path string.
            /// </summary>
            /// <param name="json">The json to update</param>
            /// <param name="path">The path of the value to set</param>
            /// <param name="value">Object containing the value to set</param>
            public static void Set(JObject json, string path, object value)
            {
                path.ValidateArgument(nameof(path));
                value.ValidateArgument(nameof(value));

                JToken current = json;
                var tokens = path.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

                for (int i = 0; i < tokens.Length; i++)
                {
                    var token = tokens[i];
                    var isLast = i >= tokens.Length - 1;
                    var target = current.SelectToken(token);

                    // Try and set value
                    if (isLast)
                    {
                        if(target == null)
                        {
                            ((JObject)current).Add(token, JToken.FromObject(value));
                        }
                        else
                        {
                            target.Replace(JToken.FromObject(value));
                        }                       
                    }
                    // Traverse json
                    else
                    {                     
                        if (target == null)
                        {
                            ((JObject)current).Add(token, new JObject());
                            target = current.SelectToken(token);
                        }

                        current = target;
                    }
                }
            }
            #endregion

            #region File
            /// <summary>
            /// Reads a json object from a file.
            /// </summary>
            /// <param name="file">The file to read the json object from</param>
            /// <returns>The json object read from <paramref name="file"/></returns>
            public static async Task<JObject> ReadAsync(FileInfo file)
            {
                file.ValidateArgumentExists(nameof(file));

                return JObject.Parse(await file.ReadAsync().ConfigureAwait(false));
            }
            #endregion
        }
        #endregion

        #region Async
        /// <summary>
        /// Contains static helper methods when coding asynchronously.
        /// </summary>
        public static class Async
        {
            /// <summary>
            /// Sleeps for <paramref name="waitTime"/> milliseconds asynchronously. When <paramref name="token"/> is cancelled no <see cref="TaskCanceledException"/> will be thrown.
            /// </summary>
            /// <param name="waitTime">How many milliseconds to sleep for</param>
            /// <param name="token">Optional token to cancel the sleeping</param>
            public static async Task Sleep(int waitTime, CancellationToken token = default)
            {
                if (waitTime <= 0) return;

                try
                {
                    await Task.Delay(waitTime, token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { }
            }

            /// <summary>
            /// Sleeps for <paramref name="waitTime"/> asynchronously. When <paramref name="token"/> is cancelled no <see cref="TaskCanceledException"/> will be thrown.
            /// </summary>
            /// <param name="waitTime">How long to sleep</param>
            /// <param name="token">Optional token to cancel the sleeping</param>
            public static async Task Sleep(TimeSpan waitTime, CancellationToken token = default)
            {
                if (waitTime.TotalMilliseconds <= 0) return;

                try
                {
                    await Task.Delay(waitTime, token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { }
            }

            /// <summary>
            /// Sleeps until <paramref name="sleepTime"/> asynchronously. When <paramref name="token"/> is cancelled no <see cref="TaskCanceledException"/> will be thrown.
            /// </summary>
            /// <param name="sleepTime">The date after which the returned task will complete</param>
            /// <param name="token">Optional token to cancel the sleeping</param>
            /// <returns>Task that will complete when either the time goes past <paramref name="sleepTime"/> or when <paramref name="token"/> gets cancelled</returns>
            public static async Task SleepUntil(DateTime sleepTime, CancellationToken token = default)
            {
                while(DateTime.Now < sleepTime)
                {
                    if (token.IsCancellationRequested) return;
                    var waitTime = DateTime.Now - sleepTime;

                    await Sleep(waitTime, token).ConfigureAwait(false);
                }
            }

            /// <summary>
            /// Waits for the completion of <paramref name="task"/> for a maximum of <paramref name="maxWaitTime"/>.
            /// </summary>
            /// <param name="task">The task to wait on</param>
            /// <param name="maxWaitTime">How long to wait for <paramref name="task"/> to complete</param>
            /// <param name="token">Optional token to cancel the request</param>
            /// <returns>Task that gets completed when either <paramref name="task"/> finishes within <paramref name="maxWaitTime"/>, when execution exceeds <paramref name="maxWaitTime"/> or when <paramref name="token"/> gets cancelled</returns>
            /// <exception cref="TaskCanceledException"></exception>
            /// <exception cref="TimeoutException"></exception>
            public static async Task WaitOn(Task task, TimeSpan maxWaitTime, CancellationToken token = default)
            {
                _ = task.ValidateArgument(nameof(task));

                // No need to wait here
                if(maxWaitTime == TimeSpan.Zero)
                {
                    await task.ConfigureAwait(false);
                    return;
                }

                var taskSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cancellationSource = new CancellationTokenSource();

                // Use caller token to cancel our token. This should make the task throw TaskCanceledException instead of the timeout
                using(token.Register(() =>
                {
                    cancellationSource.Cancel();
                    taskSource.SetCanceled();
                }))
                {
                    if (token.IsCancellationRequested) throw new TaskCanceledException($"Wait on task <{task.Id}> was cancelled");
                    // Run continuation to complete callback task
                    var completionTask = task.ContinueWith(x =>
                    {
                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetFrom(x); 
                        }
                    }, cancellationSource.Token);

                    // Check if task is completed before starting the timeout task
                    if (task.IsCompleted)
                    {
                        await task.ConfigureAwait(false);
                        return;
                    }

                    // Start task to cancel our callback task
                    var timeoutTask = Task.Run(async () =>
                    {
                        await Sleep(maxWaitTime, cancellationSource.Token).ConfigureAwait(false);
                        if (cancellationSource.Token.IsCancellationRequested) return;

                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetException(new TimeoutException($"Task <{task.Id}> did not complete within <{maxWaitTime}>")); 
                        }
                    }, cancellationSource.Token);


                    // Wait for callback
                    await taskSource.Task.ConfigureAwait(false);
                    return;
                }
            }

            /// <summary>
            /// Waits for the completion of <paramref name="task"/> for a maximum of <paramref name="maxWaitTime"/>.
            /// </summary>
            /// <param name="task">The task to wait on</param>
            /// <param name="maxWaitTime">How long to wait for <paramref name="task"/> to complete</param>
            /// <param name="token">Optional token to cancel the request</param>
            /// <returns>Task that gets completed when either <paramref name="task"/> finishes within <paramref name="maxWaitTime"/>, when execution exceeds <paramref name="maxWaitTime"/> or when <paramref name="token"/> gets cancelled</returns>
            /// <exception cref="TaskCanceledException"></exception>
            /// <exception cref="TimeoutException"></exception>
            public static async Task<T> WaitOn<T>(Task<T> task, TimeSpan maxWaitTime, CancellationToken token = default)
            {
                _ = task.ValidateArgument(nameof(task));

                // No need to wait here
                if (maxWaitTime == TimeSpan.Zero)
                {
                    return await task.ConfigureAwait(false);
                }

                var taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cancellationSource = new CancellationTokenSource();

                // Use caller token to cancel our token. This should make the task throw TaskCancelledException instead of the timeout
                using (token.Register(() => {
                    cancellationSource.Cancel();
                    taskSource.SetCanceled();
                }))
                {
                    if (token.IsCancellationRequested) throw new TaskCanceledException($"Wait on task <{task.Id}> was cancelled");
                    // Run continuation to complete callback task
                    var completionTask = task.ContinueWith(x =>
                    {
                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetFrom(x); 
                        }
                    }, cancellationSource.Token);

                    // Check if task is completed before starting the timeout task
                    if (task.IsCompleted)
                    {
                        return await task.ConfigureAwait(false);
                    }

                    // Start task to cancel our callback task
                    var timeoutTask = Task.Run(async () =>
                    {
                        await Sleep(maxWaitTime, cancellationSource.Token).ConfigureAwait(false);
                        if (cancellationSource.Token.IsCancellationRequested) return;

                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetException(new TimeoutException($"Task <{task.Id}> did not complete within <{maxWaitTime}>")); 
                        }
                    }, cancellationSource.Token);


                    // Wait for callback
                    return await taskSource.Task.ConfigureAwait(false);
                }
            }

            /// <summary>
            /// Waits for the completion of <paramref name="task"/> or until <paramref name="token"/> gets cancelled.
            /// </summary>
            /// <param name="task">The task to wait on</param>
            /// <param name="token">Optional token to cancel the request</param>
            /// <returns>Task that gets completed when either <paramref name="task"/> finishes or when <paramref name="token"/> gets cancelled</returns>
            /// <exception cref="TaskCanceledException"></exception>
            /// <exception cref="TimeoutException"></exception>
            public static async Task WaitOn(Task task, CancellationToken token = default)
            {
                _ = task.ValidateArgument(nameof(task));

                var taskSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cancellationSource = new CancellationTokenSource();

                // Use caller token to cancel our token.
                using (token.Register(() =>
                {
                    cancellationSource.Cancel();
                    taskSource.SetCanceled();
                }))
                {
                    if (token.IsCancellationRequested) throw new TaskCanceledException($"Wait on task <{task.Id}> was cancelled");
                    // Run continuation to complete callback task
                    var completionTask = task.ContinueWith(x =>
                    {
                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetFrom(x);
                        }
                    }, cancellationSource.Token);

                    // Check if task is completed before starting the timeout task
                    if (task.IsCompleted)
                    {
                        await task.ConfigureAwait(false);
                        return;
                    }

                    // Wait for callback
                    await taskSource.Task.ConfigureAwait(false);
                    return;
                }
            }

            /// <summary>
            /// Waits for the completion of <paramref name="task"/> or until <paramref name="token"/> gets cancelled.
            /// </summary>
            /// <param name="task">The task to wait on</param>
            /// <param name="token">Optional token to cancel the request</param>
            /// <returns>Task that gets completed when either <paramref name="task"/> finishes or when <paramref name="token"/> gets cancelled</returns>
            /// <exception cref="TaskCanceledException"></exception>
            /// <exception cref="TimeoutException"></exception>
            public static async Task<T> WaitOn<T>(Task<T> task, CancellationToken token = default)
            {
                _ = task.ValidateArgument(nameof(task));

                var taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cancellationSource = new CancellationTokenSource();

                // Use caller token to cancel our token. 
                using (token.Register(() => {
                    cancellationSource.Cancel();
                    taskSource.SetCanceled();
                }))
                {
                    if (token.IsCancellationRequested) throw new TaskCanceledException($"Wait on task <{task.Id}> was cancelled");
                    // Run continuation to complete callback task
                    var completionTask = task.ContinueWith(x =>
                    {
                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetFrom(x);
                        }
                    }, cancellationSource.Token);

                    // Check if task is completed before starting the timeout task
                    if (task.IsCompleted)
                    {
                        return await task.ConfigureAwait(false);
                    }

                    // Wait for callback
                    return await taskSource.Task.ConfigureAwait(false);
                }
            }

            /// <summary>
            /// Returns a task that will only complete when<paramref name="token"/> is cancelled.
            /// </summary>
            /// <param name="token">The token to wait on</param>
            /// <returns>Task that will only complete when<paramref name="token"/> is cancelled</returns>
            public static async Task WaitUntilCancellation(CancellationToken token)
            {
                if (!token.CanBeCanceled) throw new InvalidOperationException($"CancellationToken can't be cancelled");

                if (token.IsCancellationRequested) return;
                var taskSource = new TaskCompletionSource<bool>();
                using(token.Register(() => taskSource.SetResult(true)))
                {
                    if (token.IsCancellationRequested) return;
                    await taskSource.Task.ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Contains static helper methods when coding with tasks synchronously.
        /// </summary>
        public static class Sync
        {
            /// <summary>
            /// Waits for the completion of <paramref name="task"/> for a maximum of <paramref name="maxWaitTime"/>.
            /// </summary>
            /// <param name="task">The task to wait on</param>
            /// <param name="maxWaitTime">How long to wait for <paramref name="task"/> to complete</param>
            /// <param name="token">Optional token to cancel the request</param>
            /// <returns>Task that gets completed when either <paramref name="task"/> finishes within <paramref name="maxWaitTime"/>, when execution exceeds <paramref name="maxWaitTime"/> or when <paramref name="token"/> gets cancelled</returns>
            /// <exception cref="TaskCanceledException"></exception>
            /// <exception cref="TimeoutException"></exception>
            public static void WaitOn(Task task, TimeSpan maxWaitTime, CancellationToken token = default)
            {
                const double maxSleepTimeMs = 250;
                _ = task.ValidateArgument(nameof(task));

                // No need to wait here
                if (maxWaitTime == TimeSpan.Zero)
                {
                    task.GetAwaiter().GetResult();
                    return;
                }

                var taskSource = new TaskCompletionSource<object>();
                var cancellationSource = new CancellationTokenSource();

                // Use caller token to cancel our token. This should make the task throw TaskCancelledException instead of the timeout
                using (token.Register(() => { cancellationSource.Cancel(); taskSource.SetCanceled(); }))
                {
                    if (token.IsCancellationRequested) throw new TaskCanceledException($"Wait on task <{task.Id}> was cancelled");

                    // Run continuation to complete callback task
                    var completionTask = task.ContinueWith(x =>
                    {
                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetFrom(x);
                        }
                    }, cancellationSource.Token);

                    // Check if task is completed before starting the timeout task
                    if (task.IsCompleted)
                    {
                        task.GetAwaiter().GetResult();
                        return;
                    }

                    // Start task to cancel our callback task
                    var timeoutTask = Task.Run(async () =>
                    {
                        await Helper.Async.Sleep(maxWaitTime, cancellationSource.Token).ConfigureAwait(false);
                        if (cancellationSource.Token.IsCancellationRequested) return;

                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetException(new TimeoutException($"Task <{task.Id}> did not complete within <{maxWaitTime}>"));
                        }
                    }, cancellationSource.Token);


                    // Wait for callback
                    var sleepTime = TimeSpan.FromTicks(maxWaitTime.Ticks / 4);
                    if (sleepTime.TotalMilliseconds > maxSleepTimeMs) sleepTime = TimeSpan.FromMilliseconds(maxSleepTimeMs);
                    while (!taskSource.Task.IsCompleted)
                    {
                        Thread.Sleep(sleepTime);
                    }

                    taskSource.Task.GetAwaiter().GetResult();
                    return;
                }
            }

            /// <summary>
            /// Waits for the completion of <paramref name="task"/> for a maximum of <paramref name="maxWaitTime"/>.
            /// </summary>
            /// <param name="task">The task to wait on</param>
            /// <param name="maxWaitTime">How long to wait for <paramref name="task"/> to complete</param>
            /// <param name="token">Optional token to cancel the request</param>
            /// <returns>Task that gets completed when either <paramref name="task"/> finishes within <paramref name="maxWaitTime"/>, when execution exceeds <paramref name="maxWaitTime"/> or when <paramref name="token"/> gets cancelled</returns>
            /// <exception cref="TaskCanceledException"></exception>
            /// <exception cref="TimeoutException"></exception>
            public static T WaitOn<T>(Task<T> task, TimeSpan maxWaitTime, CancellationToken token = default)
            {
                const double maxSleepTimeMs = 250;
                _ = task.ValidateArgument(nameof(task));

                // No need to wait here
                if (maxWaitTime == TimeSpan.Zero)
                {
                    return task.GetAwaiter().GetResult();
                }

                var taskSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                var cancellationSource = new CancellationTokenSource();

                // Use caller token to cancel our token. This should make the task throw TaskCancelledException instead of the timeout
                using (token.Register(() => { cancellationSource.Cancel(); taskSource.SetCanceled(); }))
                {
                    if (token.IsCancellationRequested) throw new TaskCanceledException($"Wait on task <{task.Id}> was cancelled");
                    // Run continuation to complete callback task
                    var completionTask = task.ContinueWith(x =>
                    {
                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetFrom(x);
                        }
                    }, cancellationSource.Token);

                    // Check if task is completed before starting the timeout task
                    if (task.IsCompleted)
                    {
                        return task.GetAwaiter().GetResult();
                    }

                    // Start task to cancel our callback task
                    var timeoutTask = Task.Run(async () =>
                    {
                        await Helper.Async.Sleep(maxWaitTime, cancellationSource.Token).ConfigureAwait(false);
                        if (cancellationSource.Token.IsCancellationRequested) return;

                        lock (taskSource)
                        {
                            if (!taskSource.Task.IsCompleted) taskSource.SetException(new TimeoutException($"Task <{task.Id}> did not complete within <{maxWaitTime}>"));
                        }
                    }, cancellationSource.Token);


                    // Wait for callback
                    var sleepTime = TimeSpan.FromTicks(maxWaitTime.Ticks / 4);
                    if (sleepTime.TotalMilliseconds > maxSleepTimeMs) sleepTime = TimeSpan.FromMilliseconds(maxSleepTimeMs);
                    while (!taskSource.Task.IsCompleted)
                    {
                        Thread.Sleep(sleepTime);
                    }

                    return taskSource.Task.GetAwaiter().GetResult();
                }
            }
        }
        #endregion

        #region Lock
        /// <summary>
        /// Contains static helper methods for working with thread locks.
        /// </summary>
        public static class Lock
        {
            /// <summary>
            /// Tries to get a lock on <paramref name="lockObject"/> to execute <paramref name="action"/>.
            /// </summary>
            /// <param name="lockObject">The object to lock</param>
            /// <param name="action">The delegate to execute when <paramref name="lockObject"/> could be locked.</param>
            /// <returns>True if a lock could be placed on <paramref name="lockObject"/>, otherwise false</returns>
            public static bool TryLockAndExecute(object lockObject, Action action)
            {
                lockObject.ValidateArgument(nameof(lockObject));
                action.ValidateArgument(nameof(action));

                if (Monitor.TryEnter(lockObject))
                {
                    try
                    {
                        action();
                        return true;
                    }
                    finally
                    {
                        Monitor.Exit(lockObject);
                    }
                }
                return false;
            }
        }
        #endregion

        #region Time
        /// <summary>
        /// Contains helper methods related to dates/time.
        /// </summary>
        public static class Time
        {
            /// <summary>
            /// Captures the duration of the action executed within the scope. 
            /// Disposing the returned object will stop the stopwatch and output the duration to <paramref name="duration"/>.
            /// </summary>
            /// <param name="duration">Object where the duration will be outputted to</param>
            /// <returns>Disposable to define the scope to capture the duraction for</returns>
            public static IDisposable CaptureDuration(out Ref<TimeSpan> duration)
            {
                duration = new Ref<TimeSpan>();

                return new DurationAction(duration);
            }

            /// <summary>
            /// Captures the duration of the action executed within the scope. 
            /// Disposing the returned object will stop the stopwatch and call <paramref name="elapsedHandler"/> with the duration.
            /// </summary>
            /// <param name="elapsedHandler">The delegate to handle the elapsed time with</param>
            /// <returns>Disposable to define the scope to capture the duraction for</returns>
            public static IDisposable CaptureDuration(Action<TimeSpan> elapsedHandler)
            {
                elapsedHandler.ValidateArgument(nameof(elapsedHandler));
                var stopwatch = new Stopwatch();
                return new ScopedAction(() => stopwatch.Start(), () =>
                {
                    stopwatch.Stop();
                    elapsedHandler(stopwatch.Elapsed);
                });
            }
        }
        #endregion
    }
}
