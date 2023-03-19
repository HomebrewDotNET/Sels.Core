using Sels.Core.Conversion.Serializers.Table;
using Sels.Core.Testing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Test.Serializers.Table
{
    public class TableSerializerTests
    {
        [Test]
        public void TableSerializer_Serialize_SerializesObjectToString()
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
            Assert.IsNotNull(table);
            Assert.IsTrue(table.Contains(source.Name));
            Assert.IsTrue(table.Contains(source.FamilyName));
            Assert.IsTrue(table.Contains(source.ExecutionDate.ToString()));
            Assert.IsTrue(table.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.IsTrue(table.Contains(source.Score.ToString()));
            Assert.IsTrue(table.Contains(source.Course.ToString()));
            source.Signatures.Execute(x => Assert.IsTrue(table.Contains(x)));
        }
        [TestCase("\\", "   ")]
        [TestCase("|", "-------")]
        [TestCase("<>", "\t")]
        [TestCase(";", "=======")]
        [TestCase(":::", "<<<<>>>>")]
        public void TableSerializer_Serialize_SerializesObjectToStringUsingCorrectCustomString(string columnSeparator, string rowSeparator)
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
            Assert.IsNotNull(table);
            Assert.IsTrue(table.Contains(columnSeparator));
            Assert.IsTrue(table.Contains(rowSeparator));
            Assert.IsTrue(table.Contains(source.Name));
            Assert.IsTrue(table.Contains(source.FamilyName));
            Assert.IsTrue(table.Contains(source.ExecutionDate.ToString()));
            Assert.IsTrue(table.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.IsTrue(table.Contains(source.Score.ToString()));
            Assert.IsTrue(table.Contains(source.Course.ToString()));
            source.Signatures.Execute(x => Assert.IsTrue(table.Contains(x)));
        }
        [Test]
        public void TableSerializer_Serialize_SerializesArrayToString()
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
            Assert.IsNotNull(table);
            foreach(var source in sources)
            {
                Assert.IsTrue(table.Contains(source.Name));
                Assert.IsTrue(table.Contains(source.FamilyName));
                Assert.IsTrue(table.Contains(source.ExecutionDate.ToString()));
                Assert.IsTrue(table.Contains(source.ResultDate?.ToString() ?? String.Empty));
                Assert.IsTrue(table.Contains(source.Score.ToString()));
                Assert.IsTrue(table.Contains(source.Course.ToString()));
                source.Signatures.Execute(x => Assert.IsTrue(table.Contains(x)));
            }
        }
        [TestCase("Jens", "Sels", "Linux", new string[] { "Some signature", "Some other signature" })]
        [TestCase("Foo", "Bar", "C#", new string[] { "Prof A", "Prof B", "Prof C" })]
        [TestCase("Jane", "Dou", "English", new string[] { "Prof G", "Prof HB", "Prof V", "Prof VW", "Prof BM" })]
        public void TableSerializer_Deserialize_DeserializesStringToObject(string name, string familyName, string course, string[] signatures)
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
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(familyName, result.FamilyName);
            Assert.AreEqual(executionDate, result.ExecutionDate);
            Assert.AreEqual(resultDate, result.ResultDate);
            Assert.AreEqual(score, result.Score);
            Assert.AreEqual(course, result.Course);
            CollectionAssert.AreEqual(signatures, result.Signatures);
        }
        [Test]
        public void TableSerializer_Deserialize_DeserializesStringToArray()
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
            Assert.IsNotNull(result);
            Assert.AreEqual(sources.Length, result.Length);
            for(int i = 0; i < result.Length; i++)
            {
                var expected = sources[i];
                var actual = result[i];
                Assert.AreEqual(expected.Name, actual.Name);
                Assert.AreEqual(expected.FamilyName, actual.FamilyName);
                Assert.AreEqual(expected.ExecutionDate, actual.ExecutionDate);
                Assert.AreEqual(expected.ResultDate, actual.ResultDate);
                Assert.AreEqual(expected.Score, actual.Score);
                Assert.AreEqual(expected.Course, actual.Course);
                CollectionAssert.AreEqual(expected.Signatures, actual.Signatures);
            }            
        }
        [TestCase("\\", "   ")]
        [TestCase("|", "-------")]
        [TestCase("<>", "\t")]
        [TestCase(";", "=======")]
        [TestCase(":::", "<<<<>>>>")]
        public void TableSerializer_Deserialize_DeserializesStringToObjectUsingCorrectCustomString(string columnSeparator, string rowSeparator)
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
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Name, result.Name);
            Assert.AreEqual(expected.FamilyName, result.FamilyName);
            Assert.AreEqual(expected.ExecutionDate, result.ExecutionDate);
            Assert.AreEqual(expected.ResultDate, result.ResultDate);
            Assert.AreEqual(expected.Score, result.Score);
            Assert.AreEqual(expected.Course, result.Course);
            CollectionAssert.AreEqual(expected.Signatures, result.Signatures);
        }
        [Test]
        public void TableSerializer_Serialize_SerializingArrayAndThenDeserializingToArrayProducesSameResult()
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
            Assert.IsNotNull(table);
            Assert.IsNotNull(result);
            Assert.AreEqual(sources.Length, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
                var expected = sources[i];
                var actual = result[i];
                Assert.AreEqual(expected.Name, actual.Name);
                Assert.AreEqual(expected.FamilyName, actual.FamilyName);
                Assert.AreEqual(expected.ExecutionDate, actual.ExecutionDate);
                Assert.AreEqual(expected.ResultDate, actual.ResultDate);
                Assert.AreEqual(expected.Score, actual.Score);
                Assert.AreEqual(expected.Course, actual.Course);
                CollectionAssert.AreEqual(expected.Signatures, actual.Signatures);
            }
        }
    }
}
