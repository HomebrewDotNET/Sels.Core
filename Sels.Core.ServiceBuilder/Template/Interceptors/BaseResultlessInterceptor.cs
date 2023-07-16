using Castle.DynamicProxy;
using System;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Template.Interceptors
{
    /// <summary>
    /// Base class for creating interceptors where the result of the method isn't needed.
    /// </summary>
    public abstract class BaseResultlessInterceptor : BaseInterceptor
    {
        /// <inheritdoc/>
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            TResult result = default;
            await InterceptAsync(invocation, proceedInfo, async (i, info) =>
            {
                result = await proceed(invocation, proceedInfo);
            });
            return result;
        }
    }
}
