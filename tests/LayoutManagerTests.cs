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

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _testFilePath = Path.Combine(_testDirectory, "test_layout.yaml");
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
            Assert.IsTrue(content.Contains("elements:"));
        }

        [Test]
        public void ImportLayout_ValidFile_ReturnsCorrectLayout()
        {
            var originalLayout = CreateTestLayout();
            LayoutManager.ExportLayout(originalLayout, _testFilePath);

            var importedLayout = LayoutManager.ImportLayout(_testFilePath);

            Assert.IsNotNull(importedLayout);
            Assert.AreEqual(originalLayout.Global.FontSize, importedLayout.Global.FontSize);
            Assert.AreEqual(originalLayout.Global.KeySize.Width, importedLayout.Global.KeySize.Width);
            Assert.AreEqual(originalLayout.Global.KeySize.Height, importedLayout.Global.KeySize.Height);
            Assert.AreEqual(originalLayout.Global.BackgroundColor, importedLayout.Global.BackgroundColor);
            Assert.AreEqual(originalLayout.Elements.Count, importedLayout.Elements.Count);
        }

        [Test]
        public void ImportLayout_NonExistentFile_ThrowsFileNotFoundException()
        {
            var nonExistentPath = Path.Combine(_testDirectory, "non_existent.yaml");

            Assert.Throws<FileNotFoundException>(() => LayoutManager.ImportLayout(nonExistentPath));
        }

        [Test]
        public void ImportLayout_InvalidYaml_ThrowsInvalidOperationException()
        {
            File.WriteAllText(_testFilePath, "invalid yaml content: [[[");

            Assert.Throws<InvalidOperationException>(() => LayoutManager.ImportLayout(_testFilePath));
        }

        [Test]
        public void ImportLayout_InvalidLayout_ThrowsInvalidOperationException()
        {
            var invalidYaml = @"
global:
  fontSize: -1  # 無効な値
elements:
  KeyA:
    x: 10
    y: 20
";
            File.WriteAllText(_testFilePath, invalidYaml);

            Assert.Throws<InvalidOperationException>(() => LayoutManager.ImportLayout(_testFilePath));
        }

        [Test]
        public void CreateDefault65KeyboardLayout_ReturnsValidLayout()
        {
            var layout = LayoutManager.CreateDefault65KeyboardLayout();

            Assert.IsNotNull(layout);
            Assert.IsNotNull(layout.Global);
            Assert.IsNotNull(layout.Elements);
            Assert.Greater(layout.Elements.Count, 0);

            // 主要キーの存在確認
            Assert.IsTrue(layout.Elements.ContainsKey("KeyA"));
            Assert.IsTrue(layout.Elements.ContainsKey("KeySpace"));
            Assert.IsTrue(layout.Elements.ContainsKey("KeyEnter"));
            Assert.IsTrue(layout.Elements.ContainsKey("MouseBody"));

            // サイズの異なるキーの確認
            Assert.IsNotNull(layout.Elements["KeySpace"].Size);
            Assert.AreEqual(164, layout.Elements["KeySpace"].Size.Width);
            Assert.AreEqual(26, layout.Elements["KeySpace"].Size.Height);

            Assert.IsNotNull(layout.Elements["KeyShift"].Size);
            Assert.AreEqual(58, layout.Elements["KeyShift"].Size.Width);
        }

        [Test]
        public void CreateDefaultFPSLayout_ReturnsValidLayout()
        {
            var layout = LayoutManager.CreateDefaultFPSLayout();

            Assert.IsNotNull(layout);
            Assert.IsNotNull(layout.Global);
            Assert.IsNotNull(layout.Elements);

            // FPS用ウィンドウサイズ確認
            Assert.AreEqual(520, layout.Global.WindowWidth);
            Assert.AreEqual(160, layout.Global.WindowHeight);

            // FPSで必要なキーが表示状態
            Assert.IsTrue(layout.Elements["KeyW"].IsVisible);
            Assert.IsTrue(layout.Elements["KeyA"].IsVisible);
            Assert.IsTrue(layout.Elements["KeyS"].IsVisible);
            Assert.IsTrue(layout.Elements["KeyD"].IsVisible);
            Assert.IsTrue(layout.Elements["KeySpace"].IsVisible);

            // FPSで不要なキーが非表示状態
            Assert.IsFalse(layout.Elements["KeyHome"].IsVisible);
            Assert.IsFalse(layout.Elements["KeyPageUp"].IsVisible);

            // マウス位置がFPS用に調整されている
            Assert.AreEqual(290, layout.Elements["MouseBody"].X);
        }

        [Test]
        public void RoundTripExportImport_PreservesAllData()
        {
            var originalLayout = CreateComplexTestLayout();

            LayoutManager.ExportLayout(originalLayout, _testFilePath);
            var importedLayout = LayoutManager.ImportLayout(_testFilePath);

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
                Elements = new Dictionary<string, ElementConfig>
                {
                    ["KeyA"] = new ElementConfig { X = 10, Y = 20, Text = "A" },
                    ["KeySpace"] = new ElementConfig 
                    { 
                        X = 50, 
                        Y = 100, 
                        Text = "Space", 
                        Size = new SizeConfig { Width = 100, Height = 30 },
                        FontSize = 8
                    }
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
                    WindowWidth = 600,
                    WindowHeight = 200
                },
                Elements = new Dictionary<string, ElementConfig>
                {
                    ["KeyA"] = new ElementConfig { X = 15, Y = 25, Text = "A", IsVisible = true },
                    ["KeySpace"] = new ElementConfig 
                    { 
                        X = 60, 
                        Y = 120, 
                        Text = "Space", 
                        Size = new SizeConfig { Width = 150, Height = 32 },
                        FontSize = 9,
                        IsVisible = true
                    },
                    ["KeyHidden"] = new ElementConfig { X = 0, Y = 0, Text = "Hidden", IsVisible = false }
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
            Assert.AreEqual(expected.Global.WindowWidth, actual.Global.WindowWidth);
            Assert.AreEqual(expected.Global.WindowHeight, actual.Global.WindowHeight);

            // Elements
            Assert.AreEqual(expected.Elements.Count, actual.Elements.Count);
            
            foreach (var (key, expectedElement) in expected.Elements)
            {
                Assert.IsTrue(actual.Elements.ContainsKey(key), $"Key '{key}' not found in actual elements");
                var actualElement = actual.Elements[key];
                
                Assert.AreEqual(expectedElement.X, actualElement.X, $"X coordinate mismatch for {key}");
                Assert.AreEqual(expectedElement.Y, actualElement.Y, $"Y coordinate mismatch for {key}");
                Assert.AreEqual(expectedElement.Text, actualElement.Text, $"Text mismatch for {key}");
                Assert.AreEqual(expectedElement.IsVisible, actualElement.IsVisible, $"Visibility mismatch for {key}");
                Assert.AreEqual(expectedElement.FontSize, actualElement.FontSize, $"FontSize mismatch for {key}");
                
                if (expectedElement.Size == null)
                {
                    Assert.IsNull(actualElement.Size, $"Size should be null for {key}");
                }
                else
                {
                    Assert.IsNotNull(actualElement.Size, $"Size should not be null for {key}");
                    Assert.AreEqual(expectedElement.Size.Width, actualElement.Size.Width, $"Size width mismatch for {key}");
                    Assert.AreEqual(expectedElement.Size.Height, actualElement.Size.Height, $"Size height mismatch for {key}");
                }
            }
        }
    }
}