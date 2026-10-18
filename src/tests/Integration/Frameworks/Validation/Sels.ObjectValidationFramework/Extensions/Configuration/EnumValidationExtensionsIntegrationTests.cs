using Sels.ObjectValidationFramework.Profile;

namespace Sels.ObjectValidationFramework.IntegrationTests.Extensions.Configuration
{
    /// <summary>Executes enum validation extensions through a real validation profile.</summary>
    [Trait("Category", "Integration")]
    public class EnumValidationExtensionsIntegrationTests : Sels.Core.Testing.Templates.BaseIntegrationTest<Sels.Core.Testing.IntegrationTestingSettings>
    {
        /// <summary>IsDefined rejects undeclared enum values and accepts declared values.</summary>
        [Fact]
        public void IsDefined_DeclaredAndUndefinedValues_ReturnsExpectedErrors()
        {
            var profile = new EnumProbeProfile();

            var valid = profile.Validate(new EnumProbe { Value = ProbeValue.First });
            var invalid = profile.Validate(new EnumProbe { Value = (ProbeValue)99 });

            Assert.Empty(valid.Errors);
            Assert.Single(invalid.Errors);
            Assert.Contains(nameof(ProbeValue), invalid.Errors[0].Message);
        }

        /// <summary>HasFlag validates required flags while preserving the enum flag contract.</summary>
        [Fact]
        public void HasFlag_RequiredFlagPresentOrMissing_ReturnsExpectedErrors()
        {
            var profile = new FlagProbeProfile();

            var valid = profile.Validate(new FlagProbe { Value = ProbeValue.First | ProbeValue.Second });
            var invalid = profile.Validate(new FlagProbe { Value = ProbeValue.Second });

            Assert.Empty(valid.Errors);
            Assert.Single(invalid.Errors);
            Assert.Equal("Must have the flag First set", invalid.Errors[0].Message);
        }

        private sealed class EnumProbe
        {
            public ProbeValue Value { get; set; }
        }

        private sealed class FlagProbe
        {
            public ProbeValue Value { get; set; }
        }

        [Flags]
        private enum ProbeValue
        {
            None = 0,
            First = 1,
            Second = 2
        }

        private sealed class EnumProbeProfile : ValidationProfile<string>
        {
            public EnumProbeProfile()
            {
                CreateValidationFor<EnumProbe>()
                    .ForProperty(x => x.Value)
                    .IsDefined();
            }
        }

        private sealed class FlagProbeProfile : ValidationProfile<string>
        {
            public FlagProbeProfile()
            {
                CreateValidationFor<FlagProbe>()
                    .ForProperty(x => x.Value)
                    .HasFlag(ProbeValue.First);
            }
        }
    }
}