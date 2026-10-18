using Sels.Core.Conversion.Serializers.Table;
using Sels.Core.Extensions.Text;
using Sels.Core.Testing.Models;
using System.Text;

namespace Sels.Core.Conversion.Tests.Serializers.Table
{
    /// <summary>Contract tests for <see cref="TableSerializer"/>.</summary>
    [Trait("Category", "Unit")]
    public class TableSerializerTests
    {
        /// <summary>Verifies Serialize for object: returns table string.</summary>
        [Fact]
        public void Serialize_Object_ReturnsTableString()
        {
            // Arrange
            var serializer = new TableSerializer();
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
            var table = serializer.Serialize(source);

            // Assert
            Assert.NotNull(table);
            Assert.True(table.Contains(source.Name));
            Assert.True(table.Contains(source.FamilyName));
            Assert.True(table.Contains(source.ExecutionDate.ToString()));
            Assert.True(table.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.True(table.Contains(source.Score.ToString()));
            Assert.True(table.Contains(source.Course.ToString()));
            source.Signatures.Execute(x => Assert.True(table.Contains(x)));
        }

        /// <summary>Verifies Serialize for custom strings: uses custom strings.</summary>
        [Theory]
        [InlineData("\\", "   ")]
        [InlineData("|", "-------")]
        [InlineData("<>", "\t")]
        [InlineData(";", "=======")]
        [InlineData(":::", "<<<<>>>>")]
        public void Serialize_CustomStrings_UsesCustomStrings(string columnSeparator, string rowSeparator)
        {
            // Arrange
            var serializer = new TableSerializer(x => x.SplitAndJoinColumnsUsing(columnSeparator).SplitAndJoinRowsUsing(rowSeparator));
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
            var table = serializer.Serialize(source);

            // Assert
            Assert.NotNull(table);
            Assert.True(table.Contains(columnSeparator));
            Assert.True(table.Contains(rowSeparator));
            Assert.True(table.Contains(source.Name));
            Assert.True(table.Contains(source.FamilyName));
            Assert.True(table.Contains(source.ExecutionDate.ToString()));
            Assert.True(table.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.True(table.Contains(source.Score.ToString()));
            Assert.True(table.Contains(source.Course.ToString()));
            source.Signatures.Execute(x => Assert.True(table.Contains(x)));
        }

        /// <summary>Verifies Serialize for array: returns table string.</summary>
        [Fact]
        public void Serialize_Array_ReturnsTableString()
        {
            // Arrange
            var serializer = new TableSerializer();
            var sources = new ExamResult[]
            { 
                new ExamResult(){
                    Name = "Jens",
                    FamilyName = "Sels",
                    ExecutionDate = DateTime.Now.AddDays(-30),
                    ResultDate = DateTime.Now,
                    Score = 76,
                    Course = "C#",
                    Signatures = new List<string>() { "Prof X", "Prof M" }
                },
                new ExamResult(){
                    Name = "Jane",
                    FamilyName = "Dou",
                    ExecutionDate = DateTime.Now.AddDays(-30),
                    ResultDate = DateTime.Now,
                    Score = 89,
                    Course = "English",
                    Signatures = new List<string>() { "Prof A", "Prof B" }
                }
            };

            // Act
            var table = serializer.Serialize(sources);

            // Assert
            Assert.NotNull(table);
            foreach(var source in sources)
            {
                Assert.True(table.Contains(source.Name));
                Assert.True(table.Contains(source.FamilyName));
                Assert.True(table.Contains(source.ExecutionDate.ToString()));
                Assert.True(table.Contains(source.ResultDate?.ToString() ?? String.Empty));
                Assert.True(table.Contains(source.Score.ToString()));
                Assert.True(table.Contains(source.Course.ToString()));
                source.Signatures.Execute(x => Assert.True(table.Contains(x)));
            }
        }

        /// <summary>Verifies Deserialize for table string: returns object.</summary>
        [Theory]
        [InlineData("Jens", "Sels", "Linux", new string[] { "Some signature", "Some other signature" })]
        [InlineData("Foo", "Bar", "C#", new string[] { "Prof A", "Prof B", "Prof C" })]
        [InlineData("Jane", "Dou", "English", new string[] { "Prof G", "Prof HB", "Prof V", "Prof VW", "Prof BM" })]
        public void Deserialize_TableString_ReturnsObject(string name, string familyName, string course, string[] signatures)
        {
            // Arrange
            var serializer = new TableSerializer();
            var executionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-5);
            var resultDate = DateTime.Parse("04/01/1998 00:13:45");
            var score = Helper.Random.GetRandomDouble(0, 100);
            var builder = new StringBuilder()
                                .Append(nameof(ExamResult.Name)).Append(';')
                                .Append(nameof(ExamResult.FamilyName)).Append(';')
                                .Append(nameof(ExamResult.ExecutionDate)).Append(';')
                                .Append(nameof(ExamResult.ResultDate)).Append(';')
                                .Append(nameof(ExamResult.Score)).Append(';')
                                .Append(nameof(ExamResult.Course)).Append(';')
                                .AppendLine(nameof(ExamResult.Signatures))
                                .Append(name).Append(';')
                                .Append(familyName).Append(';')
                                .Append(executionDate).Append(';')
                                .Append(resultDate).Append(';')
                                .Append(score).Append(';')
                                .Append(course).Append(';')
                                .AppendLine(signatures.JoinString(','));   
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

        /// <summary>Verifies Deserialize for table string: returns array.</summary>
        [Fact]
        public void Deserialize_TableString_ReturnsArray()
        {
            // Arrange
            var serializer = new TableSerializer();
            var sources = new ExamResult[]
            {
                new ExamResult(){
                    Name = "Jens",
                    FamilyName = "Sels",
                    ExecutionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-30),
                    ResultDate = DateTime.Parse("04/01/1998 00:13:45"),
                    Score = 76,
                    Course = "C#",
                    Signatures = new List<string>() { "Prof X", "Prof M" }
                },
                new ExamResult(){
                    Name = "Jane",
                    FamilyName = "Dou",
                    ExecutionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-30),
                    ResultDate = DateTime.Parse("04/01/1998 00:13:45"),
                    Score = 89,
                    Course = "English",
                    Signatures = new List<string>() { "Prof A", "Prof B" }
                }
            };
            var builder = new StringBuilder()
                                .Append(nameof(ExamResult.Name)).Append(';')
                                .Append(nameof(ExamResult.FamilyName)).Append(';')
                                .Append(nameof(ExamResult.ExecutionDate)).Append(';')
                                .Append(nameof(ExamResult.ResultDate)).Append(';')
                                .Append(nameof(ExamResult.Score)).Append(';')
                                .Append(nameof(ExamResult.Course)).Append(';')
                                .AppendLine(nameof(ExamResult.Signatures));
            sources.Execute(x =>
            {
                builder.Append(x.Name).Append(';')
                       .Append(x.FamilyName).Append(';')
                       .Append(x.ExecutionDate).Append(';')
                       .Append(x.ResultDate).Append(';')
                       .Append(x.Score).Append(';')
                       .Append(x.Course).Append(';')
                       .AppendLine(x.Signatures.JoinString(','));
            });
            var source = builder.ToString();

            // Act
            var result = serializer.Deserialize<ExamResult[]>(source);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sources.Length, result.Length);
            for(int i = 0; i < result.Length; i++)
            {
                var expected = sources[i];
                var actual = result[i];
                Assert.Equal(expected.Name, actual.Name);
                Assert.Equal(expected.FamilyName, actual.FamilyName);
                Assert.Equal(expected.ExecutionDate, actual.ExecutionDate);
                Assert.Equal(expected.ResultDate, actual.ResultDate);
                Assert.Equal(expected.Score, actual.Score);
                Assert.Equal(expected.Course, actual.Course);
                Assert.Equal(expected.Signatures, actual.Signatures);
            }            
        }

