using Microsoft.Extensions.Options;
using Sels.Core.Options;
using Sels.Core.Validation;

namespace Sels.Core.Legacy.Tests.Components.Options
{
    /// <summary>Contract tests for options validation adaptation.</summary>
    [Trait("Category", "Unit")]
    public class OptionsValidatorTests
    {
        /// <summary>OptionsValidator maps empty and nonempty validator results to framework results.</summary>
        [Fact]
        public void Validate_ValidatorReturnsErrors_ReturnsFailuresInOrder()
        {
            var options = new OptionsProbe();
            var validator = Substitute.For<IValidator<OptionsProbe, string, OptionValidationContext>>();
            validator.Validate(options, Arg.Any<OptionValidationContext>())
                .Returns(call => call.Arg<OptionValidationContext>().IsDefault ? Array.Empty<string>() : new[] { "first", "second" });
            var sut = new OptionsValidator<OptionsProbe>(validator);

            var defaultResult = sut.Validate(null!, options);
            var namedResult = sut.Validate("named", options);

            Assert.True(defaultResult.Succeeded);
            Assert.False(namedResult.Succeeded);
            Assert.Equal(new[] { "first", "second" }, namedResult.Failures);
        }

        /// <summary>OptionsValidator rejects a null options object with its parameter name.</summary>
        [Fact]
        public void Validate_NullOptions_ThrowsArgumentNullException()
        {
            var validator = Substitute.For<IValidator<OptionsProbe, string, OptionValidationContext>>();
            var sut = new OptionsValidator<OptionsProbe>(validator);

            var exception = Assert.Throws<ArgumentNullException>(() => sut.Validate("", null!));

            Assert.Equal("options", exception.ParamName);
        }

        /// <summary>Options value used by validator contract tests.</summary>
        public sealed class OptionsProbe
        {
        }
    }
}