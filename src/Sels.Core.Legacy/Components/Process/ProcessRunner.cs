using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.DateTimes;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using SystemProcess = System.Diagnostics.Process;

namespace Sels.Core.Process
{
    /// <inheritdoc cref="IProcessRunner"/>
    public class ProcessRunner : IProcessRunner
    {
        // Statics
#if NETCOREAPP1_0_OR_GREATER
        private static readonly bool _isLinux = OperatingSystem.IsLinux();
#else
        private static readonly bool _isLinux = false;
#endif

        // Fields
        private readonly string _fileName;
        private List<ILogger> _loggers = new List<ILogger>();

        private int _sleepTime = 100;
        private int _killTime = 10000;

        private string _user;
        private string _domain;
        private Action<SecureString> _passwordBuilder;

        private List<Action> _exitHandlers = new List<Action>();
        private List<Action<string>> _standardOutputHandlers = new List<Action<string>>();
        private List<Action<string>> _errorOutputHandlers = new List<Action<string>>();

        private ProcessPriorityClass? _priority;
        private bool _useWindow = false;
        private bool _useShell = false;
        private bool _keepStandardOutput = true;
        private bool _keepErrorOutput = true;

        /// <inheritdoc cref="ProcessRunner"/>
        /// <param name="fileName">Filename of the process to run</param>
        public ProcessRunner(string fileName)
        {
            _fileName = fileName.ValidateArgumentNotNullOrWhitespace(nameof(fileName));
        }

#region Setup
        /// <inheritdoc/>
        public IProcessRunner AsUser(string username, Action<SecureString> passwordBuilder, string domain = null)
        {
            username.ValidateArgumentNotNullOrWhitespace(nameof(username));
            passwordBuilder.ValidateArgument(nameof(passwordBuilder));

            _user = username;
            _passwordBuilder = passwordBuilder;
            _domain = domain;
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner AsUser(string username, string password = null, string domain = null)
        {
            return AsUser(username, x =>
            {
                if(password != null)
                {
                    foreach(var character in password)
                    {
                        x.AppendChar(character);
                    }
                }
            }, domain);
        }
        /// <inheritdoc/>
        public IProcessRunner OnErrorOutput(Action<string> handler)
        {
            handler.ValidateArgument(nameof(handler));

            _errorOutputHandlers.Add(handler);
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner OnExit(Action action)
        {
            action.ValidateArgument(nameof(action));

            _exitHandlers.Add(action);
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner OnStandardOutput(Action<string> handler)
        {
            handler.ValidateArgument(nameof(handler));

            _standardOutputHandlers.Add(handler);
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner OnOutput(Action<string> handler)
        {
            return OnStandardOutput(handler).OnErrorOutput(handler);
        }
        /// <inheritdoc/>
        public IProcessRunner UseShell()
        {
            _useShell = true;
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithKillTime(int killTime)
        {
            killTime.ValidateArgumentLargerOrEqual(nameof(killTime), 0);

            _killTime = killTime;
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithLogger(ILogger logger)
        {
            if(logger != null)
            {
                _loggers.Add(logger);
            }
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithSleepTime(int sleepTime)
        {
            _sleepTime = sleepTime.ValidateArgumentLargerOrEqual(nameof(sleepTime), 0);
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithWindow()
        {
            _useWindow = true;
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithPriority(ProcessPriorityClass processPriority)
        {
            _priority = processPriority;
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithoutStandardOutput()
        {
            _keepStandardOutput = false;
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithoutErrorOutput()
        {
            _keepErrorOutput = false;
            return this;
        }
        /// <inheritdoc/>
        public IProcessRunner WithoutOutput()
        {
            return WithoutStandardOutput().WithoutErrorOutput();
        }
        #endregion

        /// <inheritdoc/>
        public async Task<ProcessExecutionResult> ExecuteAsync(string arguments = null, CancellationToken token = default)
        {
            using (var timedLogger = _loggers.CreateTimedLogger(LogLevel.Debug, $"Executing program {_fileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)}", x => $"Executed program {_fileName}{(arguments.HasValue() ? $" with arguments {arguments}" : string.Empty)} in {x.PrintTotalMs()}"))
            {
                var startInfo = new ProcessStartInfo(_fileName, arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = _useShell,
                    CreateNoWindow = _useWindow
                };

                // Set user if enabled
                if (_user.HasValue())
                {
                    startInfo.UserName = _user;
                    if (!_isLinux)
                    {
                        var securePassword = new SecureString();
                        _passwordBuilder(securePassword);
#pragma warning disable CA1416 // Validate platform compatibility
                        startInfo.Password = securePassword;
                        startInfo.Domain = _domain;
#pragma warning restore CA1416 // Validate platform compatibility
                    }
                }

                // Prepare process
                var timer = new Stopwatch();
                DateTime? exitTime = null;
                var outputLines = new List<string>();
                var errorLines = new List<string>();
                var process = new SystemProcess()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };
                process.OutputDataReceived += (s, a) =>
                {
                    timedLogger.Log((t, l) => l.Trace($"Process {process.Id} standard output: {a.Data} ({t.PrintTotalMs()})"));
                    if(_keepStandardOutput) outputLines.Add(a.Data);
                    _standardOutputHandlers.ForceExecute(x => x(a.Data), (x, e) => _loggers.LogException(LogLevel.Warning, e));
                };
                process.ErrorDataReceived += (s, a) =>
                {
                    timedLogger.Log((t, l) => l.Trace($"Process {process.Id} error output: {a.Data} ({t.PrintTotalMs()})"));
                    if(_keepErrorOutput) errorLines.Add(a.Data);
                    _errorOutputHandlers.ForceExecute(x => x(a.Data), (x, e) => _loggers.LogException(LogLevel.Warning, e));
                };
                process.Exited += (s, a) => {
                    timer.Stop();
                    exitTime = DateTime.Now;
                    timedLogger.Log((t, l) => l.Debug($"Process {process.Id} has exited ({t.PrintTotalMs()})"));
                    _exitHandlers.ForceExecute(x => x(), (x, e) => _loggers.LogException(LogLevel.Warning, e));
                };

                try
                {
                    process.Start();
                    var startTime = DateTime.Now;
                    if (!process.HasExited) startTime = process.StartTime;
                    if(_priority.HasValue) process.PriorityClass = _priority.Value;
                    timer.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Started process {process.Id} ({time.PrintTotalMs()})"));

                    // Wait for process to finish
                    while (!process.HasExited)
                    {
                        timedLogger.Log((time, log) => log.LogMessage(LogLevel.Trace, $"Waiting for process {process.Id} to exit ({time.PrintTotalMs()})"));
                        await Helper.Async.Sleep(_sleepTime);

                        // Wait for process to exit
                        if (token.IsCancellationRequested && !process.HasExited)
                        {
                            timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killing process {process.Id} ({time.PrintTotalMs()})"));
                            var killTask = Task.Run(() => { if (!process.HasExited) process.Kill(); });
                            timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Sent kill signal to process {process.Id} and will now wait for maximum {_killTime}ms for it to exit ({time.PrintTotalMs()})"));

#if NET5_0_OR_GREATER
                            // Trigger token after kill time
                            bool hasExitedGracefully = true;
                            var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(_killTime.ChangeType<double>()));
                            using (var tokenRegistration = tokenSource.Token.Register(() => hasExitedGracefully = false))
                            {
                                await process.WaitForExitAsync(tokenSource.Token);
                            }
#else
                            bool hasExitedGracefully = process.WaitForExit(_killTime);
#endif

                            if (!hasExitedGracefully)
                            {
                                timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killed process {process.Id} could not gracefully exit within {_killTime}ms ({time.PrintTotalMs()})"));
                                throw new TaskCanceledException($"Process {process.Id} could not properly stop in {_killTime}ms");
                            }
                            else
                            {
                                timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Killed process {process.Id} exited gracefully ({time.PrintTotalMs()})"));
                                await killTask;
                                break;
                            }
                        }
                    }

                    timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Process {process.Id} has exited with code {process.ExitCode} ({time.PrintTotalMs()})"));
                    timer.Stop();

                    return new ProcessExecutionResult(process.ExitCode, outputLines, errorLines, startTime, exitTime ?? DateTime.Now, timer.Elapsed);
                }
                finally
                {
                    timedLogger.Log((time, log) => log.LogMessage(LogLevel.Debug, $"Disposing process {process.Id} ({time.PrintTotalMs()})"));
                    timer.Stop();
                    process.Dispose();
                }
            }
        }
    }
}
