using Sels.ObjectValidationFramework.Profile;

namespace Sels.ObjectValidationFramework.IntegrationTests.Extensions.Configuration
{
    /// <summary>Executes string validation extensions through real validation profiles.</summary>
    [Trait("Category", "Integration")]
    public class StringValidationExtensionsIntegrationTests : ValidationProfileProbe
    {
        /// <summary>String whitespace rule rejects null, empty, and whitespace values with the contract message.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t")]
        public void CannotBeNullOrWhitespace_InvalidValue_ReturnsContractError(string? value)
        {
            var profile = new StringProbeProfile();

            var result = profile.Validate(new StringProbe { Value = value });

            Assert.Single(result.Errors);
            Assert.Equal("Cannot be null, empty or whitespace", result.Errors[0].Message);
        }

        /// <summary>Non-whitespace string values pass the real validation profile.</summary>
        [Fact]
        public void CannotBeNullOrWhitespace_ValidValue_ReturnsNoErrors()
        {
            var profile = new StringProbeProfile();

            var result = profile.Validate(new StringProbe { Value = "ready" });

            Assert.Empty(result.Errors);
        }

        private sealed class StringProbe
        {
            public string? Value { get; set; }
        }

        private sealed class StringProbeProfile : ValidationProfile<string>
        {
            public StringProbeProfile()
            {
                CreateValidationFor<StringProbe>()
                    .ForProperty(x => x.Value)
                    .CannotBeNullOrWhitespace();
            }
        }
    }

    /// <summary>Provides integration-base configuration for validation tests.</summary>
    public abstract class ValidationProfileProbe : Sels.Core.Testing.Templates.BaseIntegrationTest<Sels.Core.Testing.IntegrationTestingSettings>
    {
    }
}