        /// <summary>Verifies Deserialize for custom strings: uses custom strings.</summary>
        [Theory]
        [InlineData("\\", "   ")]
        [InlineData("|", "-------")]
        [InlineData("<>", "\t")]
        [InlineData(";", "=======")]
        [InlineData(":::", "<<<<>>>>")]
        public void Deserialize_CustomStrings_UsesCustomStrings(string columnSeparator, string rowSeparator)
        {
            // Arrange
            var serializer = new TableSerializer(x => x.SplitAndJoinColumnsUsing(columnSeparator).SplitAndJoinRowsUsing(rowSeparator));
            var expected = new ExamResult()
            {
                Name = "Jens",
                FamilyName = "Sels",
                ExecutionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-30),
                ResultDate = DateTime.Parse("04/01/1998 00:13:45"),
                Score = 90,
                Course = "C#",
                Signatures = new List<string>() { "Prof X", "Prof M" }
            };
            var source = new string[] {
                new string[]{nameof(ExamResult.Name), nameof(ExamResult.FamilyName), nameof(ExamResult.ExecutionDate), nameof(ExamResult.ResultDate), nameof(ExamResult.Score), nameof(ExamResult.Course), nameof(ExamResult.Signatures)}.JoinString(columnSeparator),
                new string[]{expected.Name, expected.FamilyName, expected.ExecutionDate.ToString(), expected.ResultDate?.ToString() ?? string.Empty, expected.Score.ToString(), expected.Course, expected.Signatures.JoinString(",")}.JoinString(columnSeparator),
            }.JoinString(rowSeparator);

