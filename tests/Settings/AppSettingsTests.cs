using NUnit.Framework;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Layout;
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
            Assert.That(settings.HighlightColor, Is.EqualTo("Green"));
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
        public void CreateFromLayout_ShouldReturnAppSettings_WithNullLayout()
        {
            // Act
            var settings = AppSettings.CreateFromLayout(null!);

            // Assert
            Assert.That(settings, Is.Not.Null);
            // null layout should use default AppSettings values
            Assert.That(settings.BackgroundColor, Is.EqualTo("Transparent"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("White"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Green"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("FullKeyboard65"));
            
            // デフォルト値が設定されることを確認
            Assert.That(settings.WindowLeft, Is.EqualTo(ApplicationConstants.UILayout.DefaultWindowLeft));
            Assert.That(settings.WindowTop, Is.EqualTo(ApplicationConstants.UILayout.DefaultWindowTop));
            Assert.That(settings.IsTopmost, Is.True);
            Assert.That(settings.DisplayScale, Is.EqualTo(1.0));
            Assert.That(settings.IsMouseVisible, Is.True);
        }

        [Test]
        public void CreateFromLayout_ShouldReturnAppSettings_WithEmptyLayout()
        {
            // Arrange
            var layout = new LayoutConfig();

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings, Is.Not.Null);
            // When layout has empty Global settings, it uses the default values from GlobalSettings
            Assert.That(settings.BackgroundColor, Is.EqualTo("#2A2A2A")); // Default from GlobalSettings
            Assert.That(settings.ForegroundColor, Is.EqualTo("#FFFFFF")); // Default from GlobalSettings
            Assert.That(settings.HighlightColor, Is.EqualTo("#00FF00")); // Default from GlobalSettings
            Assert.That(settings.CurrentProfile, Is.EqualTo("")); // ProfileInfo.Name defaults to empty string
        }

        [Test]
        public void CreateFromLayout_ShouldExtractColorsFromGlobal_WhenGlobalExists()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Global = new GlobalSettings
                {
                    BackgroundColor = "DarkBlue",
                    ForegroundColor = "LightGray",
                    HighlightColor = "Orange"
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings.BackgroundColor, Is.EqualTo("DarkBlue"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("LightGray"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Orange"));
        }

        [Test]
        public void CreateFromLayout_ShouldExtractProfileName_WhenProfileExists()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Profile = new ProfileInfo
                {
                    Name = "TestProfile"
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings.CurrentProfile, Is.EqualTo("TestProfile"));
        }

        [Test]
        public void CreateFromLayout_ShouldExtractAllValues_WhenFullLayoutProvided()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Global = new GlobalSettings
                {
                    BackgroundColor = "Purple",
                    ForegroundColor = "Cyan",
                    HighlightColor = "Magenta"
                },
                Profile = new ProfileInfo
                {
                    Name = "FullTestProfile"
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings.BackgroundColor, Is.EqualTo("Purple"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("Cyan"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Magenta"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("FullTestProfile"));
            
            // デフォルト値が保持されることを確認
            Assert.That(settings.WindowLeft, Is.EqualTo(ApplicationConstants.UILayout.DefaultWindowLeft));
            Assert.That(settings.WindowTop, Is.EqualTo(ApplicationConstants.UILayout.DefaultWindowTop));
            Assert.That(settings.IsTopmost, Is.True);
            Assert.That(settings.DisplayScale, Is.EqualTo(1.0));
            Assert.That(settings.IsMouseVisible, Is.True);
        }

        [Test]
        public void CreateFromLayout_ShouldHandleNullGlobalConfig()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Global = null!,
                Profile = new ProfileInfo
                {
                    Name = "TestProfile"
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings.BackgroundColor, Is.EqualTo("Transparent"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("White"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Green"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("TestProfile"));
        }

        [Test]
        public void CreateFromLayout_ShouldHandleNullProfileConfig()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Global = new GlobalSettings
                {
                    BackgroundColor = "Navy",
                    ForegroundColor = "Silver",
                    HighlightColor = "Gold"
                },
                Profile = null!
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings.BackgroundColor, Is.EqualTo("Navy"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("Silver"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Gold"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("FullKeyboard65"));
        }

        [Test]
        public void CreateFromLayout_ShouldHandleEmptyStringsInGlobal()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Global = new GlobalSettings
                {
                    BackgroundColor = "",
                    ForegroundColor = "",
                    HighlightColor = ""
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert - empty strings should be preserved
            Assert.That(settings.BackgroundColor, Is.EqualTo(""));
            Assert.That(settings.ForegroundColor, Is.EqualTo(""));
            Assert.That(settings.HighlightColor, Is.EqualTo(""));
        }

        [Test]
        public void CreateFromLayout_ShouldHandleNullStringsInGlobal()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Global = new GlobalSettings
                {
                    BackgroundColor = null!,
                    ForegroundColor = null!,
                    HighlightColor = null!
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings.BackgroundColor, Is.EqualTo("Transparent"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("White"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Green"));
        }

        [Test]
        public void CreateFromLayout_ShouldHandleEmptyProfileName()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Profile = new ProfileInfo
                {
                    Name = ""
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert - empty string should be preserved
            Assert.That(settings.CurrentProfile, Is.EqualTo(""));
        }

        [Test]
        public void CreateFromLayout_ShouldHandleNullProfileName()
        {
            // Arrange
            var layout = new LayoutConfig
            {
                Profile = new ProfileInfo
                {
                    Name = null!
                }
            };

            // Act
            var settings = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings.CurrentProfile, Is.EqualTo("FullKeyboard65"));
        }

        [Test]
        public void CreateFromLayout_ShouldReturnNewInstance_EachTime()
        {
            // Arrange
            var layout = new LayoutConfig();

            // Act
            var settings1 = AppSettings.CreateFromLayout(layout);
            var settings2 = AppSettings.CreateFromLayout(layout);

            // Assert
            Assert.That(settings1, Is.Not.SameAs(settings2));
            Assert.That(settings1, Is.Not.Null);
            Assert.That(settings2, Is.Not.Null);
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