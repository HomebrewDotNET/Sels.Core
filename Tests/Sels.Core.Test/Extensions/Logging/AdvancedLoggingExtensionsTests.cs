using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Sels.Core.Components;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;

namespace Sels.Core.Test.Extensions.Logging
{
    [TestFixture]
    public class AdvancedLoggingExtensionsTests
    {
        [Test]
        public void TraceMethod_WithNullLoggers_ShouldReturnEmptyDisposable()
        {
            // Arrange
            IEnumerable<ILogger> loggers = null;

            // Act
            var result = loggers.TraceMethod(LogLevel.Information, typeof(string), "TestMethod");

            // Assert
            Assert.AreSame(EmptyDisposable.Instance, result);
        }

        [Test]
        public void TraceMethod_WithEmptyLoggers_ShouldReturnEmptyDisposable()
        {
            // Arrange
            var loggers = new List<ILogger>();

            // Act
            var result = loggers.TraceMethod(LogLevel.Information, typeof(string), "TestMethod");

            // Assert
            Assert.AreSame(EmptyDisposable.Instance, result);
        }

        [Test]
        public void TraceMethod_WithNullLogger_ShouldReturnEmptyDisposable()
        {
            // Arrange
            ILogger logger = null;

            // Act
            var result = logger.TraceMethod(LogLevel.Information, typeof(string), "TestMethod");

            // Assert
            Assert.AreSame(EmptyDisposable.Instance, result);
        }

        [Test]
        public void TraceMethod_WithNullCaller_ShouldUseNullString()
        {
            // Arrange
            var loggers = new List<ILogger> { new FakeLogger() };

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                using var result = loggers.TraceMethod(LogLevel.Information, null, "TestMethod");
            });
        }

        [Test]
        public void TraceMethod_WithNullMethod_ShouldUseUnknownString()
        {
            // Arrange
            var loggers = new List<ILogger> { new FakeLogger() };

            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                using var result = loggers.TraceMethod(LogLevel.Information, typeof(string), null);
            });
        }

        private class FakeLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => EmptyDisposable.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }
    }
}