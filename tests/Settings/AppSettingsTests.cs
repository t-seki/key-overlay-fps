using NUnit.Framework;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.Tests.Settings
{
    /// <summary>
    /// AppSettingsのテストクラス
    /// </summary>
    [TestFixture]
    public class AppSettingsTests
    {
        [Test]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var settings = new AppSettings();

            // Assert
            Assert.That(settings.WindowLeft, Is.EqualTo(ApplicationConstants.UILayout.DefaultWindowLeft));
            Assert.That(settings.WindowTop, Is.EqualTo(ApplicationConstants.UILayout.DefaultWindowTop));
            Assert.That(settings.IsTopmost, Is.True);
            Assert.That(settings.DisplayScale, Is.EqualTo(1.0));
            Assert.That(settings.IsMouseVisible, Is.True);
            Assert.That(settings.BackgroundColor, Is.EqualTo("Transparent"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("White"));
            Assert.That(settings.HighlightColor, Is.EqualTo("#B400FF00"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("FullKeyboard65"));
        }

        [Test]
        public void WindowLeft_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();
            var expectedValue = 250.5;

            // Act
            settings.WindowLeft = expectedValue;

            // Assert
            Assert.That(settings.WindowLeft, Is.EqualTo(expectedValue));
        }

        [Test]
        public void WindowTop_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();
            var expectedValue = 350.7;

            // Act
            settings.WindowTop = expectedValue;

            // Assert
            Assert.That(settings.WindowTop, Is.EqualTo(expectedValue));
        }

        [Test]
        public void IsTopmost_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.IsTopmost = false;

            // Assert
            Assert.That(settings.IsTopmost, Is.False);
        }

        [Test]
        public void DisplayScale_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();
            var expectedValue = 1.5;

            // Act
            settings.DisplayScale = expectedValue;

            // Assert
            Assert.That(settings.DisplayScale, Is.EqualTo(expectedValue));
        }

        [Test]
        public void IsMouseVisible_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();

            // Act
            settings.IsMouseVisible = false;

            // Assert
            Assert.That(settings.IsMouseVisible, Is.False);
        }

        [Test]
        public void BackgroundColor_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();
            var expectedValue = "Red";

            // Act
            settings.BackgroundColor = expectedValue;

            // Assert
            Assert.That(settings.BackgroundColor, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ForegroundColor_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();
            var expectedValue = "Blue";

            // Act
            settings.ForegroundColor = expectedValue;

            // Assert
            Assert.That(settings.ForegroundColor, Is.EqualTo(expectedValue));
        }

        [Test]
        public void HighlightColor_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();
            var expectedValue = "Yellow";

            // Act
            settings.HighlightColor = expectedValue;

            // Assert
            Assert.That(settings.HighlightColor, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CurrentProfile_ShouldBeSettableAndGettable()
        {
            // Arrange
            var settings = new AppSettings();
            var expectedValue = "CustomProfile";

            // Act
            settings.CurrentProfile = expectedValue;

            // Assert
            Assert.That(settings.CurrentProfile, Is.EqualTo(expectedValue));
        }

        [Test]
        public void AllProperties_ShouldHaveValidDefaultValues()
        {
            // Act
            var settings = new AppSettings();

            // Assert - すべてのプロパティが適切なデフォルト値を持つことを確認
            Assert.That(settings.WindowLeft, Is.Not.EqualTo(double.NaN));
            Assert.That(settings.WindowTop, Is.Not.EqualTo(double.NaN));
            Assert.That(settings.IsTopmost, Is.TypeOf<bool>());
            Assert.That(settings.DisplayScale, Is.Not.EqualTo(double.NaN));
            Assert.That(settings.IsMouseVisible, Is.TypeOf<bool>());
            Assert.That(settings.BackgroundColor, Is.Not.Null);
            Assert.That(settings.ForegroundColor, Is.Not.Null);
            Assert.That(settings.HighlightColor, Is.Not.Null);
            Assert.That(settings.CurrentProfile, Is.Not.Null);
            
            // デフォルト値の妥当性チェック
            Assert.That(settings.DisplayScale, Is.GreaterThan(0));
            Assert.That(settings.BackgroundColor, Is.Not.Empty);
            Assert.That(settings.ForegroundColor, Is.Not.Empty);
            Assert.That(settings.HighlightColor, Is.Not.Empty);
            Assert.That(settings.CurrentProfile, Is.Not.Empty);
        }

        [Test]
        public void Properties_ShouldAcceptBoundaryValues()
        {
            // Arrange
            var settings = new AppSettings();

            // Act & Assert - 境界値のテスト
            Assert.DoesNotThrow(() => settings.WindowLeft = double.MinValue);
            Assert.DoesNotThrow(() => settings.WindowLeft = double.MaxValue);
            Assert.DoesNotThrow(() => settings.WindowTop = double.MinValue);
            Assert.DoesNotThrow(() => settings.WindowTop = double.MaxValue);
            Assert.DoesNotThrow(() => settings.DisplayScale = double.Epsilon);
            Assert.DoesNotThrow(() => settings.DisplayScale = double.MaxValue);
        }

        [Test]
        public void ColorProperties_ShouldAcceptValidColorValues()
        {
            // Arrange
            var settings = new AppSettings();
            var validColors = new[] { "Red", "Blue", "Green", "#FF0000", "#00FF00", "#0000FF", "Transparent" };

            // Act & Assert
            foreach (var color in validColors)
            {
                Assert.DoesNotThrow(() => settings.BackgroundColor = color, $"BackgroundColor should accept: {color}");
                Assert.DoesNotThrow(() => settings.ForegroundColor = color, $"ForegroundColor should accept: {color}");
                Assert.DoesNotThrow(() => settings.HighlightColor = color, $"HighlightColor should accept: {color}");
            }
        }
    }
}