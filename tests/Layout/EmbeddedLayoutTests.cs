using System;
using System.IO;
using NUnit.Framework;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.Tests.Layout
{
    /// <summary>
    /// 埋め込みリソースレイアウト機能のテスト
    /// </summary>
    [TestFixture]
    public class EmbeddedLayoutTests
    {
        private string _tempDirectory = null!;

        // 存在しないディレクトリ（外部ファイルの経路を通さない）
        private string MissingLayoutsDirectory => Path.Combine(_tempDirectory, "missing_layouts");

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Test]
        public void LoadLayout_WithoutExternalFile_ShouldUseEmbeddedResource()
        {
            var layoutManager = new LayoutManager(MissingLayoutsDirectory, typeof(LayoutManager).Assembly);

            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);
            Assert.That(layoutManager.CurrentLayout?.Profile?.Name, Is.EqualTo("65%キーボード"));

            layoutManager.LoadLayout(KeyboardProfile.FPSKeyboard);
            Assert.That(layoutManager.CurrentLayout?.Profile?.Name, Is.EqualTo("FPSキーボード"));
        }

        [Test]
        public void LoadLayout_WithoutExternalFileAndEmbeddedResource_ShouldFallbackToDefault()
        {
            // テストのアセンブリはレイアウトのリソースを持たない
            var layoutManager = new LayoutManager(MissingLayoutsDirectory, typeof(EmbeddedLayoutTests).Assembly);

            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);
            Assert.That(layoutManager.CurrentLayout?.Profile?.Name, Is.EqualTo("FullKeyboard65"));
            Assert.That(layoutManager.CurrentLayout?.Keys, Is.Not.Empty);

            layoutManager.LoadLayout(KeyboardProfile.FPSKeyboard);
            Assert.That(layoutManager.CurrentLayout?.Profile?.Name, Is.EqualTo("FPSKeyboard"));
        }

        [Test]
        public void LoadLayout_WithExternalFile_ShouldPreferExternalFile()
        {
            var layoutsDirectory = Path.Combine(_tempDirectory, "layouts");
            Directory.CreateDirectory(layoutsDirectory);
            var yaml = File.ReadAllText(FindRepoLayout("65_keyboard.yaml"));
            var replaced = yaml.Replace("name: \"65%キーボード\"", "name: \"外部ファイル\"");
            Assert.That(replaced, Is.Not.EqualTo(yaml), "リポジトリのレイアウトのプロファイル名を書き換えられませんでした");
            File.WriteAllText(Path.Combine(layoutsDirectory, "65_keyboard.yaml"), replaced);
            var layoutManager = new LayoutManager(layoutsDirectory, typeof(LayoutManager).Assembly);

            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);

            Assert.That(layoutManager.CurrentLayout?.Profile?.Name, Is.EqualTo("外部ファイル"));
        }

        [Test]
        public void LoadLayout_WithInvalidExternalFile_ShouldFallbackToEmbeddedResource()
        {
            var layoutsDirectory = Path.Combine(_tempDirectory, "layouts");
            Directory.CreateDirectory(layoutsDirectory);
            File.WriteAllText(Path.Combine(layoutsDirectory, "65_keyboard.yaml"), "invalid yaml content: [[[");
            var layoutManager = new LayoutManager(layoutsDirectory, typeof(LayoutManager).Assembly);

            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);

            Assert.That(layoutManager.CurrentLayout?.Profile?.Name, Is.EqualTo("65%キーボード"));
        }

        private static string FindRepoLayout(string fileName)
        {
            // テスト実行ディレクトリから4階層上がってリポジトリのルートに到達
            var projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))))!;
            return Path.Combine(projectRoot, "layouts", fileName);
        }

        [Test]
        public void LoadLayout_WithDefaultConstructor_ShouldLoadBothProfiles()
        {
            // 既定コンストラクタ（外部ファイルか埋め込みかは環境次第）で両プロファイルが読めること
            var layoutManager = new LayoutManager();

            // Act & Assert - 65%キーボードレイアウト
            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);
            Assert.That(layoutManager.CurrentLayout, Is.Not.Null);
            Assert.That(layoutManager.CurrentLayout.Profile?.Name, Is.EqualTo("65%キーボード"));

            // Act & Assert - FPSレイアウト
            layoutManager.LoadLayout(KeyboardProfile.FPSKeyboard);
            Assert.That(layoutManager.CurrentLayout, Is.Not.Null);
            Assert.That(layoutManager.CurrentLayout.Profile?.Name, Is.EqualTo("FPSキーボード"));
        }

        [Test]
        public void LoadLayout_WhenResourceMissing_ShouldFallbackToDefaultWithValidSections()
        {
            // Arrange
            var layoutManager = new LayoutManager(MissingLayoutsDirectory, typeof(EmbeddedLayoutTests).Assembly);

            // Act - 外部ファイルも埋め込みリソースも無い場合でもデフォルトレイアウトで成功するはず
            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);

            // Assert
            Assert.That(layoutManager.CurrentLayout, Is.Not.Null);
            Assert.That(layoutManager.CurrentLayout.Global, Is.Not.Null);
            Assert.That(layoutManager.CurrentLayout.Keys, Is.Not.Null);
            Assert.That(layoutManager.CurrentLayout.Window, Is.Not.Null);
        }

        [Test]
        public void GetWindowSize_WithDefaultLayout_ShouldReturnValidSize()
        {
            // Arrange
            var layoutManager = new LayoutManager();
            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);

            // Act
            var (width, height) = layoutManager.GetWindowSize();
            var (widthWithMouse, _) = layoutManager.GetWindowSize(true);

            // Assert
            Assert.That(width, Is.GreaterThan(0));
            Assert.That(height, Is.GreaterThan(0));
            Assert.That(widthWithMouse, Is.GreaterThanOrEqualTo(width));
        }

        [Test]
        public void GetMousePosition_WithDefaultLayout_ShouldReturnValidPosition()
        {
            // Arrange
            var layoutManager = new LayoutManager();
            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);

            // Act
            var (left, top) = layoutManager.GetMousePosition();

            // Assert
            Assert.That(left, Is.GreaterThanOrEqualTo(0));
            Assert.That(top, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void GetVisibleKeys_WithDefaultLayout_ShouldReturnKeys()
        {
            // Arrange
            var layoutManager = new LayoutManager();
            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);

            // Act
            var visibleKeys = layoutManager.GetVisibleKeys();

            // Assert
            Assert.That(visibleKeys, Is.Not.Empty);
            Assert.That(visibleKeys, Does.Contain("KeyW"));
            Assert.That(visibleKeys, Does.Contain("KeyA"));
            Assert.That(visibleKeys, Does.Contain("KeyS"));
            Assert.That(visibleKeys, Does.Contain("KeyD"));
        }

        [Test]
        public void LoadLayout_FPSProfile_ShouldHaveDifferentWindowSize()
        {
            // Arrange
            var layoutManager = new LayoutManager();

            // Act
            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);
            var (fullWidth, fullHeight) = layoutManager.GetWindowSize();

            layoutManager.LoadLayout(KeyboardProfile.FPSKeyboard);
            var (fpsWidth, fpsHeight) = layoutManager.GetWindowSize();

            // Assert
            Assert.That(fpsWidth, Is.LessThanOrEqualTo(fullWidth)); // FPSレイアウトは通常より小さい
            Assert.That(fpsHeight, Is.EqualTo(fullHeight)); // 高さは同じ
        }
    }
}