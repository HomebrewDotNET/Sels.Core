using NUnit.Framework;
using Sels.Core.Components;
using System;

namespace Sels.Core.Test.Components
{
    [TestFixture]
    public class EmptyDisposableTests
    {
        [Test]
        public void Instance_ShouldBeSingleton()
        {
            // Act
            var instance1 = EmptyDisposable.Instance;
            var instance2 = EmptyDisposable.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
            Assert.IsNotNull(instance1);
        }

        [Test]
        public void Dispose_ShouldNotThrow()
        {
            // Arrange
            var disposable = EmptyDisposable.Instance;

            // Act & Assert
            Assert.DoesNotThrow(() => disposable.Dispose());
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var disposable = EmptyDisposable.Instance;

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                disposable.Dispose();
                disposable.Dispose();
                disposable.Dispose();
            });
        }

        [Test]
        public void Instance_ShouldBeSealed()
        {
            // Act
            var type = typeof(EmptyDisposable);

            // Assert
            Assert.IsTrue(type.IsSealed, "EmptyDisposable should be sealed to prevent inheritance");
        }

        [Test]
        public void Instance_ShouldImplementIDisposable()
        {
            // Arrange
            var instance = EmptyDisposable.Instance;

            // Assert
            Assert.IsInstanceOf<IDisposable>(instance);
        }
    }
}