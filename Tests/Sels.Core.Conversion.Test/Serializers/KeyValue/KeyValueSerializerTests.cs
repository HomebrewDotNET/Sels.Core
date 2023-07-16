using Sels.Core.Conversion.Serializers.KeyValue;
using Sels.Core.Extensions.Text;
using Sels.Core.Testing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Test.Serializers.KeyValue
{
    public class KeyValueSerializerTests
    {
        [Test]
        public void KeyValueSerializer_Serialize_SerializesObjectToKeyValuePairs()
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
            Assert.IsNotNull(pairs);
            Assert.IsTrue(pairs.Contains(source.Name));
            Assert.IsTrue(pairs.Contains(source.FamilyName));
            Assert.IsTrue(pairs.Contains(source.ExecutionDate.ToString()));
            Assert.IsTrue(pairs.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.IsTrue(pairs.Contains(source.Score.ToString()));
            Assert.IsTrue(pairs.Contains(source.Course.ToString()));
            source.Signatures.Execute(x => Assert.IsTrue(pairs.Contains(x)));
        }
        [TestCase(":", "   ")]
        [TestCase("=", " | ")]
        [TestCase("=>", "\t")]
        [TestCase(">", " ; ")]
        [TestCase("-", " ! ")]
        public void KeyValueSerializer_Serialize_SerializesObjectToKeyValuePairsUsingCorrectCustomString(string keyValueSeparator, string rowSeparator)
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
            Assert.IsNotNull(pairs);
            Assert.IsTrue(pairs.Contains(keyValueSeparator));
            Assert.IsTrue(pairs.Contains(rowSeparator));
            Assert.IsTrue(pairs.Contains(source.Name));
            Assert.IsTrue(pairs.Contains(source.FamilyName));
            Assert.IsTrue(pairs.Contains(source.ExecutionDate.ToString()));
            Assert.IsTrue(pairs.Contains(source.ResultDate?.ToString() ?? String.Empty));
            Assert.IsTrue(pairs.Contains(source.Score.ToString()));
            Assert.IsTrue(pairs.Contains(source.Course));
            source.Signatures.Execute(x => Assert.IsTrue(pairs.Contains(x)));
        }
        [TestCase("Jens", "Sels", "Linux", new string[] { "Some signature", "Some other signature" })]
        [TestCase("Foo", "Bar", "C#", new string[] { "Prof A", "Prof B", "Prof C" })]
        [TestCase("Jane", "Dou", "English", new string[] { "Prof G", "Prof HB", "Prof V", "Prof VW", "Prof BM" })]
        public void KeyValueSerializer_Deserialize_DeserializesStringToObject(string name, string familyName, string course, string[] signatures)
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
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(familyName, result.FamilyName);
            Assert.AreEqual(executionDate, result.ExecutionDate);
            Assert.AreEqual(resultDate, result.ResultDate);
            Assert.AreEqual(score, result.Score);
            Assert.AreEqual(course, result.Course);
            CollectionAssert.AreEqual(signatures, result.Signatures);
        }
        [TestCase(":", "   ")]
        [TestCase("=", " | ")]
        [TestCase("=>", "\t")]
        [TestCase(">", " ; ")]
        [TestCase("-", " ! ")]
        public void KeyValueSerializer_Deserialize_DeserializesStringToObjectUsingCorrectCustomString(string keyValueSeparator, string rowSeparator)
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
            Assert.IsNotNull(result);
            Assert.IsTrue(source.Contains(result.Name));
            Assert.IsTrue(source.Contains(result.FamilyName));
            Assert.IsTrue(source.Contains(result.Score.ToString()));
            Assert.IsTrue(source.Contains(result.Course));
            result.Signatures.Execute(x => Assert.IsTrue(source.Contains(x)));
        }
        [TestCase(":", "   ")]
        [TestCase("=", " | ")]
        [TestCase("=>", "\t")]
        [TestCase(">", " ; ")]
        [TestCase("-", " ! ")]
        public void KeyValueSerializer_Serialize_SerializingToKeyValuePaisAndDeserializingBackToObjectProducesSameResult(string keyValueSeparator, string rowSeparator)
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
            Assert.IsNotNull(pairs);
            Assert.IsNotNull(result);
            Assert.AreEqual(source.Name, result.Name);
            Assert.AreEqual(source.FamilyName, result.FamilyName);
            Assert.AreEqual(source.ExecutionDate, result.ExecutionDate);
            Assert.AreEqual(source.ResultDate, result.ResultDate);
            Assert.AreEqual(source.Score, result.Score);
            Assert.AreEqual(source.Course, result.Course);
            CollectionAssert.AreEqual(source.Signatures, result.Signatures);
        }
    }
}
