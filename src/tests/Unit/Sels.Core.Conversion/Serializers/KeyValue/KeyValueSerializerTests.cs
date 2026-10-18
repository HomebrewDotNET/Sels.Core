using Sels.Core.Conversion.Serializers.KeyValue;
using Sels.Core.Extensions.Text;
using Sels.Core.Testing.Models;
using System.Text;

namespace Sels.Core.Conversion.Tests.Serializers.KeyValue
{
    /// <summary>Contract tests for <see cref="KeyValueSerializer"/>.</summary>
    [Trait("Category", "Unit")]
    public class KeyValueSerializerTests
    {
        /// <summary>Verifies Serialize for object: returns key value pairs.</summary>
        [Fact]
        public void Serialize_Object_ReturnsKeyValuePairs()
        {
            // Arrange
            var serializer = new KeyValueSerializer();
            var source = new ExamResult()
            {
                Name = "Jens",
                FamilyName = "Sels",
                ExecutionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-30),
                ResultDate = DateTime.Parse("04/01/1998 00:13:45"),
                Score = 90,
                Course = "C#",
                Signatures = new List<string>() { "Prof X", "Prof M" }
            };

            // Act
            var pairs = serializer.Serialize(source);

            // Assert
            Assert.NotNull(pairs);
            Assert.True(pairs.Contains(source.Name));
            Assert.True(pairs.Contains(source.FamilyName));
            Assert.True(pairs.Contains(source.ExecutionDate.ToString()));
            Assert.True(pairs.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.True(pairs.Contains(source.Score.ToString()));
            Assert.True(pairs.Contains(source.Course.ToString()));
            source.Signatures.Execute(x => Assert.True(pairs.Contains(x)));
        }

        /// <summary>Verifies Serialize for custom strings: uses custom strings.</summary>
        [Theory]
        [InlineData(":", "   ")]
        [InlineData("=", " | ")]
        [InlineData("=>", "\t")]
        [InlineData(">", " ; ")]
        [InlineData("-", " ! ")]
        public void Serialize_CustomStrings_UsesCustomStrings(string keyValueSeparator, string rowSeparator)
        {
            // Arrange
            var serializer = new KeyValueSerializer(x => x.ConvertKeyValuePairUsing(keyValueSeparator).SplitAndJoinRowsUsing(rowSeparator));
            var source = new ExamResult()
            {
                Name = "Jens",
                FamilyName = "Sels",
                ExecutionDate = DateTime.Now.AddDays(-30),
                ResultDate = DateTime.Now,
                Score = 90,
                Course = "C#",
                Signatures = new List<string>() { "Prof X", "Prof M" }
            };

            // Act
            var pairs = serializer.Serialize(source);

            // Assert
            Assert.NotNull(pairs);
            Assert.True(pairs.Contains(keyValueSeparator));
            Assert.True(pairs.Contains(rowSeparator));
            Assert.True(pairs.Contains(source.Name));
            Assert.True(pairs.Contains(source.FamilyName));
            Assert.True(pairs.Contains(source.ExecutionDate.ToString()));
            Assert.True(pairs.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.True(pairs.Contains(source.Score.ToString()));
            Assert.True(pairs.Contains(source.Course));
            source.Signatures.Execute(x => Assert.True(pairs.Contains(x)));
        }

        /// <summary>Verifies Deserialize for key value string: returns object.</summary>
        [Theory]
        [InlineData("Jens", "Sels", "Linux", new string[] { "Some signature", "Some other signature" })]
        [InlineData("Foo", "Bar", "C#", new string[] { "Prof A", "Prof B", "Prof C" })]
        [InlineData("Jane", "Dou", "English", new string[] { "Prof G", "Prof HB", "Prof V", "Prof VW", "Prof BM" })]
        public void Deserialize_KeyValueString_ReturnsObject(string name, string familyName, string course, string[] signatures)
        {
            // Arrange
            var serializer = new KeyValueSerializer();
            var executionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-5);
            var resultDate = DateTime.Parse("04/01/1998 00:13:45");
            var score = Helper.Random.GetRandomDouble(0, 100);
            var builder = new StringBuilder()
                                .AppendLine($"{nameof(ExamResult.Name)}:{name}")
                                .AppendLine($"{nameof(ExamResult.FamilyName)}:{familyName}")
                                .AppendLine($"{nameof(ExamResult.ExecutionDate)}:{executionDate}")
                                .AppendLine($"{nameof(ExamResult.ResultDate)}:{resultDate}")
                                .AppendLine($"{nameof(ExamResult.Score)}:{score}")
                                .AppendLine($"{nameof(ExamResult.Course)}:{course}");
            signatures.Execute(x => builder.AppendLine($"{nameof(ExamResult.Signatures)}:{x}"));
            var source = builder.ToString();
                
            // Act
            var result = serializer.Deserialize<ExamResult>(source);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(familyName, result.FamilyName);
            Assert.Equal(executionDate, result.ExecutionDate);
            Assert.Equal(resultDate, result.ResultDate);
            Assert.Equal(score, result.Score);
            Assert.Equal(course, result.Course);
            Assert.Equal(signatures, result.Signatures);
        }

