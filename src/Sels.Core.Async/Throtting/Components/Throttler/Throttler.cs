using Microsoft.Extensions.Logging;
using Sels.Core.Async.TaskManagement;
using Sels.Core.Extensions;
using Sels.Core.Models;
using Sels.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Text;

namespace Sels.Core.Async.Throtting.Components.Throttler
{
    /// <inheritdoc cref="IThrottler"/>
    public class Throttler : IThrottler
    {
        // Fields
        private readonly ILogger? _logger;
        private readonly ILoggerFactory? _loggerFactory;
        private readonly ITaskManager _taskManager;
        private readonly IValidator<ThrottledWorkerOptions, string, object> _validator;

        /// <inheritdoc cref="Throttler(ITaskManager, IValidator{ThrottledWorkerOptions, string, object}, ILoggerFactory?)"/>
        public Throttler(ILoggerFactory loggerFactory, ITaskManager taskManager, IValidator<ThrottledWorkerOptions, string, object> validator)
            : this(taskManager, validator, loggerFactory)
        {
        }

        /// <summary>Creates a throttler with optional logging.</summary>
        /// <param name="taskManager">The task manager used to schedule work.</param>
        /// <param name="validator">The options validator.</param>
        /// <param name="loggerFactory">An optional logger factory.</param>
        public Throttler(ITaskManager taskManager, IValidator<ThrottledWorkerOptions, string, object> validator, ILoggerFactory? loggerFactory = null)
        {
            _taskManager = taskManager.ValidateArgument(nameof(taskManager));
            _validator = validator.ValidateArgument(nameof(validator));
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.CreateLogger<Throttler>();
        }

        /// <inheritdoc/>
        public IThrottledWorker<TResult, TContext> CreateWorker<TResult, TContext>(TResult result, TContext context, ThrottledWorkerOptions? options = null, CancellationToken cancellationToken = default)
            where TResult : class
            where TContext : class
        {
            result = result.ValidateArgument(nameof(result));
            context = context.ValidateArgument(nameof(context));
            options = options ?? ThrottledWorkerOptions.Default;

            var errors = _validator.Validate(options);
            if(errors.HasValue())
            {
                throw new ArgumentException($"Invalid throttled worker options: {errors.JoinString(", ")}", nameof(options));
            }

            _logger.Debug($"Creating throttled worker with result <{{{AsyncLog.Throttling.Result}}}> and context <{{{AsyncLog.Throttling.Context}}}>", result, context);
            return new ThrottledWorker<TResult, TContext>(_taskManager, result, context, options, cancellationToken, _loggerFactory?.CreateLogger<ThrottledWorker<TResult, TContext>>());
        }
        /// <inheritdoc/>
        public IThrottledWorker<TResult, Null> CreateWorker<TResult>(TResult result, ThrottledWorkerOptions? options = null, CancellationToken cancellationToken = default) where TResult : class
        {
            return CreateWorker<TResult, Null>(result, Null.Value, options, cancellationToken);
        }
    }
}
