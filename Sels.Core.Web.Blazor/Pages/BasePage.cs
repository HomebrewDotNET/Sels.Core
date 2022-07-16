using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Blazor.Pages
{
    /// <summary>
    /// Template for creating blazor page.
    /// Contains a cancellation token that gets cancelled when the page is navigated away from.
    /// </summary>
    public abstract class BasePage : ComponentBase, IDisposable
    {
        // Fields
        private CancellationTokenSource? _tokenSource;

        // Properties
        /// <summary>
        /// Cancellation token that gets cancelled when the page is navigated away from. 
        /// </summary>
        protected CancellationToken Token => (_tokenSource ??= new()).Token;

        /// <inheritdoc/>
        public void Dispose()
        {
            if(_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }
    }
}