        /// <summary>Verifies Deserialize for custom strings: uses custom strings.</summary>
        [Theory]
        [InlineData(":", "   ")]
        [InlineData("=", " | ")]
        [InlineData("=>", "\t")]
        [InlineData(">", " ; ")]
        [InlineData("-", " ! ")]
        public void Deserialize_CustomStrings_UsesCustomStrings(string keyValueSeparator, string rowSeparator)
        {
            // Arrange
            var serializer = new KeyValueSerializer(x => x.ConvertKeyValuePairUsing(keyValueSeparator).SplitAndJoinRowsUsing(rowSeparator));
            var source = new string[]
            {
                $"{nameof(ExamResult.Name)}{keyValueSeparator}Jens",
                $"{nameof(ExamResult.FamilyName)}{keyValueSeparator}Sels",
                $"{nameof(ExamResult.Score)}{keyValueSeparator}75",
                $"{nameof(ExamResult.Course)}{keyValueSeparator}English",
                $"{nameof(ExamResult.Signatures)}{keyValueSeparator}Prof Q",
                $"{nameof(ExamResult.Signatures)}{keyValueSeparator}Prof L",
                $"{nameof(ExamResult.Signatures)}{keyValueSeparator}Prof Z"
            }.JoinString(rowSeparator);

            // Act
            var result = serializer.Deserialize<ExamResult>(source);

            // Assert
            Assert.NotNull(result);
            Assert.True(source.Contains(result.Name));
            Assert.True(source.Contains(result.FamilyName));
            Assert.True(source.Contains(result.Score.ToString()));
            Assert.True(source.Contains(result.Course));
            result.Signatures.Execute(x => Assert.True(source.Contains(x)));
        }

        /// <summary>Verifies Serialize for round trip through deserialize: returns equivalent object.</summary>
        [Theory]
        [InlineData(":", "   ")]
        [InlineData("=", " | ")]
        [InlineData("=>", "\t")]
        [InlineData(">", " ; ")]
        [InlineData("-", " ! ")]
        public void Serialize_RoundTripThroughDeserialize_ReturnsEquivalentObject(string keyValueSeparator, string rowSeparator)
        {
            // Arrange
            var serializer = new KeyValueSerializer(x => x.ConvertKeyValuePairUsing(keyValueSeparator).SplitAndJoinRowsUsing(rowSeparator));
            var source = new ExamResult()
            {
                Name = "Jens",
                FamilyName = "Sels",
                ExecutionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-30),
                ResultDate = DateTime.Parse("04/01/1998 00:13:45"),
                Score = 90,
                Course = "C#",
                Signatures = new List<string>() { "Prof X", "Prof M" }
            };
            // Act
            var pairs = serializer.Serialize(source);
            var result = serializer.Deserialize<ExamResult>(pairs);

            // Assert
            Assert.NotNull(pairs);
            Assert.NotNull(result);
            Assert.Equal(source.Name, result.Name);
            Assert.Equal(source.FamilyName, result.FamilyName);
            Assert.Equal(source.ExecutionDate, result.ExecutionDate);
            Assert.Equal(source.ResultDate, result.ResultDate);
            Assert.Equal(source.Score, result.Score);
            Assert.Equal(source.Course, result.Course);
            Assert.Equal(source.Signatures, result.Signatures);
        }
    }
}
