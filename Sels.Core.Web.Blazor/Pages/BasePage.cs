using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;
using Sels.Core.Extensions.Conversion;

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
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        // Properties
        /// <summary>
        /// Cancellation token that gets cancelled when the page is navigated away from. 
        /// </summary>
        protected CancellationToken Token => (_tokenSource ??= new()).Token;
        /// <summary>
        /// If the component has been rendered for the first time.
        /// </summary>
        protected bool Rendered { get; private set; }

        /// <summary>
        /// Gets the value for property with name <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="T">Type of the property value</typeparam>
        /// <param name="property">The name of the property. When not provided the compiler will fill out the property name</param>
        /// <returns>The value for property with name <paramref name="property"/></returns>
        protected T GetValue<T>([CallerMemberName] string? property = null)
        {
            property.ValidateArgument(nameof(property));

            return _properties.TryGetOrSet(property.StartsWith("get_") ? property.Remove(0, 4) : property, default(T)).Cast<T>();
        }

        /// <summary>
        /// Sets the value for property with name <paramref name="property"/>.
        /// Calls <see cref="ComponentBase.StateHasChanged"/> so a render is triggered.
        /// </summary>
        /// <typeparam name="T">Type of the property value</typeparam>
        /// <param name="value">The value to set</param>
        /// <param name="property">The name of the property. When not provided the compiler will fill out the property name</param>
        protected void SetValue<T>(T value, [CallerMemberName] string? property = null)
        {
            property.ValidateArgument(nameof(property));

            _properties.AddOrUpdate(property.StartsWith("set_") ? property.Remove(0, 4) : property, value);
            StateHasChanged();
        }
        /// <inheritdoc/>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                Rendered = true;
                OnAfterFirstRender();
            }
        }
        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender) await OnAfterFirstRenderAsync();
        }

        /// <summary>
        /// Method invoked after the first render.
        /// </summary>
        protected virtual Task OnAfterFirstRenderAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method invoked after the first render.
        /// </summary>
        protected virtual void OnAfterFirstRender()
        {
        }

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
