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

        [SetUp]
        public void SetUp()
        {
            // テスト用の一時ディレクトリを作成
            _tempDirectory = Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempDirectory);

            _settingsManager = new SettingsManager(_tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
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
            Assert.That(settings.HighlightColor, Is.EqualTo("#B400FF00"));
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
            var expectedPath = Path.Combine(_tempDirectory, "settings.yaml");

            // Act
            _settingsManager.Save();

            // Assert
            Assert.That(File.Exists(expectedPath), Is.True);
        }

        [Test]
        public void Load_ShouldReadExistingFile_WhenFileExists()
        {
            // Arrange
            var settingsFile = Path.Combine(_tempDirectory, "settings.yaml");
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
            Assert.That(_settingsManager.Current.BackgroundColor, Is.EqualTo("#FFFF0000"));
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
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("#FF0000FF"));
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
            Assert.That(_settingsManager.Current.HighlightColor, Is.EqualTo("#FFFFFF00"));
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
        public void SetForegroundColor_ShouldSaveAsArgbHex_ForKnownAndCustomColors()
        {
            // Arrange
            _settingsManager.Load();

            // Act & Assert - 色名には変換せず、常に #AARRGGBB で保存する
            _settingsManager.SetForegroundColor(System.Windows.Media.Colors.White);
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("#FFFFFFFF"));

            _settingsManager.SetForegroundColor(System.Windows.Media.Color.FromRgb(128, 64, 192));
            Assert.That(_settingsManager.Current.ForegroundColor, Is.EqualTo("#FF8040C0"));

            _settingsManager.SetHighlightColor(ApplicationConstants.Colors.DefaultHighlight);
            Assert.That(_settingsManager.Current.HighlightColor, Is.EqualTo("#B400FF00"));
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
            var newSettingsManager = new SettingsManager(_tempDirectory);
            newSettingsManager.Load();

            // Assert
            var settings = newSettingsManager.Current;
            Assert.That(settings.WindowLeft, Is.EqualTo(300));
            Assert.That(settings.WindowTop, Is.EqualTo(400));
            Assert.That(settings.DisplayScale, Is.EqualTo(1.5));
            Assert.That(settings.BackgroundColor, Is.EqualTo("#FFFF0000"));
            Assert.That(settings.ForegroundColor, Is.EqualTo("#FF0000FF"));
            Assert.That(settings.HighlightColor, Is.EqualTo("#FFFFFF00"));
            Assert.That(settings.CurrentProfile, Is.EqualTo("TestProfile"));
            Assert.That(settings.IsTopmost, Is.EqualTo(!initialIsTopmost)); // トグルされた値
            Assert.That(settings.IsMouseVisible, Is.EqualTo(!initialIsMouseVisible)); // トグルされた値
        }

        [Test]
        public void Load_ShouldIgnoreUnknownKeys()
        {
            // Arrange
            var settingsFile = Path.Combine(_tempDirectory, "settings.yaml");
            File.WriteAllText(settingsFile, @"
windowLeft: 100
removedField: something
displayScale: 1.5
anotherUnknown:
  nested: 1
");

            // Act
            _settingsManager.Load();

            // Assert
            Assert.That(_settingsManager.Current.WindowLeft, Is.EqualTo(100));
            Assert.That(_settingsManager.Current.DisplayScale, Is.EqualTo(1.5));
            Assert.That(_settingsManager.Recovery, Is.Null);
            Assert.That(File.Exists(settingsFile + ".bak"), Is.False);
        }

        [TestCase("windowLeft: [unclosed\ndisplayScale: 1.5\n", TestName = "Load_ShouldBackUpAndRestoreDefaults_WhenYamlIsMalformed")]
        [TestCase("displayScale: abc\n", TestName = "Load_ShouldBackUpAndRestoreDefaults_WhenTypeMismatches")]
        public void Load_ShouldBackUpAndRestoreDefaults_WhenFileIsCorrupted(string corruptedYaml)
        {
            // Arrange
            var settingsFile = Path.Combine(_tempDirectory, "settings.yaml");
            var backupFile = settingsFile + ".bak";
            File.WriteAllText(settingsFile, corruptedYaml);

            // Act
            _settingsManager.Load();

            // Assert: 元のファイルが .bak に退避されている
            Assert.That(File.Exists(backupFile), Is.True);
            Assert.That(File.ReadAllText(backupFile), Is.EqualTo(corruptedYaml));

            // Assert: 復旧の結果が外に出ている
            var recovery = _settingsManager.Recovery;
            Assert.That(recovery, Is.Not.Null);
            Assert.That(recovery!.BackupSucceeded, Is.True);
            Assert.That(recovery.BackupPath, Is.EqualTo(backupFile));
            Assert.That(recovery.SettingsPath, Is.EqualTo(settingsFile));

            // Assert: 既定値で作り直した settings.yaml が保存され、読み直せる
            Assert.That(File.Exists(settingsFile), Is.True);
            Assert.That(_settingsManager.Current.DisplayScale, Is.EqualTo(1.0));
            var reloaded = new SettingsManager(_tempDirectory);
            reloaded.Load();
            Assert.That(reloaded.Recovery, Is.Null);
            Assert.That(reloaded.Current.DisplayScale, Is.EqualTo(1.0));
        }

        [Test]
        public void Load_ShouldOverwriteExistingBackup_WhenFileIsCorrupted()
        {
            // Arrange
            var settingsFile = Path.Combine(_tempDirectory, "settings.yaml");
            var backupFile = settingsFile + ".bak";
            File.WriteAllText(backupFile, "old backup");
            File.WriteAllText(settingsFile, "displayScale: abc\n");

            // Act
            _settingsManager.Load();

            // Assert
            Assert.That(File.ReadAllText(backupFile), Is.EqualTo("displayScale: abc\n"));
            Assert.That(_settingsManager.Recovery!.BackupSucceeded, Is.True);
        }

        [Test]
        public void Load_ShouldKeepOriginalAndNotSave_WhenBackupFails()
        {
            // Arrange
            var settingsFile = Path.Combine(_tempDirectory, "settings.yaml");
            var backupFile = settingsFile + ".bak";
            const string corruptedYaml = "displayScale: abc\n";
            File.WriteAllText(settingsFile, corruptedYaml);
            var manager = new SettingsManager(
                _tempDirectory,
                (source, destination) => throw new IOException("退避の失敗を模擬"));

            // Act
            manager.Load();

            // Assert: 既定値をメモリ上で使い、失敗を外に出している
            Assert.That(manager.Current.DisplayScale, Is.EqualTo(1.0));
            var recovery = manager.Recovery;
            Assert.That(recovery, Is.Not.Null);
            Assert.That(recovery!.BackupSucceeded, Is.False);
            Assert.That(recovery.BackupPath, Is.EqualTo(backupFile));

            // Assert: 元のファイルは上書きされていない
            Assert.That(File.ReadAllText(settingsFile), Is.EqualTo(corruptedYaml));
            Assert.That(File.Exists(backupFile), Is.False);

            // Act & Assert: その後の設定変更でも保存しない
            manager.UpdateWindowPosition(150, 250);
            manager.SetCurrentProfile("NewProfile");
            manager.Save();
            Assert.That(manager.Current.WindowLeft, Is.EqualTo(150));
            Assert.That(File.ReadAllText(settingsFile), Is.EqualTo(corruptedYaml));
        }

        [Test]
        public void Load_ShouldClearRecovery_WhenLoadedSuccessfullyAfterRecovery()
        {
            // Arrange
            var settingsFile = Path.Combine(_tempDirectory, "settings.yaml");
            File.WriteAllText(settingsFile, "displayScale: abc\n");
            _settingsManager.Load();
            Assert.That(_settingsManager.Recovery, Is.Not.Null);

            // Act: 既定値で作り直したファイルを読み直す
            _settingsManager.Load();

            // Assert
            Assert.That(_settingsManager.Recovery, Is.Null);
        }

        [Test]
        public void Load_ShouldResumeSaving_WhenReloadedSuccessfullyAfterBackupFailure()
        {
            // Arrange: 退避に失敗して保存が止まった状態にする
            var settingsFile = Path.Combine(_tempDirectory, "settings.yaml");
            File.WriteAllText(settingsFile, "displayScale: abc\n");
            var manager = new SettingsManager(
                _tempDirectory,
                (source, destination) => throw new IOException("退避の失敗を模擬"));
            manager.Load();
            Assert.That(manager.Recovery!.BackupSucceeded, Is.False);

            // 正常なファイルに置き換えて読み直す
            File.WriteAllText(settingsFile, "displayScale: 1.5\n");
            manager.Load();
            Assert.That(manager.Recovery, Is.Null);

            // Act
            manager.SetDisplayScale(2.0);

            // Assert: 保存が再開している
            var reloaded = new SettingsManager(_tempDirectory);
            reloaded.Load();
            Assert.That(reloaded.Current.DisplayScale, Is.EqualTo(2.0));
        }
    }
}
