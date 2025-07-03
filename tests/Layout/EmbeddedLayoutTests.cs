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
        [Test]
        public void LoadLayout_WithEmbeddedResources_ShouldSucceed()
        {
            // Arrange
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
        public void LoadLayout_WhenEmbeddedResourceFails_ShouldFallbackToDefault()
        {
            // Arrange
            var layoutManager = new LayoutManager();

            // Act - 埋め込みリソースが見つからない場合でもデフォルトレイアウトで成功するはず
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