using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Test
{
    public class Helper_Strings_FormatAsLog
    {
        [TestCase("Hello from {Name}", new object[] { "Jens" }, "Hello from Jens")]
        [TestCase("{0}", new object[] { 56 }, "56")]
        [TestCase("{{{0},{1},{2}}}", new object[] { 1, 2, 3 }, "{1,2,3}")]
        [TestCase("{A}|{b}|{a}", new object[] { 4, 2 }, "4|2|4")]
        public void GeneratesCorrectString(string template, object[] parameters, string expected)
        {
            // Arrange

            // Act
            var actual = Helper.Strings.FormatAsLog(template, parameters);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
