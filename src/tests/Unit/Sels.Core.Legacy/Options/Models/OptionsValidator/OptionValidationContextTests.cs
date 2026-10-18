using Sels.Core.Options;

namespace Sels.Core.Legacy.Tests.Options.Models.OptionsValidator
{
    /// <summary>Contract tests for named options validation context.</summary>
    [Trait("Category", "Unit")]
    public class OptionValidationContextTests
    {
        /// <summary>Null and empty option names identify default options.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_DefaultName_IsDefault(string? name)
        {
            var context = new OptionValidationContext(name!);

            Assert.Equal(string.Empty, context.Name);
            Assert.True(context.IsDefault);
        }

        /// <summary>A nonempty option name is retained as a named context.</summary>
        [Fact]
        public void Constructor_NamedOption_PreservesName()
        {
            const string name = "scenario";

            var context = new OptionValidationContext(name);

            Assert.Equal(name, context.Name);
            Assert.False(context.IsDefault);
        }
    }
}