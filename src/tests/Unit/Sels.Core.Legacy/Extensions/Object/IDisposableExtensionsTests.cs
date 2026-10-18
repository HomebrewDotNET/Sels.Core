using Sels.Core.Extensions.Disposable;

namespace Sels.Core.Legacy.Tests.Extensions.Object
{
    /// <summary>Contract tests for disposal helpers.</summary>
    [Trait("Category", "Unit")]
    public class IDisposableExtensionsTests
    {
        /// <summary>TryDispose returns false for null and non-disposable values.</summary>
        [Fact]
        public void TryDispose_NullOrNonDisposable_ReturnsFalse()
        {
            object? value = null;
            var nonDisposable = new object();

            var nullResult = IDisposableExtensions.TryDispose<object>(value);
            var nonDisposableResult = nonDisposable.TryDispose();

            Assert.False(nullResult);
            Assert.False(nonDisposableResult);
        }

        /// <summary>TryDispose returns true and disposes a disposable value.</summary>
        [Fact]
        public void TryDispose_DisposeSucceeds_ReturnsTrue()
        {
            var value = new DisposableProbe();

            var result = value.TryDispose<DisposableProbe>();

            Assert.True(result);
            Assert.True(value.IsDisposed);
        }

        /// <summary>TryDispose reports the original exception and contains handler failures.</summary>
        [Fact]
        public void TryDispose_DisposeOrHandlerThrows_ReturnsFalse()
        {
            var expected = new InvalidOperationException("dispose failed");
            var value = new DisposableProbe(expected);
            Exception? observed = null;

            var result = value.TryDispose<DisposableProbe>(exception => observed = exception);
            var handlerFailureResult = value.TryDispose<DisposableProbe>(_ => throw new InvalidOperationException());

            Assert.False(result);
            Assert.Same(expected, observed);
            Assert.False(handlerFailureResult);
        }

        private sealed class DisposableProbe : IDisposable
        {
            private readonly Exception? _exception;

            public bool IsDisposed { get; private set; }

            public DisposableProbe(Exception? exception = null)
            {
                _exception = exception;
            }

            public void Dispose()
            {
                IsDisposed = true;
                if (_exception != null)
                {
                    throw _exception;
                }
            }
        }
    }
}