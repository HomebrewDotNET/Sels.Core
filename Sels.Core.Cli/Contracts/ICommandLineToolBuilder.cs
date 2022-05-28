using Microsoft.Extensions.DependencyInjection;
using Sels.Core.Contracts.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Cli
{
    /// <summary>
    /// Builder for creating and executing a command line tool that executes sync code.
    /// </summary>
    /// <typeparam name="TArg">Type of object containing the parsed command line arguments</typeparam>
    public interface ICommandLineSyncToolBuilder<TArg> : ICommandLineToolBuilder<TArg, ICommandLineSyncToolBuilder<TArg>> where TArg : new()
    {
        /// <summary>
        /// Defines an action to be executed by the tool using the provided command line arguments.
        /// </summary>
        /// <param name="action">Delegate that executes the code in the tool</param>
        /// <returns>Current builder for method chaining</returns>
        ICommandLineSyncToolBuilder<TArg> Execute(Action<IServiceProvider, TArg?> action);
        /// <summary>
        /// Executes the command line tool.
        /// </summary>
        /// <param name="args">The arguments provided to the tool</param>
        /// <returns>The exit code of the tool</returns>
        int Run(string[]? args = null);
    }

    /// <summary>
    /// Builder for creating and executing a command line tool that executes async code.
    /// </summary>
    /// <typeparam name="TArg">Type of object containing the parsed command line arguments</typeparam>
    public interface ICommandLineAsyncToolBuilder<TArg> : ICommandLineToolBuilder<TArg, ICommandLineAsyncToolBuilder<TArg>> where TArg : new()
    {
        /// <summary>
        /// Defines an action to be executed by the tool using the provided command line arguments.
        /// </summary>
        /// <param name="action">Delegate that executes the code in the tool</param>
        /// <returns>Current builder for method chaining</returns>
        ICommandLineAsyncToolBuilder<TArg> Execute(Func<IServiceProvider, TArg?, CancellationToken, Task> action);
        /// <summary>
        /// Executes the command line tool.
        /// </summary>
        /// <param name="args">The arguments provided to the tool</param>
        /// <returns>The exit code of the tool</returns>
        Task<int> RunAsync(string[]? args = null);
    }
    /// <summary>
    /// Builder for configuring a command line tool.
    /// </summary>
    /// <typeparam name="TArg">Type of object containing the parsed command line arguments</typeparam>
    /// <typeparam name="TDerived">Type of the deriving builder</typeparam>
    public interface ICommandLineToolBuilder<TArg, TDerived>
    {
        /// <summary>
        /// Builds the service collection for injecting services.
        /// </summary>
        /// <param name="action">Action that configures the service collection</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived RegisterServices(Action<IServiceCollection, TArg?> action);
        /// <summary>
        /// Overwrites the default parser with <paramref name="parser"/>.
        /// </summary>
        /// <param name="parser">Delegate that creates <typeparamref name="TArg"/> using the provided</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived ParseArgumentsUsing(Func<string[], TArg> parser);
        /// <summary>
        /// Defines a handler for handling exceptions of type <typeparamref name="TException"/> when an unhandled exception gets thrown.
        /// </summary>
        /// <typeparam name="TException">Type of the exception to handle</typeparam>
        /// <param name="handler">Delegate that handles the exception. Returns the exit code</param>
        /// <returns>Current builder for method chaining</returns>
        TDerived Handles<TException>(Func<TException, int> handler) where TException : Exception;
    }
}
