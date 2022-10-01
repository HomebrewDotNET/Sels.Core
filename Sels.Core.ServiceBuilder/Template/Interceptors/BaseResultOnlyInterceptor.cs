using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Template.Interceptors
{
    /// <summary>
    /// Base class for interceptors that only intercept methods with a result.
    /// </summary>
    public abstract class BaseResultOnlyInterceptor : BaseInterceptor
    {
        /// <inheritdoc/>
        protected override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            return proceed(invocation, proceedInfo);
        }
    }
}
