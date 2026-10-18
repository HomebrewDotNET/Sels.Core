using System.Reflection;
using Sels.Core.Attributes;
using Sels.Core.Entity;

namespace Sels.Core.Tests
{
    /// <summary>Contract tests for runtime expression helpers.</summary>
    [Trait("Category", "Unit")]
    public class CoreHelperTests
    {
        /// <summary>Generated hash combiners are stable for equal included properties and ignored values.</summary>
        [Fact]
        public void GenerateHashCodeCombiner_EqualAndSkippedProperties_PreservesHash()
        {
            var combine = CoreHelper.Expressions.GenerateHashCodeCombiner<HashModel>(false);
            var skipCombine = CoreHelper.Expressions.GenerateHashCodeCombiner<HashModel>(false, nameof(HashModel.Skipped));
            var first = new HashModel { Value = 12, Skipped = "left" };
            var same = new HashModel { Value = 12, Skipped = "left" };
            var skippedDifferent = new HashModel { Value = 12, Skipped = "right" };

            var firstHash = combine(first);
            var sameHash = combine(same);
            var skippedFirstHash = skipCombine(first);
            var skippedSecondHash = skipCombine(skippedDifferent);

            Assert.Equal(firstHash, sameHash);
            Assert.Equal(skippedFirstHash, skippedSecondHash);
        }

        /// <summary>Equivalent properties contribute their controlled bucket to the hash.</summary>
        [Fact]
        public void GenerateHashCodeCombiner_EquivalentProperty_ProducesMatchingHash()
        {
            var combine = CoreHelper.Expressions.GenerateHashCodeCombiner<HashModel>(true);
            var first = new HashModel { Child = new HashKey(19, "same") };
            var second = new HashModel { Child = new HashKey(19, "same") };

            var firstHash = combine(first);
            var secondHash = combine(second);

            Assert.Equal(firstHash, secondHash);
        }

        /// <summary>Generic and runtime type overloads agree and validate required inputs.</summary>
        [Fact]
        public void GenerateHashCodeCombiner_OverloadsAndNullArguments_PreserveContract()
        {
            var generic = CoreHelper.Expressions.GenerateHashCodeCombiner<HashModel>();
            var runtime = CoreHelper.Expressions.GenerateHashCodeCombiner(typeof(HashModel));
            var value = new HashModel { Value = 7 };

            var genericHash = generic(value);
            var runtimeHash = runtime(value);
            var nullTypeException = Assert.Throws<ArgumentNullException>(() => CoreHelper.Expressions.GenerateHashCodeCombiner(null!));
            var nullSkipException = Assert.Throws<ArgumentNullException>(() => CoreHelper.Expressions.GenerateHashCodeCombiner(typeof(HashModel), false, null!));

            Assert.Equal(genericHash, runtimeHash);
            Assert.Equal("type", nullTypeException.ParamName);
            Assert.Equal("skipPropertyNames", nullSkipException.ParamName);
        }

        /// <summary>CoreHelper is a static root with no public instance constructor.</summary>
        [Fact]
        public void CoreHelper_TypeMetadata_IsStatic()
        {
            var type = typeof(CoreHelper);

            var publicConstructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            Assert.True(type.IsAbstract);
            Assert.True(type.IsSealed);
            Assert.Empty(publicConstructors);
        }

        /// <summary>CoreConstants retains the configuration and expression names used by runtime helpers.</summary>
        [Fact]
        public void CoreConstants_PublicGroups_ExposeExistingValues()
        {
            var environmentKey = CoreConstants.Configuration.DotNetEnvironmentKey;
            var inputName = CoreConstants.Expressions.InputParameterName;

            Assert.Equal("DOTNET_ENVIRONMENT", environmentKey);
            Assert.Equal("input", inputName);
            Assert.Equal("ASPNETCORE_ENVIRONMENT", CoreConstants.Configuration.AspNetCoreEnvironmentKey);
            Assert.Equal("appsettings.json", CoreConstants.Configuration.DefaultSettingsFileName);
            Assert.Equal("appsettings.{0}.json", CoreConstants.Configuration.EnvironmentSettingsFileNameFormat);
            Assert.Equal("Startup configuration has already been set.", CoreConstants.Configuration.AlreadyInitializedMessage);
            Assert.Equal("Production", CoreConstants.Configuration.DefaultEnvironmentName);
            Assert.Equal("castInput", CoreConstants.Expressions.CastInputVariableName);
            Assert.Equal("hashCode", CoreConstants.Expressions.HashCodeVariableName);
        }

        private sealed class HashModel
        {
            public int Value { get; set; }

            [Ignored]
            public string? Skipped { get; set; }

            public IDeduplicatable? Child { get; set; }
        }

        private sealed class HashKey : IDeduplicatable
        {
            private readonly string _value;

            public HashKey(int bucket, string value)
            {
                Bucket = bucket;
                _value = value;
            }

            public int Bucket { get; }

            public bool IsEquivalentTo(IDeduplicatable other) => other is HashKey key && key._value == _value;

            public int GetBucket() => Bucket;
        }
    }
}