            // Act
            var result = serializer.Deserialize<ExamResult>(source);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.FamilyName, result.FamilyName);
            Assert.Equal(expected.ExecutionDate, result.ExecutionDate);
            Assert.Equal(expected.ResultDate, result.ResultDate);
            Assert.Equal(expected.Score, result.Score);
            Assert.Equal(expected.Course, result.Course);
            Assert.Equal(expected.Signatures, result.Signatures);
        }

        /// <summary>Verifies Serialize for array round trip through deserialize: returns equivalent array.</summary>
        [Fact]
        public void Serialize_ArrayRoundTripThroughDeserialize_ReturnsEquivalentArray()
        {
            // Arrange
            var serializer = new TableSerializer();
            var sources = new ExamResult[]
            {
                new ExamResult(){
                    Name = "Jens",
                    FamilyName = "Sels",
                    ExecutionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-30),
                    ResultDate = DateTime.Parse("04/01/1998 00:13:45"),
                    Score = 76,
                    Course = "C#",
                    Signatures = new List<string>() { "Prof X", "Prof M" }
                },
                new ExamResult(){
                    Name = "Jane",
                    FamilyName = "Dou",
                    ExecutionDate = DateTime.Parse("04/01/1998 00:13:45").AddDays(-30),
                    ResultDate = DateTime.Parse("04/01/1998 00:13:45"),
                    Score = 89,
                    Course = "English",
                    Signatures = new List<string>() { "Prof A", "Prof B" }
                }
            };

            // Act
            var table = serializer.Serialize(sources);
            var result = serializer.Deserialize<ExamResult[]>(table);

            // Assert
            Assert.NotNull(table);
            Assert.NotNull(result);
            Assert.Equal(sources.Length, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
                var expected = sources[i];
                var actual = result[i];
                Assert.Equal(expected.Name, actual.Name);
                Assert.Equal(expected.FamilyName, actual.FamilyName);
                Assert.Equal(expected.ExecutionDate, actual.ExecutionDate);
                Assert.Equal(expected.ResultDate, actual.ResultDate);
                Assert.Equal(expected.Score, actual.Score);
                Assert.Equal(expected.Course, actual.Course);
                Assert.Equal(expected.Signatures, actual.Signatures);
            }
        }
    }
}
