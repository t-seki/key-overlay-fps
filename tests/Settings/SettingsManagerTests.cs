using System;
using System.IO;
using System.Windows.Media;
using NUnit.Framework;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.Tests.Settings
{
    /// <summary>
    /// SettingsManagerのテストクラス
    /// </summary>
    [TestFixture]
    public class SettingsManagerTests
    {
        private SettingsManager _settingsManager;
        private string _tempDirectory;
        private string _originalAppData;

        [SetUp]
        public void SetUp()
        {
            // テスト用の一時ディレクトリを作成
            _tempDirectory = Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempDirectory);

            // AppDataの場所を一時的に変更
            _originalAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Environment.SetEnvironmentVariable("APPDATA", _tempDirectory);

            _settingsManager = new SettingsManager();
        }

        [TearDown]
        public void TearDown()
        {
            // AppDataの場所を元に戻す
            Environment.SetEnvironmentVariable("APPDATA", _originalAppData);

            // 一時ディレクトリを削除
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Test]
        public void Constructor_ShouldCreateValidInstance()
        {
            // Act & Assert
            Assert.That(_settingsManager, Is.Not.Null);
            Assert.That(_settingsManager.Current, Is.Not.Null);
        }

        [Test]
        public void Current_ShouldReturnDefaultSettings_WhenNotLoaded()
        {
            // Act
            var settings = _settingsManager.Current;

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
        public void Load_ShouldCreateDefaultSettings_WhenFileDoesNotExist()
        {
            // Act
            _settingsManager.Load();

            // Assert
            var settings = _settingsManager.Current;
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.BackgroundColor, Is.Not.Null);
            Assert.That(settings.ForegroundColor, Is.Not.Null);
            Assert.That(settings.HighlightColor, Is.Not.Null);
        }

        [Test]
        public void Save_ShouldCreateSettingsFile()
        {
            // Arrange
            _settingsManager.Load();
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var expectedPath = Path.Combine(appDataPath, "KeyOverlayFPS", "settings.yaml");

            // Act
            _settingsManager.Save();

            // Assert
            Assert.That(File.Exists(expectedPath), Is.True);
        }

        [Test]
        public void Load_ShouldReadExistingFile_WhenFileExists()
        {
            // Arrange
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsDir = Path.Combine(appDataPath, "KeyOverlayFPS");
            Directory.CreateDirectory(settingsDir);
            
            var settingsFile = Path.Combine(settingsDir, "settings.yaml");
            var yamlContent = @"
windowLeft: 100
windowTop: 200
isTopmost: false
displayScale: 1.5
isMouseVisible: false
backgroundColor: Red
foregroundColor: Blue
highlightColor: Yellow
currentProfile: TestProfile
";
            File.WriteAllText(settingsFile, yamlContent);

            // Act
            _settingsManager.Load();

            // Assert
            var settings = _settingsManager.Current;
            Assert.That(settings.WindowLeft, Is.EqualTo(100));
            Assert.That(settings.WindowTop, Is.EqualTo(200));
            Assert.That(settings.IsTopmost, Is.False);
            Assert.That(settings.DisplayScale, Is.EqualTo(1.5));
            Assert.That(settings.IsMouseVisible, Is.False);
            Assert.That(settings.BackgroundColor, Is.EqualTo("Red"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("Blue"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Yellow"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("TestProfile"));
        }

        [Test]
        public void UpdateWindowPosition_ShouldUpdateSettings()
        {
            // Arrange
            _settingsManager.Load();
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.UpdateWindowPosition(150, 250);

            // Assert
            Assert.That(_settingsManager.Current.WindowLeft, Is.EqualTo(150));
            Assert.That(_settingsManager.Current.WindowTop, Is.EqualTo(250));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void ToggleTopmost_ShouldToggleTopmostSetting()
        {
            // Arrange
            _settingsManager.Load();
            var initialValue = _settingsManager.Current.IsTopmost;
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.ToggleTopmost();

            // Assert
            Assert.That(_settingsManager.Current.IsTopmost, Is.EqualTo(!initialValue));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SetBackgroundColor_ShouldSetColorName_WhenNotTransparent()
        {
            // Arrange
            _settingsManager.Load();
            var color = System.Windows.Media.Colors.Red;
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.SetBackgroundColor(color, false);

            // Assert
            Assert.That(_settingsManager.Current.BackgroundColor, Is.EqualTo("Red"));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SetBackgroundColor_ShouldSetTransparent_WhenTransparentIsTrue()
        {
            // Arrange
            _settingsManager.Load();
            var color = System.Windows.Media.Colors.Red;
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.SetBackgroundColor(color, true);

            // Assert
            Assert.That(_settingsManager.Current.BackgroundColor, Is.EqualTo("Transparent"));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SetForegroundColor_ShouldSetColorName()
        {
            // Arrange
            _settingsManager.Load();
            var color = System.Windows.Media.Colors.Blue;
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.SetForegroundColor(color);

            // Assert
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("Blue"));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SetHighlightColor_ShouldSetColorName()
        {
            // Arrange
            _settingsManager.Load();
            var color = System.Windows.Media.Colors.Yellow;
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.SetHighlightColor(color);

            // Assert
            Assert.That(_settingsManager.Current.HighlightColor, Is.EqualTo("Yellow"));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SetDisplayScale_ShouldUpdateScale()
        {
            // Arrange
            _settingsManager.Load();
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.SetDisplayScale(2.0);

            // Assert
            Assert.That(_settingsManager.Current.DisplayScale, Is.EqualTo(2.0));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void ToggleMouseVisibility_ShouldToggleMouseVisibility()
        {
            // Arrange
            _settingsManager.Load();
            var initialValue = _settingsManager.Current.IsMouseVisible;
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.ToggleMouseVisibility();

            // Assert
            Assert.That(_settingsManager.Current.IsMouseVisible, Is.EqualTo(!initialValue));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SetCurrentProfile_ShouldUpdateProfile()
        {
            // Arrange
            _settingsManager.Load();
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            _settingsManager.SetCurrentProfile("NewProfile");

            // Assert
            Assert.That(_settingsManager.Current.CurrentProfile, Is.EqualTo("NewProfile"));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void GetColorNameFromColor_ShouldReturnCorrectColorName_ForKnownColors()
        {
            // Arrange
            _settingsManager.Load();

            // Act & Assert - 複数の色で確認
            _settingsManager.SetForegroundColor(System.Windows.Media.Colors.White);
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("White"));

            _settingsManager.SetForegroundColor(System.Windows.Media.Colors.Green);
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("Green"));

            _settingsManager.SetForegroundColor(System.Windows.Media.Colors.Orange);
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("Orange"));

            _settingsManager.SetForegroundColor(System.Windows.Media.Colors.Purple);
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("Purple"));

            _settingsManager.SetForegroundColor(System.Windows.Media.Colors.Pink);
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("Pink"));
        }

        [Test]
        public void GetColorNameFromColor_ShouldReturnHexValue_ForUnknownColors()
        {
            // Arrange
            _settingsManager.Load();
            var customColor = System.Windows.Media.Color.FromRgb(128, 64, 192); // カスタムカラー

            // Act
            _settingsManager.SetForegroundColor(customColor);

            // Assert
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("#8040C0"));
        }

        [Test]
        public void SettingsChanged_ShouldNotFire_WhenOnlyGettingCurrent()
        {
            // Arrange
            _settingsManager.Load();
            var eventFired = false;
            _settingsManager.SettingsChanged += (sender, e) => eventFired = true;

            // Act
            var _ = _settingsManager.Current;

            // Assert
            Assert.That(eventFired, Is.False);
        }

        [Test]
        public void SaveLoad_ShouldPreserveAllSettings_RoundTripTest()
        {
            // Arrange - 最初に設定を作成し、初期状態を記録
            _settingsManager.Load();
            var initialIsTopmost = _settingsManager.Current.IsTopmost;
            var initialIsMouseVisible = _settingsManager.Current.IsMouseVisible;
            
            // 設定を変更
            _settingsManager.UpdateWindowPosition(300, 400);
            _settingsManager.SetDisplayScale(1.5);
            _settingsManager.SetBackgroundColor(System.Windows.Media.Colors.Red, false);
            _settingsManager.SetForegroundColor(System.Windows.Media.Colors.Blue);
            _settingsManager.SetHighlightColor(System.Windows.Media.Colors.Yellow);
            _settingsManager.SetCurrentProfile("TestProfile");
            _settingsManager.ToggleTopmost();
            _settingsManager.ToggleMouseVisibility();

            // Act - 保存する
            _settingsManager.Save();
            
            // 新しいインスタンスで読み込み
            var newSettingsManager = new SettingsManager();
            newSettingsManager.Load();

            // Assert
            var settings = newSettingsManager.Current;
            Assert.That(settings.WindowLeft, Is.EqualTo(300));
            Assert.That(settings.WindowTop, Is.EqualTo(400));
            Assert.That(settings.DisplayScale, Is.EqualTo(1.5));
            Assert.That(settings.BackgroundColor, Is.EqualTo("Red"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("Blue"));
            Assert.That(settings.HighlightColor, Is.EqualTo("Yellow"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("TestProfile"));
            Assert.That(settings.IsTopmost, Is.EqualTo(!initialIsTopmost)); // トグルされた値
            Assert.That(settings.IsMouseVisible, Is.EqualTo(!initialIsMouseVisible)); // トグルされた値
        }
    }
}