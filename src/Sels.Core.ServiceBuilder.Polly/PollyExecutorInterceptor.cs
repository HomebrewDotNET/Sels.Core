using Castle.Core.Logging;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Polly;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.Core.ServiceBuilder.Template.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Sels.Core.Extensions.Logging;

namespace Sels.Core.ServiceBuilder.Polly
{
    /// <summary>
    /// Executes intercepted methods with Polly policies.
    /// </summary>
    /// <typeparam name="T">The type of the intercepted instance</typeparam>
    public class PollyExecutorInterceptor<T> : BaseResultlessInterceptor, IRootPolicyInterceptorBuilder<T>
    {
        // Fields
        private readonly ILogger _logger;
        private List<PollySyncMethodTarget> _syncPolicies = new List<PollySyncMethodTarget>();
        private List<PollyAsyncMethodTarget> _asyncPolicies = new List<PollyAsyncMethodTarget>();

        /// <inheritdoc cref="PollyExecutorInterceptor{T}"/>
        /// <param name="logger">Optional logger for tracing</param>
        public PollyExecutorInterceptor(ILogger<PollyExecutorInterceptor<T>> logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public ISelectSyncPolicyInterceptorBuilder<T> ForAll { 
            get {
                var target = _syncPolicies.FirstOrDefault(x => x.Target == null);
                if(target == null)
                {
                    target = new PollySyncMethodTarget(this, null);
                    _syncPolicies.Add(target);
                }
                return target;
            } 
        }
        /// <inheritdoc/>
        public ISelectAsyncPolicyInterceptorBuilder<T> ForAllAsync
        {
            get
            {
                var target = _asyncPolicies.FirstOrDefault(x => x.Target == null);
                if (target == null)
                {
                    target = new PollyAsyncMethodTarget(this, null);
                    _asyncPolicies.Add(target);
                }
                return target;
            }
        }
        /// <inheritdoc/>
        public ISelectSyncPolicyInterceptorBuilder<T> For(MethodInfo method)
        {
            method.ValidateArgument(nameof(method));
            var target = _syncPolicies.FirstOrDefault(x => x.Target.AreEqual(method));
            if (target == null)
            {
                target = new PollySyncMethodTarget(this, method);
                _syncPolicies.Add(target);
            }
            return target;
        }
        /// <inheritdoc/>
        public ISelectAsyncPolicyInterceptorBuilder<T> ForAsync(MethodInfo method)
        {
            method.ValidateArgument(nameof(method));
            var target = _asyncPolicies.FirstOrDefault(x => x.Target.AreEqual(method));
            if (target == null)
            {
                target = new PollyAsyncMethodTarget(this, method);
                _asyncPolicies.Add(target);
            }
            return target;
        }

        /// <inheritdoc/>
        protected override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            var isAsync = invocation.Method.CanRunAsync();
            var methodName = invocation.Method.GetDisplayName();

            if (isAsync)
            {
                var policyTargets = _asyncPolicies.Where(x => x.Target == null || x.Target.AreEqual(invocation.Method));

                if (policyTargets.HasValue())
                {
                    var policies = policyTargets.SelectMany(x => x.Policies).ToArray();
                    IAsyncPolicy policy = null;
                    if (policies.Length == 0) throw new InvalidOperationException($"Expected policies to be configured for method <{methodName}>");
                    else if (policies.Length > 1)
                    {
                        _logger.Debug($"<{policies.Length}> policies are configured for method <{methodName}>. Creating wrapper");
                        policy = Policy.WrapAsync(policies.OrderBy(x => x.Order).Select(x => x.Policy).ToArray());
                    }
                    else
                    {
                        policy = policies.First().Policy;
                    }

                    _logger.Log($"Intercepting async method <{methodName}> using <{policies.Length}> policies");
                    return policy.ExecuteAsync(() => proceed(invocation, proceedInfo));
                }
                else
                {
                    _logger.Debug($"Method <{methodName}> does not have any policies configured. Running normally");
                }
            }
            else
            {
                var policyTargets = _syncPolicies.Where(x => x.Target == null || x.Target.AreEqual(invocation.Method));

                if (policyTargets.HasValue())
                {
                    var policies = policyTargets.SelectMany(x => x.Policies).ToArray();
                    ISyncPolicy policy = null;
                    if (policies.Length == 0) throw new InvalidOperationException($"Expected policies to be configured for method <{methodName}>");
                    else if (policies.Length > 1)
                    {
                        _logger.Debug($"<{policies.Length}> policies are configured for method <{methodName}>. Creating wrapper");
                        policy = Policy.Wrap(policies.OrderBy(x => x.Order).Select(x => x.Policy).ToArray());
                    }
                    else
                    {
                        policy = policies.First().Policy;
                    }

                    _logger.Log($"Intercepting sync method <{methodName}> using <{policies.Length}> policies");
                    return policy.Execute(() => proceed(invocation, proceedInfo));
                }
                else
                {
                    _logger.Debug($"Method <{methodName}> does not have any policies configured. Running normally");
                }
            }
            return proceed(invocation, proceedInfo);
        }

