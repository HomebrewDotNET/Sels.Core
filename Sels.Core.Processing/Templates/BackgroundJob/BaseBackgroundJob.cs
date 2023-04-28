using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Converters;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Template for implementing a <see cref="IBackgroundJob"/>.
    /// Base class provides methods for parsing the input and adds exception handling.
    /// </summary>
    /// <typeparam name="T">The type of the input</typeparam>
    public abstract class BaseBackgroundJob<T> : IBackgroundJob
    {
        // Properties
        /// <inheritdoc/>
        public IBackgroundJobContext Context { get; set; }
        /// <inheritdoc/>
        public IBackgroundJobState State { get; set; }
        /// <inheritdoc/>
        public ILogger Logger { get; set; }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(string input, CancellationToken token = default)
        {
            using var methodLogger = Logger.TraceMethod(this);
            try
            {
                // Parse input
                Debug($"Parsing input of length <{input?.Length}> to type <{typeof(T)}>");
                var typedInput = await ParseInputAsync(input);
                Debug($"Parsed input. Validating");
                var errors = await ValidateInputAsync(typedInput);
                if (errors.HasValue()) throw new InvalidBackgroundJobInputException($"Input for background job <{Context.Id}> is not valid: {Environment.NewLine}{errors.JoinStringNewLine()}");

                token.ThrowIfCancellationRequested();

                // Execute job
                Debug($"Input is valid. Executing background job");
                return await ExecuteAsync(typedInput, token);
            }
            catch (OperationCanceledException)
            {
                Log($"Job execution was requested to stop");
                throw;
            }
            catch(InvalidBackgroundJobInputException inputException)
            {
                Error($"Input for background job is invalid so can't execute. Failing job");
                return this.Fail(inputException.Message);
            }
            catch(BackgroundJobInterruptedException interruptException)
            {
                Debug($"Background job interrupted with result of type <{interruptException.Result.GetType()}>");
                return interruptException.Result;
            }
            catch(Exception ex)
            {
                var exceptionResult = await HandleJobExceptionAsync(ex);
                if(exceptionResult != null)
                {
                    Debug($"Exception occured during processing resulting in a result of type <{exceptionResult.GetType()}>. Returning as job result", ex);
                }

                AddLogEntry(Context.IsOnLastRetry ? LogLevel.Error : LogLevel.Warning, $"Exception occured while processing the job");
                throw;
            }
        }

        // Virtuals
        /// <summary>
        /// Parses the background job input to <typeparamref name="T"/>. Uses <see cref="GenericConverter.DefaultJsonConverter"/> by default.
        /// </summary>
        /// <param name="input">The input to parse</param>
        /// <returns><paramref name="input"/> parsed to an instance of <typeparamref name="T"/></returns>
        /// <exception cref="InvalidBackgroundJobInputException"></exception>
        protected virtual Task<T> ParseInputAsync(string input)
        {
            using var methodLogger = Logger.TraceMethod(this);
            if (typeof(T).IsString()) return input.CastTo<T>().ToTaskResult();
            if (input == null) return default;

            if (GenericConverter.DefaultJsonConverter.TryConvertTo<T>(input, out var converted))
            {
                return converted.ToTaskResult();
            }

            throw new InvalidBackgroundJobInputException($"Could not convert background job input to requested type <{typeof(T)}>");
        }

        /// <summary>
        /// Optional method to validate the parsed input.
        /// </summary>
        /// <param name="input">The input to validate</param>
        /// <returns>Any validation errors for the input. Returned null or no errors means the input is valid</returns>
        protected Task<IEnumerable<string>> ValidateInputAsync(T input) => Task.FromResult((IEnumerable<string>)null);

        /// <summary>
        /// Optional method that can be implemented to handle any exception thrown during processing.
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        /// <returns>The result to return for the job if needed. Returning null will cause the exception to be rethrown so the job will be retried at a later date</returns>
        protected virtual Task<object> HandleJobExceptionAsync(Exception exception) => Task.FromResult((object)null);

        // Abstractions
        /// <summary>
        /// Executes the current background job.
        /// </summary>
        /// <param name="input">The deserialized input</param>
        /// <param name="token">Cancellation token that will be cancelled if the job is requested to stop processing</param>
        /// <returns>Optional result from executing the background job</returns>
        protected abstract Task<object> ExecuteAsync(T input, CancellationToken token);

        #region Helpers
        #region Parameters
        /// <inheritdoc cref="IBackgroundJobState.Get{T}(string)"/>

        protected TData Get<TData>(string name) => State.Get<TData>(name);
        /// <inheritdoc cref="IBackgroundJobState.Set{T}(string, T)"/>

        protected void Set<TData>(string name, TData data) => State.Set(name, data);
        #endregion

        #region ExecutedActions
        /// <inheritdoc cref="IBackgroundJobState.IsActionExecuted(string)"/>

        protected bool IsActionExecuted(string action) => State.IsActionExecuted(action);
        /// <inheritdoc cref="IBackgroundJobState.AddExecutedAction(string, string)"/>

        protected void AddExecutedAction(string action, string message = null) => State.AddExecutedAction(action, message);
        /// <inheritdoc cref="IBackgroundJobState.AddExecutedAction{T}(string, T, string)"/>

        protected void AddExecutedAction<TData>(string action, TData data, string message = null) => State.AddExecutedAction(action, data, message);
        /// <inheritdoc cref="IBackgroundJobState.TryGetExecutedActionState{T}(string, out T)"/>

        protected bool TryGetExecutedActionState<TData>(string action, out TData data) => State.TryGetExecutedActionState(action, out data);
        #endregion

        #region Logging
        /// <inheritdoc cref="IBackgroundJobState.AddLogEntry(LogLevel, string, Exception)"/>
        protected void AddLogEntry(LogLevel logLevel, string message, Exception exception = null) => State.AddLogEntry(logLevel, message, exception);
        /// <inheritdoc cref="IBackgroundJobState.Trace(string, Exception)"/>
        protected void Trace(string message, Exception exception = null) => State.Trace(message, exception);
        /// <inheritdoc cref="IBackgroundJobState.Debug(string, Exception)"/>
        protected void Debug(string message, Exception exception = null) => State.Debug(message, exception);
        /// <inheritdoc cref="IBackgroundJobState.Log(string, Exception)"/>
        protected void Log(string message, Exception exception = null) => State.Log(message, exception);
        /// <inheritdoc cref="IBackgroundJobState.Warning(string, Exception)"/>
        protected void Warning(string message, Exception exception = null) => State.Warning(message, exception);
        /// <inheritdoc cref="IBackgroundJobState.Error(string, Exception)"/>
        protected void Error(string message, Exception exception = null) => State.Error(message, exception);
        #endregion
        #endregion
    }
}
