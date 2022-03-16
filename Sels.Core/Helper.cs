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
using SystemConsole = System.Console;
using SystemRandom = System.Random;
using Sels.Core.Extensions.Reflection;

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
                var currentDirectory = Directory.GetCurrentDirectory();

                return new ConfigurationBuilder().SetBasePath(currentDirectory).AddJsonFile(Constants.Configuration.DefaultAppSettingsFile).Build();
            }
        }
        #endregion

        #region App
        /// <summary>
        /// Contains helper methods for running applications.
        /// </summary>
        public static class App
        {
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
            /// Creates an enumerator returning all elements in <paramref name="enumerators"/>. Nulls are ignored.
            /// </summary>
            /// <typeparam name="T">Type of element to return</typeparam>
            /// <param name="enumerators">List of enumerators to returns the elements from</param>
            /// <returns>An enumerator returning all elements in <paramref name="enumerators"/></returns>
            public static IEnumerable<T> Enumerate<T>(params IEnumerable<T>[] enumerators)
            {
                if (enumerators.HasValue())
                {
                    foreach(var enumerator in enumerators.Where(x => x != null))
                    {
                        foreach(var element in enumerator.Where(x => x != null))
                        {
                            yield return element;
                        }
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
                using (var logger = loggers.CreateTimedLogger(LogLevel.Debug, $"Executing program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)}", x => $"Executed program {programFileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)} in {x.PrintTotalMs()}")) {
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
            private static SystemRandom _random = new SystemRandom();

            /// <summary>
            /// Returns a random int larger or equal to <paramref name="min"/> and smaller or equal to <paramref name="max"/>.
            /// </summary>
            /// <param name="min">The lowest possible value to generate</param>
            /// <param name="max">The highest possible value to generate</param>
            /// <returns>A random int in range of <paramref name="min"/> and <paramref name="max"/></returns>
            public static int GetRandomInt(int min, int max)
            {
                max.ValidateArgumentLarger(nameof(max), min);

                return _random.Next(min, max+1);
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

                return _random.NextDouble() * (max - min) + min;
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
    }
}