        private class PollySyncMethodTarget : ISyncPolicyInterceptorBuilder<T>
        {
            // Fields
            private readonly PollyExecutorInterceptor<T> _parent;

            // Properties
            /// <summary>
            /// The method target. Null means the target for all methods.
            /// </summary>
            public MethodInfo Target { get; }
            /// <summary>
            /// The policies configured for <see cref="Target"/>.
            /// </summary>
            public List<(ISyncPolicy Policy, uint Order)> Policies { get; } = new List<(ISyncPolicy Policy, uint Order)>();

            /// <inheritdoc cref="PollySyncMethodTarget"/>
            /// <param name="parent">The instance that created the current instance</param>
            /// <param name="target"><inheritdoc cref="Target"/></param>
            public PollySyncMethodTarget(PollyExecutorInterceptor<T> parent, MethodInfo target)
            {
                _parent = parent.ValidateArgument(nameof(parent));
                Target = target;
            }

            /// <inheritdoc/>
            public ISelectSyncPolicyInterceptorBuilder<T> ForAll => _parent.ForAll;

            public ISelectAsyncPolicyInterceptorBuilder<T> ForAllAsync => _parent.ForAllAsync;
            /// <inheritdoc/>
            public ISelectSyncPolicyInterceptorBuilder<T> For(MethodInfo method) => _parent.For(method);
            /// <inheritdoc/>
            public ISelectAsyncPolicyInterceptorBuilder<T> ForAsync(MethodInfo method) => _parent.ForAsync(method);

            /// <inheritdoc/>
            public ISyncPolicyInterceptorBuilder<T> ExecuteWith(ISyncPolicy policy, uint order = 0)
            {
                Policies.Add((policy.ValidateArgument(nameof(policy)), order));
                return this;
            }
        }

        private class PollyAsyncMethodTarget : IAsyncPolicyInterceptorBuilder<T>
        {
            // Fields
            private readonly PollyExecutorInterceptor<T> _parent;

            // Properties
            /// <summary>
            /// The method target. Null means the target for all methods.
            /// </summary>
            public MethodInfo Target { get; }
            /// <summary>
            /// The policies configured for <see cref="Target"/>.
            /// </summary>
            public List<(IAsyncPolicy Policy, uint Order)> Policies { get; } = new List<(IAsyncPolicy Policy, uint Order)>();

            /// <inheritdoc cref="PollyAsyncMethodTarget"/>
            /// <param name="parent">The instance that created the current instance</param>
            /// <param name="target"><inheritdoc cref="Target"/></param>
            public PollyAsyncMethodTarget(PollyExecutorInterceptor<T> parent, MethodInfo target)
            {
                _parent = parent.ValidateArgument(nameof(parent));
                Target = target;
            }
            /// <inheritdoc/>
            public IAsyncPolicyInterceptorBuilder<T> ExecuteWith(IAsyncPolicy policy, uint order = 0)
            {
                Policies.Add((policy.ValidateArgument(nameof(policy)), order));
                return this;
            }
            /// <inheritdoc/>
            public ISelectSyncPolicyInterceptorBuilder<T> ForAll => _parent.ForAll;

            public ISelectAsyncPolicyInterceptorBuilder<T> ForAllAsync => _parent.ForAllAsync;
            /// <inheritdoc/>
            public ISelectSyncPolicyInterceptorBuilder<T> For(MethodInfo method) => _parent.For(method);
            /// <inheritdoc/>
            public ISelectAsyncPolicyInterceptorBuilder<T> ForAsync(MethodInfo method) => _parent.ForAsync(method);
        }
    }


}
