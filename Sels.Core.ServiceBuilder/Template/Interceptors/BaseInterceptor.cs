using Castle.DynamicProxy;

namespace Sels.Core.ServiceBuilder.Template.Interceptors
{
    /// <summary>
    /// Base class for creating new interceptors.
    /// </summary>
    public abstract class BaseInterceptor : AsyncInterceptorBase, IInterceptor
    {
        /// <inheritdoc/>
        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }
    }
}
