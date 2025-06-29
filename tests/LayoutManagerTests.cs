using NUnit.Framework;
using KeyOverlayFPS.Layout;
using System;
using System.IO;
using System.Collections.Generic;

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
        public void ExportLayout_ValidLayout_CreatesFile()
        {
            var layout = CreateTestLayout();

            LayoutManager.ExportLayout(layout, _testFilePath);

            Assert.IsTrue(File.Exists(_testFilePath));
            var content = File.ReadAllText(_testFilePath);
            Assert.IsNotEmpty(content);
            Assert.IsTrue(content.Contains("global:"));
            Assert.IsTrue(content.Contains("keys:"));
        }

        [Test]
        public void ImportLayout_ValidFile_ReturnsCorrectLayout()
        {
            var originalLayout = CreateTestLayout();
            LayoutManager.ExportLayout(originalLayout, _testFilePath);

            var importedLayout = _layoutManager.ImportLayout(_testFilePath);

            Assert.IsNotNull(importedLayout);
            Assert.AreEqual(originalLayout.Global.FontSize, importedLayout.Global.FontSize);
            Assert.AreEqual(originalLayout.Global.KeySize.Width, importedLayout.Global.KeySize.Width);
            Assert.AreEqual(originalLayout.Global.KeySize.Height, importedLayout.Global.KeySize.Height);
            Assert.AreEqual(originalLayout.Global.BackgroundColor, importedLayout.Global.BackgroundColor);
            Assert.AreEqual(originalLayout.Keys.Count, importedLayout.Keys.Count);
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
            Assert.That(layout.Window.Width, Is.EqualTo(580));
            Assert.That(layout.Window.Height, Is.EqualTo(160));
            Assert.That(layout.Keys.Count, Is.GreaterThan(60)); // 65%キーボードなので60キー以上
            Assert.That(layout.Mouse, Is.Not.Null);
            Assert.That(layout.Mouse.IsVisible, Is.True);
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
            Assert.That(layout.Window.Width, Is.EqualTo(520));
            Assert.That(layout.Window.Height, Is.EqualTo(160));
            Assert.That(layout.Keys.Count, Is.GreaterThan(20)); // FPSキーボードなので20キー以上
            Assert.That(layout.Mouse, Is.Not.Null);
            Assert.That(layout.Mouse.IsVisible, Is.True);
            Assert.That(layout.Mouse.Position.X, Is.EqualTo(290)); // FPS用位置
        }

        [Test]
        public void RoundTripExportImport_PreservesAllData()
        {
            var originalLayout = CreateComplexTestLayout();

            LayoutManager.ExportLayout(originalLayout, _testFilePath);
            var importedLayout = _layoutManager.ImportLayout(_testFilePath);

            AssertLayoutsEqual(originalLayout, importedLayout);
        }

        [Test]
        public void ExportLayout_CreatesDirectory_WhenNotExists()
        {
            var deepPath = Path.Combine(_testDirectory, "deep", "nested", "path", "layout.yaml");

            var layout = CreateTestLayout();
            LayoutManager.ExportLayout(layout, deepPath);

            Assert.IsTrue(File.Exists(deepPath));
        }

        private static LayoutConfig CreateTestLayout()
        {
            return new LayoutConfig
            {
                Global = new GlobalSettings
                {
                    FontSize = 12,
                    KeySize = new SizeConfig { Width = 30, Height = 30 },
                    BackgroundColor = "#123456",
                    HighlightColor = "#789ABC"
                },
                Keys = new Dictionary<string, KeyDefinition>
                {
                    ["KeyA"] = new KeyDefinition 
                    { 
                        Position = new PositionConfig { X = 10, Y = 20 }, 
                        Text = "A",
                        VirtualKey = 0x41
                    },
                    ["KeySpace"] = new KeyDefinition 
                    { 
                        Position = new PositionConfig { X = 50, Y = 100 }, 
                        Text = "Space",
                        Size = new SizeConfig { Width = 100, Height = 30 },
                        FontSize = 8,
                        VirtualKey = 0x20
                    }
                },
                Mouse = new MouseSettings
                {
                    Position = new PositionConfig { X = 290, Y = 20 },
                    IsVisible = true
                }
            };
        }

        private static LayoutConfig CreateComplexTestLayout()
        {
            return new LayoutConfig
            {
                Global = new GlobalSettings
                {
                    FontSize = 14,
                    FontFamily = "Consolas",
                    KeySize = new SizeConfig { Width = 28, Height = 28 },
                    BackgroundColor = "#FF0000",
                    HighlightColor = "#00FF00",
                    ForegroundColor = "#0000FF",
                    MouseCircleSize = 25,
                    MouseCircleColor = "#FFFF00",
                    MouseMoveHighlightColor = "#FF00FF",
                    MouseMoveHighlightDuration = 0.2,
                    MouseMoveThreshold = 3.0,
                },
                Keys = new Dictionary<string, KeyDefinition>
                {
                    ["KeyA"] = new KeyDefinition 
                    { 
                        Position = new PositionConfig { X = 15, Y = 25 }, 
                        Text = "A", 
                        IsVisible = true,
                        VirtualKey = 0x41
                    },
                    ["KeySpace"] = new KeyDefinition 
                    { 
                        Position = new PositionConfig { X = 60, Y = 120 }, 
                        Text = "Space",
                        Size = new SizeConfig { Width = 150, Height = 32 },
                        FontSize = 9,
                        IsVisible = true,
                        VirtualKey = 0x20
                    },
                    ["KeyHidden"] = new KeyDefinition 
                    { 
                        Position = new PositionConfig { X = 0, Y = 0 }, 
                        Text = "Hidden", 
                        IsVisible = false,
                        VirtualKey = 0x48
                    }
                },
                Mouse = new MouseSettings
                {
                    Position = new PositionConfig { X = 290, Y = 20 },
                    IsVisible = true,
                    Movement = new MouseMovementConfig
                    {
                        CircleSize = 25,
                        CircleColor = "#FFFF00",
                        HighlightColor = "#FF00FF",
                        HighlightDuration = 200,
                        Threshold = 3.0
                    }
                }
            };
        }

        private static void AssertLayoutsEqual(LayoutConfig expected, LayoutConfig actual)
        {
            // Global settings
            Assert.AreEqual(expected.Global.FontSize, actual.Global.FontSize);
            Assert.AreEqual(expected.Global.FontFamily, actual.Global.FontFamily);
            Assert.AreEqual(expected.Global.KeySize.Width, actual.Global.KeySize.Width);
            Assert.AreEqual(expected.Global.KeySize.Height, actual.Global.KeySize.Height);
            Assert.AreEqual(expected.Global.BackgroundColor, actual.Global.BackgroundColor);
            Assert.AreEqual(expected.Global.HighlightColor, actual.Global.HighlightColor);
            Assert.AreEqual(expected.Global.ForegroundColor, actual.Global.ForegroundColor);
            Assert.AreEqual(expected.Global.MouseCircleSize, actual.Global.MouseCircleSize);
            Assert.AreEqual(expected.Global.MouseCircleColor, actual.Global.MouseCircleColor);
            Assert.AreEqual(expected.Global.MouseMoveHighlightColor, actual.Global.MouseMoveHighlightColor);
            Assert.AreEqual(expected.Global.MouseMoveHighlightDuration, actual.Global.MouseMoveHighlightDuration, 0.001);
            Assert.AreEqual(expected.Global.MouseMoveThreshold, actual.Global.MouseMoveThreshold, 0.001);

            // Keys
            Assert.AreEqual(expected.Keys.Count, actual.Keys.Count);
            
            foreach (var (key, expectedKey) in expected.Keys)
            {
                Assert.IsTrue(actual.Keys.ContainsKey(key), $"Key '{key}' not found in actual keys");
                var actualKey = actual.Keys[key];
                
                Assert.AreEqual(expectedKey.Position.X, actualKey.Position.X, $"X position mismatch for {key}");
                Assert.AreEqual(expectedKey.Position.Y, actualKey.Position.Y, $"Y position mismatch for {key}");
                Assert.AreEqual(expectedKey.Text, actualKey.Text, $"Text mismatch for {key}");
                Assert.AreEqual(expectedKey.IsVisible, actualKey.IsVisible, $"Visibility mismatch for {key}");
                Assert.AreEqual(expectedKey.FontSize, actualKey.FontSize, $"FontSize mismatch for {key}");
                Assert.AreEqual(expectedKey.VirtualKey, actualKey.VirtualKey, $"VirtualKey mismatch for {key}");
                
                if (expectedKey.Size == null)
                {
                    Assert.IsNull(actualKey.Size, $"Size should be null for {key}");
                }
                else
                {
                    Assert.IsNotNull(actualKey.Size, $"Size should not be null for {key}");
                    Assert.AreEqual(expectedKey.Size.Width, actualKey.Size.Width, $"Size width mismatch for {key}");
                    Assert.AreEqual(expectedKey.Size.Height, actualKey.Size.Height, $"Size height mismatch for {key}");
                }
            }

            // Mouse settings
            Assert.AreEqual(expected.Mouse.Position.X, actual.Mouse.Position.X);
            Assert.AreEqual(expected.Mouse.Position.Y, actual.Mouse.Position.Y);
            Assert.AreEqual(expected.Mouse.IsVisible, actual.Mouse.IsVisible);
            
            if (expected.Mouse.Movement != null && actual.Mouse.Movement != null)
            {
                Assert.AreEqual(expected.Mouse.Movement.CircleSize, actual.Mouse.Movement.CircleSize);
                Assert.AreEqual(expected.Mouse.Movement.CircleColor, actual.Mouse.Movement.CircleColor);
                Assert.AreEqual(expected.Mouse.Movement.HighlightColor, actual.Mouse.Movement.HighlightColor);
                Assert.AreEqual(expected.Mouse.Movement.HighlightDuration, actual.Mouse.Movement.HighlightDuration);
                Assert.AreEqual(expected.Mouse.Movement.Threshold, actual.Mouse.Movement.Threshold);
            }
        }
    }
}