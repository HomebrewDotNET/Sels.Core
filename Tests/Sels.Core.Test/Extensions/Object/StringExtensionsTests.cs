using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Test.Extensions.Object
{
    public class StringExtensionsTests
    {
        [TestCase("{Key}={Value}", "{Key}", "Action=Build", new string[] { "{Value}" }, "Action")]
        [TestCase("{Command}::{Action}", "{Action}", "Config::Deploy", new string[] { "{Command}" }, "Deploy")]
        [TestCase("Hello from {0}!", "{0}", "Hello from Jens Sels!", null, "Jens Sels")]
        [TestCase("[{Index}]", "{0}", "[2]", null, null)]
        public void StringExtensions_ExtractFromFormat_CorrectValueIsReturned(string format, string parameter, string value, string[] otherParameters, string expected)
        {
            // Arrange

            // Act
            var actual = format.ExtractFromFormat(parameter, value, otherParameters);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
