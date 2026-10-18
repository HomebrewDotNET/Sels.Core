using NSubstitute;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Tests.Contracts;

namespace Sels.Core.Conversion.Tests.Converters
{
    /// <summary>Contract tests for <see cref="GenericConverter"/>.</summary>
    [Trait("Category", "Unit")]
    public class GenericConverterTests : ITypeConverterTests
    {
        /// <summary>Verifies CanConvert for sub converter registered: queries sub converter.</summary>
        [Theory]
        [InlineData(4, typeof(string))]
        [InlineData("Hello", typeof(char[]))]
        [InlineData(StringComparison.OrdinalIgnoreCase, typeof(char[]))]
        [InlineData(5.6, typeof(int))]
        [InlineData(new string[] { "Hello again!" }, typeof(string))]
        public void CanConvert_SubConverterRegistered_QueriesSubConverter<T>(T source, Type targetType)
        {
            // Arrange
            var subConverter = Substitute.For<ITypeConverter>();
            var converter = new GenericConverter().AddConverter(subConverter);

            // Act
            converter.CanConvert(source, targetType);

            // Assert
            _ = subConverter.Received().CanConvert(source, targetType, null);
        }

        /// <summary>Verifies ConvertTo for sub converter can convert: uses sub converter.</summary>
        [Theory]
        [InlineData(4, typeof(string))]
        [InlineData("Hello", typeof(char[]))]
        [InlineData(StringComparison.OrdinalIgnoreCase, typeof(char[]))]
        [InlineData(5.6, typeof(int))]
        [InlineData(new string[] { "Hello again!" }, typeof(string))]
        public void ConvertTo_SubConverterCanConvert_UsesSubConverter<T>(T source, Type targetType)
        {
            // Arrange
            var subConverter = Substitute.For<ITypeConverter>();
            subConverter.CanConvert(source, targetType, null).Returns(true);
            var converter = new GenericConverter().AddConverter(subConverter);

            // Act
            converter.ConvertTo(source, targetType);

            // Assert
            _ = subConverter.Received().ConvertTo(source, targetType, null);
        }

        /// <summary>Verifies ConvertTo for ignore unconvertable and sub converter throws: does not throw.</summary>
        [Fact]
        public void ConvertTo_IgnoreUnconvertableAndSubConverterThrows_DoesNotThrow()
        {
            // Arrange
            var subConverter = Substitute.For<ITypeConverter>();
            subConverter.CanConvert(Arg.Any<object>(), Arg.Any<Type>(), Arg.Is<IReadOnlyDictionary<string, object>>(v => v == null)).Returns(true);
            subConverter.ConvertTo(Arg.Any<object>(), Arg.Any<Type>(), Arg.Is<IReadOnlyDictionary<string, object>>(v => v == null)).Returns(_ => throw new Exception());
            var converter = new GenericConverter(GenericConverterSettings.IgnoreUnconvertable).AddConverter(subConverter);
            bool result = false;

            // Act
            try
            {
                converter.ConvertTo(new { }, typeof(string));
            }
            catch (Exception)
            {
                result = true;
            }

            // Assert
            Assert.False(result);
        }

        /// <summary>Verifies ConvertTo for same target type: returns same value without sub converter.</summary>
        [Fact]
        public void ConvertTo_SameTargetType_ReturnsSameValueWithoutSubConverter()
        {
            // Arrange
            var subConverter = Substitute.For<ITypeConverter>();
            var converter = new GenericConverter().AddConverter(subConverter);
            var source = string.Empty;

            // Act
            var result = converter.ConvertTo(source, typeof(string));

            // Assert
            _ = subConverter.DidNotReceive().CanConvert(Arg.Any<object>(), Arg.Any<Type>(), Arg.Is<IReadOnlyDictionary<string, object>>(v => v == null));
            _ = subConverter.DidNotReceive().ConvertTo(Arg.Any<object>(), Arg.Any<Type>(), Arg.Is<IReadOnlyDictionary<string, object>>(v => v == null));
            Assert.Equal(source, result);
        }

        /// <summary>Verifies ConvertTo for always attempt conversion and same target type: uses sub converter.</summary>
        [Fact]
        public void ConvertTo_AlwaysAttemptConversionAndSameTargetType_UsesSubConverter()
        {
            // Arrange
            var subConverter = Substitute.For<ITypeConverter>();
            subConverter.CanConvert(Arg.Any<object>(), Arg.Any<Type>(), Arg.Is<IReadOnlyDictionary<string, object>>(v => v == null)).Returns(true);
            var converter = new GenericConverter(GenericConverterSettings.AlwaysAttemptConversion).AddConverter(subConverter);

            // Act
            converter.ConvertTo(string.Empty, typeof(string));

            // Assert
            _ = subConverter.Received().ConvertTo(Arg.Any<object>(), Arg.Any<Type>(), Arg.Is<IReadOnlyDictionary<string, object>>(v => v == null));
        }        

        /// <inheritdoc/>
        protected override ITypeConverter GetTestInstance()
        {
            return new GenericConverter();
        }
    }
}
