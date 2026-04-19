using System;
using System.Globalization;
using System.Windows;
using FluentAssertions;
using MetaSkillStudio.Converters;
using Xunit;

namespace MetaSkillStudio.Tests.Converters
{
    /// <summary>
    /// Unit tests for WPF Value Converters.
    /// </summary>
    public class ConvertersTests
    {
        #region BoolToVisibilityConverter Tests

        [Fact]
        public void BoolToVisibilityConverter_True_ReturnsVisible()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void BoolToVisibilityConverter_False_ReturnsCollapsed()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void BoolToVisibilityConverter_NonBool_ReturnsCollapsed()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.Convert("not a bool", typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void BoolToVisibilityConverter_Null_ReturnsCollapsed()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void BoolToVisibilityConverter_ConvertBack_Visible_ReturnsTrue()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void BoolToVisibilityConverter_ConvertBack_Collapsed_ReturnsFalse()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void BoolToVisibilityConverter_ConvertBack_Hidden_ReturnsFalse()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(false);
        }

        #endregion

        #region InverseBoolToVisibilityConverter Tests

        [Fact]
        public void InverseBoolToVisibilityConverter_True_ReturnsCollapsed()
        {
            // Arrange
            var converter = new InverseBoolToVisibilityConverter();

            // Act
            var result = converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void InverseBoolToVisibilityConverter_False_ReturnsVisible()
        {
            // Arrange
            var converter = new InverseBoolToVisibilityConverter();

            // Act
            var result = converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void InverseBoolToVisibilityConverter_NonBool_ReturnsVisible()
        {
            // Arrange
            var converter = new InverseBoolToVisibilityConverter();

            // Act
            var result = converter.Convert("not a bool", typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void InverseBoolToVisibilityConverter_Null_ReturnsVisible()
        {
            // Arrange
            var converter = new InverseBoolToVisibilityConverter();

            // Act
            var result = converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void InverseBoolToVisibilityConverter_ConvertBack_Visible_ReturnsFalse()
        {
            // Arrange
            var converter = new InverseBoolToVisibilityConverter();

            // Act
            var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(false);
        }

        [Fact]
        public void InverseBoolToVisibilityConverter_ConvertBack_Collapsed_ReturnsTrue()
        {
            // Arrange
            var converter = new InverseBoolToVisibilityConverter();

            // Act
            var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void InverseBoolToVisibilityConverter_ConvertBack_Hidden_ReturnsTrue()
        {
            // Arrange
            var converter = new InverseBoolToVisibilityConverter();

            // Act
            var result = converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(true);
        }

        #endregion

        #region StringEmptyToVisibilityConverter Tests

        [Fact]
        public void StringEmptyToVisibilityConverter_EmptyString_ReturnsCollapsed()
        {
            // Arrange
            var converter = new StringEmptyToVisibilityConverter();

            // Act
            var result = converter.Convert("", typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void StringEmptyToVisibilityConverter_Null_ReturnsCollapsed()
        {
            // Arrange
            var converter = new StringEmptyToVisibilityConverter();

            // Act
            var result = converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void StringEmptyToVisibilityConverter_Whitespace_ReturnsCollapsed()
        {
            // Arrange
            var converter = new StringEmptyToVisibilityConverter();

            // Act
            var result = converter.Convert("   ", typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void StringEmptyToVisibilityConverter_NonEmptyString_ReturnsVisible()
        {
            // Arrange
            var converter = new StringEmptyToVisibilityConverter();

            // Act
            var result = converter.Convert("Hello", typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Visible);
        }

        [Fact]
        public void StringEmptyToVisibilityConverter_NonString_ReturnsCollapsed()
        {
            // Arrange
            var converter = new StringEmptyToVisibilityConverter();

            // Act
            var result = converter.Convert(123, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [Fact]
        public void StringEmptyToVisibilityConverter_ConvertBack_ThrowsNotImplemented()
        {
            // Arrange
            var converter = new StringEmptyToVisibilityConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                converter.ConvertBack(Visibility.Visible, typeof(string), null, CultureInfo.InvariantCulture));
        }

        #endregion

        #region Converter Integration Tests

        [Theory]
        [InlineData(true, false, "", Visibility.Visible, Visibility.Visible, Visibility.Collapsed)]
        [InlineData(false, true, "text", Visibility.Collapsed, Visibility.Collapsed, Visibility.Visible)]
        [InlineData(true, true, "", Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed)]
        [InlineData(false, false, "content", Visibility.Collapsed, Visibility.Visible, Visibility.Visible)]
        public void Converters_WorkTogetherForComplexScenarios(
            bool boolValue, 
            bool inverseBoolValue, 
            string stringValue,
            Visibility expectedBoolVisibility,
            Visibility expectedInverseVisibility,
            Visibility expectedStringVisibility)
        {
            // Arrange
            var boolConverter = new BoolToVisibilityConverter();
            var inverseConverter = new InverseBoolToVisibilityConverter();
            var stringConverter = new StringEmptyToVisibilityConverter();

            // Act
            var boolResult = boolConverter.Convert(boolValue, typeof(Visibility), null, CultureInfo.InvariantCulture);
            var inverseResult = inverseConverter.Convert(inverseBoolValue, typeof(Visibility), null, CultureInfo.InvariantCulture);
            var stringResult = stringConverter.Convert(stringValue, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            boolResult.Should().Be(expectedBoolVisibility);
            inverseResult.Should().Be(expectedInverseVisibility);
            stringResult.Should().Be(expectedStringVisibility);
        }

        #endregion
    }
}
