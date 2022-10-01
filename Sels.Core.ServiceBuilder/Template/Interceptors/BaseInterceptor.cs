using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
