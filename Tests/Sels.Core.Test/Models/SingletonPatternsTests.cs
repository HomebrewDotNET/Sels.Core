using NUnit.Framework;
using Sels.Core.Cli.ArgumentParsing;
using System;
using System.Threading.Tasks;

namespace Sels.Core.Test.Models
{
    [TestFixture]
    public class SingletonPatternsTests
    {
        [Test]
        public void NullArguments_Instance_ShouldBeSingleton()
        {
            // Act
            var instance1 = NullArguments.Instance;
            var instance2 = NullArguments.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
            Assert.IsNotNull(instance1);
        }

        [Test]
        public void NullArguments_Instance_ShouldBeThreadSafe()
        {
            // Arrange
            NullArguments instance1 = null;
            NullArguments instance2 = null;

            // Act
            var task1 = Task.Run(() => instance1 = NullArguments.Instance);
            var task2 = Task.Run(() => instance2 = NullArguments.Instance);

            Task.WaitAll(task1, task2);

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void NullArguments_Constructor_ShouldCreateNewInstance()
        {
            // Act
            var instance1 = new NullArguments();
            var instance2 = new NullArguments();

            // Assert
            Assert.AreNotSame(instance1, instance2);
            Assert.AreNotSame(instance1, NullArguments.Instance);
            Assert.AreNotSame(instance2, NullArguments.Instance);
        }

        [Test]
        public void NullArguments_Instance_ShouldBeReadonly()
        {
            // This test verifies that the Instance field is readonly by checking
            // that reflection shows it as readonly
            var field = typeof(NullArguments).GetField("Instance");
            Assert.IsTrue(field.IsInitOnly, "Instance field should be readonly");
        }
    }
}