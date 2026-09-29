using NUnit.Framework;
using KeyOverlayFPS.Layout;
using System;
using System.IO;

namespace KeyOverlayFPS.Tests
{
    [TestFixture]
    public class LayoutManagerTests
    {
        private string _testDirectory = null!;
        private string _testFilePath = null!;
        private LayoutManager _layoutManager = null!;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _testFilePath = Path.Combine(_testDirectory, "test_layout.yaml");
            _layoutManager = new LayoutManager();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void ImportLayout_ValidFile_ReturnsCorrectLayout()
        {
            var yaml = @"
global:
  fontSize: 12
  foregroundColor: ""#123456""
keys:
  KeyA:
    position: { x: 10, y: 20 }
    text: ""A""
    virtualKey: 0x41
  KeySpace:
    position: { x: 50, y: 100 }
    size: { width: 100, height: 30 }
    text: ""Space""
    fontSize: 8
    virtualKey: VK_SPACE
mouse:
  position: { x: 290, y: 20 }
";
            File.WriteAllText(_testFilePath, yaml);

            var importedLayout = _layoutManager.ImportLayout(_testFilePath);

            Assert.That(importedLayout, Is.Not.Null);
            Assert.That(importedLayout.Global.FontSize, Is.EqualTo(12));
            Assert.That(importedLayout.Global.ForegroundColor, Is.EqualTo("#123456"));
            Assert.That(importedLayout.Keys.Count, Is.EqualTo(2));
            Assert.That(importedLayout.Keys["KeyA"].VirtualKey, Is.EqualTo(0x41));
            Assert.That(importedLayout.Keys["KeySpace"].VirtualKey, Is.EqualTo(0x20));
            Assert.That(importedLayout.Keys["KeySpace"].Size.Width, Is.EqualTo(100));
            Assert.That(importedLayout.Mouse.Position.X, Is.EqualTo(290));
        }

        [Test]
        public void ImportLayout_WithRemovedGlobalColors_IgnoresThem()
        {
            // 旧版のレイアウトにあった global.backgroundColor / highlightColor は、スキーマから消しても読める
            var yaml = @"
global:
  fontSize: 12
  backgroundColor: ""#2A2A2A""
  highlightColor: ""#00FF00""
  foregroundColor: ""#FFFFFF""
keys:
  KeyA:
    position: { x: 10, y: 20 }
    text: ""A""
    virtualKey: VK_A
";
            File.WriteAllText(_testFilePath, yaml);

            var importedLayout = _layoutManager.ImportLayout(_testFilePath);

            Assert.That(importedLayout.Global.FontSize, Is.EqualTo(12));
            Assert.That(importedLayout.Global.ForegroundColor, Is.EqualTo("#FFFFFF"));
            Assert.That(importedLayout.Keys["KeyA"].VirtualKey, Is.EqualTo(0x41));
        }

        [Test]
        public void ImportLayout_NonExistentFile_ThrowsFileNotFoundException()
        {
            var nonExistentPath = Path.Combine(_testDirectory, "non_existent.yaml");

            Assert.Throws<FileNotFoundException>(() => _layoutManager.ImportLayout(nonExistentPath));
        }

        [Test]
        public void ImportLayout_InvalidYaml_ThrowsInvalidOperationException()
        {
            File.WriteAllText(_testFilePath, "invalid yaml content: [[[");

            Assert.Throws<InvalidOperationException>(() => _layoutManager.ImportLayout(_testFilePath));
        }

        [Test]
        public void ImportLayout_InvalidLayout_ThrowsInvalidOperationException()
        {
            var invalidYaml = @"
global:
  fontSize: -1  # 無効な値
keys:
  KeyA:
    position: { x: 10, y: 20 }
    text: ""A""
";
            File.WriteAllText(_testFilePath, invalidYaml);

            Assert.Throws<InvalidOperationException>(() => _layoutManager.ImportLayout(_testFilePath));
        }

        [Test]
        public void ImportLayout_ShouldCreateValidLayout_ForSixtyFiveKeyboard()
        {
            // Arrange
            // テスト実行ディレクトリから4階層上がってプロジェクトルートに到達
            var projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))))!;
            var layoutPath = Path.Combine(projectRoot, "layouts", "65_keyboard.yaml");

            // Act
            var layout = _layoutManager.ImportLayout(layoutPath);

            // Assert
            Assert.That(layout, Is.Not.Null);
            Assert.That(layout.Profile.Name, Is.EqualTo("65%キーボード"));
            Assert.That(layout.Window.Width, Is.EqualTo(560));
            Assert.That(layout.Window.Height, Is.EqualTo(160));
            Assert.That(layout.Keys.Count, Is.GreaterThan(60)); // 65%キーボードなので60キー以上
            Assert.That(layout.Mouse, Is.Not.Null);
            // マウス要素が定義されていることを確認（IsVisibleプロパティは削除済み）
        }

        [Test]
        public void ImportLayout_ShouldCreateValidLayout_ForFPSKeyboard()
        {
            // Arrange
            // テスト実行ディレクトリから4階層上がってプロジェクトルートに到達
            var projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))))!;
            var layoutPath = Path.Combine(projectRoot, "layouts", "fps_keyboard.yaml");

            // Act
            var layout = _layoutManager.ImportLayout(layoutPath);

            // Assert
            Assert.That(layout, Is.Not.Null);
            Assert.That(layout.Profile.Name, Is.EqualTo("FPSキーボード"));
            Assert.That(layout.Window.Width, Is.EqualTo(370));
            Assert.That(layout.Window.Height, Is.EqualTo(160));
            Assert.That(layout.Keys.Count, Is.GreaterThan(20)); // FPSキーボードなので20キー以上
            Assert.That(layout.Mouse, Is.Not.Null);
            // マウス要素が定義されていることを確認（IsVisibleプロパティは削除済み）
            Assert.That(layout.Mouse.Position.X, Is.EqualTo(290)); // FPS用位置
        }
    }
}