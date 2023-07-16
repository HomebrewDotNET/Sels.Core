using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Cli.ArgumentParsing;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Cli
{
    internal class CommandLineTool<TArg> : ICommandLineSyncToolBuilder<TArg>, ICommandLineAsyncToolBuilder<TArg> where TArg : new()
    {
        // Fields
        private readonly List<Action<IServiceProvider, TArg?>> _syncExecutions = new List<Action<IServiceProvider, TArg?>>();
        private readonly List<Func<IServiceProvider, TArg?, CancellationToken, Task>> _asyncExecutions = new List<Func<IServiceProvider, TArg?, CancellationToken, Task>>();
        private readonly Dictionary<Type, Func<Exception, int>> _exceptionHandlers = new Dictionary<Type, Func<Exception, int>>();
        private Func<string[]?, TArg>? _parser;
        private Action<IServiceCollection, TArg?> _serviceBuilder;

        /// <inheritdoc/>
        public ICommandLineSyncToolBuilder<TArg> Execute(Action<IServiceProvider, TArg?> action)
        {
            action.ValidateArgument(nameof(action));

            _syncExecutions.Add(action);
            return this;
        }
        /// <inheritdoc/>
        public ICommandLineAsyncToolBuilder<TArg> Execute(Func<IServiceProvider, TArg?, CancellationToken, Task> action)
        {
            action.ValidateArgument(nameof(action));

            _asyncExecutions.Add(action);
            return this;
        }

        /// <inheritdoc/>
        public ICommandLineSyncToolBuilder<TArg> Handles<TException>(Func<TException, int> handler) where TException : Exception
        {
            handler.ValidateArgument(nameof(handler));

            if (_exceptionHandlers.ContainsKey(typeof(TException))) throw new InvalidOperationException($"Exception handler for <{typeof(TException)}> already configured");

            _exceptionHandlers.Add(typeof(TException), x => handler((TException)x));
            return this;
        }
        /// <inheritdoc/>
        ICommandLineAsyncToolBuilder<TArg> ICommandLineToolBuilder<TArg, ICommandLineAsyncToolBuilder<TArg>>.Handles<TException>(Func<TException, int> handler)
        {
            handler.ValidateArgument(nameof(handler));

            if (_exceptionHandlers.ContainsKey(typeof(TException))) throw new InvalidOperationException($"Exception handler for <{typeof(TException)}> already configured");

            _exceptionHandlers.Add(typeof(TException), x => handler((TException)x));
            return this;
        }
        /// <inheritdoc/>
        public ICommandLineSyncToolBuilder<TArg> ParseArgumentsUsing(Func<string[], TArg> parser)
        {
            parser.ValidateArgument(nameof(parser));

            _parser = parser;
            return this;
        }
        /// <inheritdoc/>
        ICommandLineAsyncToolBuilder<TArg> ICommandLineToolBuilder<TArg, ICommandLineAsyncToolBuilder<TArg>>.ParseArgumentsUsing(Func<string[], TArg> parser)
        {
            parser.ValidateArgument(nameof(parser));

            _parser = parser;
            return this;
        }
        /// <inheritdoc/>
        public ICommandLineSyncToolBuilder<TArg> ConfigureServices(Action<IServiceCollection, TArg?> action)
        {
            action.ValidateArgument(nameof(action));

            _serviceBuilder = action;
            return this;
        }
        /// <inheritdoc/>
        ICommandLineAsyncToolBuilder<TArg> ICommandLineToolBuilder<TArg, ICommandLineAsyncToolBuilder<TArg>>.ConfigureServices(Action<IServiceCollection, TArg?> action)
        {
            action.ValidateArgument(nameof(action));

            _serviceBuilder = action;
            return this;
        }
        /// <inheritdoc/>
        public int Run(string[]? args = null)
        {
            try
            {
                // Setup token
                var cancellationSource = new CancellationTokenSource();
                // Cancel token when application is about to exit.
                Helper.App.OnExit(() => cancellationSource.Cancel());
                // Cancel token when user wants to abort
                Console.CancelKeyPress += (s, e) =>
                {
                    cancellationSource.Cancel();
                    e.Cancel = true;
                };

                // Parse arguments
                TArg arguments = new();
                if (typeof(TArg).Is<string[]>()) arguments = args.CastToOrDefault<TArg>();
                else if (args.HasValue()) arguments = _parser != null ? _parser(args) : CreateArguments(args);

                // Create service provider
                var collection = new ServiceCollection();
                if (_serviceBuilder != null) _serviceBuilder(collection, arguments);

                using (var provider = collection.BuildServiceProvider())
                {
                    foreach (var execution in _syncExecutions)
                    {
                        // Execute tool
                        try
                        {
                            using(var scope = provider.CreateScope())
                            {
                                execution(scope.ServiceProvider, arguments);
                            }
                        }
                        catch (CommandLineException cliEx)
                        {
                            Console.Error.WriteLine(cliEx.Message);
                            return cliEx.ExitCode;
                        }
                        catch (Exception ex)
                        {
                            var handler = _exceptionHandlers.Where(x => ex.GetType().IsAssignableTo(x.Key)).Select(x => x.Value).FirstOrDefault();

                            if (handler != null)
                            {
                                return handler(ex);
                            }
                            Console.Error.WriteLine($"Could not execute tool successfully: {ex.Message}");
                            return CommandLine.ErrorExitCode;
                        }  
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return CommandLine.ErrorExitCode;
            }

            return CommandLine.SuccesfulExitCode;
        }
        /// <inheritdoc/>
        public async Task<int> RunAsync(string[]? args = null)
        {
            try
            {
                // Setup token
                var cancellationSource = new CancellationTokenSource();
                // Cancel token when application is about to exit.
                Helper.App.OnExit(() => cancellationSource.Cancel());
                // Cancel token when user wants to abort
                Console.CancelKeyPress += (s, e) =>
                {
                    cancellationSource.Cancel();
                    e.Cancel = true;
                };

                // Parse arguments
                TArg arguments = new();
                if (typeof(TArg).Is<string[]>()) arguments = args.CastToOrDefault<TArg>();
                else if (args.HasValue()) arguments = _parser != null ? _parser(args) : CreateArguments(args);

                // Create service provider
                var collection = new ServiceCollection();
                if (_serviceBuilder != null) _serviceBuilder(collection, arguments);

                using (var provider = collection.BuildServiceProvider())
                {
                    foreach (var execution in _asyncExecutions)
                    {
                        // Execute tool
                        try
                        {
                            await using(var scope = provider.CreateAsyncScope())
                            {
                                await execution(scope.ServiceProvider, arguments, cancellationSource.Token);
                            }
                        }
                        catch (CommandLineException cliEx)
                        {
                            Console.Error.WriteLine(cliEx.Message);
                            return cliEx.ExitCode;
                        }
                        catch (Exception ex)
                        {
                            var handler = _exceptionHandlers.Where(x => ex.GetType().IsAssignableTo(x.Key)).Select(x => x.Value).FirstOrDefault();

                            if (handler != null)
                            {
                                return handler(ex);
                            }
                            Console.Error.WriteLine($"Could not execute tool successfully: {ex.Message}");
                            return CommandLine.ErrorExitCode;
                        }  
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return CommandLine.ErrorExitCode;
            }

            return CommandLine.SuccesfulExitCode;
        }

        private TArg CreateArguments(string[] args)
        {
            args.ValidateArgumentNotNullOrEmpty(nameof(args));

            TArg result = default;
            var parsedResult = Parser.Default.ParseArguments<TArg>(args);
            parsedResult.WithParsed(x => result = x).WithNotParsed(x => {
                var builder = SentenceBuilder.Create();
                throw new InvalidCommandLineArgumentsException(HelpText.RenderParsingErrorsTextAsLines(parsedResult, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1));
                });
            return result ?? new TArg();
        }
    }
}
