using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using NUnit.Framework;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Tests.UI
{
    /// <summary>
    /// MouseElementManagerのテストクラス（WPF要素を作るため STA で実行）
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class MouseElementManagerTests
    {
        [Test]
        [TestCase("MouseBody", true)]
        [TestCase("MouseLeft", true)]
        [TestCase("MouseRight", true)]
        [TestCase("MouseWheelButton", true)]
        [TestCase("MouseButton4", true)]
        [TestCase("MouseButton5", true)]
        [TestCase("ScrollUp", true)]
        [TestCase("ScrollDown", true)]
        [TestCase("MouseDirectionCanvas", true)]
        [TestCase("KeyW", false)]
        [TestCase("", false)]
        public void IsMouseElement_ShouldMatchKnownMouseElementNames(string name, bool expected)
        {
            Assert.That(MouseElementManager.IsMouseElement(name), Is.EqualTo(expected));
        }

        [Test]
        public void Constructor_ShouldThrow_ForNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new MouseElementManager(null!, new UIElementLocator()));
            Assert.Throws<ArgumentNullException>(() => new MouseElementManager(new LayoutManager(), null!));
        }

        [Test]
        public void SetMouseElementVisibility_ShouldToggleVisibility_ForCanvasAndOtherElements()
        {
            var directionCanvas = new Canvas { Name = "MouseDirectionCanvas" };
            var border = new Border { Name = "MouseBody" };

            MouseElementManager.SetMouseElementVisibility(directionCanvas, false);
            MouseElementManager.SetMouseElementVisibility(border, false);
            Assert.That(directionCanvas.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(border.Visibility, Is.EqualTo(Visibility.Collapsed));

            MouseElementManager.SetMouseElementVisibility(directionCanvas, true);
            MouseElementManager.SetMouseElementVisibility(border, true);
            Assert.That(directionCanvas.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(border.Visibility, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void UpdateMousePositions_ShouldDoNothing_WhenLayoutNotLoaded()
        {
            var canvas = new Canvas();
            var body = new Border { Name = "MouseBody" };
            canvas.Children.Add(body);
            var locator = new UIElementLocator();
            locator.BuildCache(canvas);
            var manager = new MouseElementManager(new LayoutManager(), locator);

            Assert.DoesNotThrow(() => manager.UpdateMousePositions());
            Assert.That(double.IsNaN(Canvas.GetLeft(body)), Is.True);
        }

        [Test]
        public void UpdateMousePositions_ShouldPlaceElementsRelativeToMousePosition()
        {
            var missingDirectory = Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests_missing_" + Guid.NewGuid().ToString("N")[..8]);
            var layoutManager = new LayoutManager(missingDirectory, typeof(LayoutManager).Assembly);
            layoutManager.LoadLayout(KeyboardProfile.FullKeyboard65);
            var mouse = layoutManager.CurrentLayout!.Mouse;
            mouse.Position = new PositionConfig { X = 100, Y = 200 };
            mouse.Body.Offset = new PositionConfig { X = 3, Y = 4 };
            mouse.DirectionCanvas.Offset = new PositionConfig { X = 5, Y = 6 };
            mouse.Buttons.Clear();
            mouse.Buttons["MouseLeft"] = new ButtonConfig { Offset = new PositionConfig { X = 7, Y = 8 } };
            mouse.Buttons["ScrollUp"] = new ButtonConfig { Offset = new PositionConfig { X = 9, Y = 10 } };

            var canvas = new Canvas();
            var body = new Border { Name = "MouseBody" };
            var left = new Border { Name = "MouseLeft" };
            var scrollUp = new Border { Name = "ScrollUp" };
            var scrollDown = new Border { Name = "ScrollDown" }; // 設定に無いので動かない
            var direction = new Canvas { Name = "MouseDirectionCanvas" };
            foreach (var element in new UIElement[] { body, left, scrollUp, scrollDown, direction })
            {
                canvas.Children.Add(element);
            }
            var locator = new UIElementLocator();
            locator.BuildCache(canvas);
            var manager = new MouseElementManager(layoutManager, locator);

            manager.UpdateMousePositions();

            Assert.That(Canvas.GetLeft(body), Is.EqualTo(103));
            Assert.That(Canvas.GetTop(body), Is.EqualTo(204));
            Assert.That(Canvas.GetLeft(left), Is.EqualTo(107));
            Assert.That(Canvas.GetTop(left), Is.EqualTo(208));
            Assert.That(Canvas.GetLeft(scrollUp), Is.EqualTo(109));
            Assert.That(Canvas.GetTop(scrollUp), Is.EqualTo(210));
            Assert.That(Canvas.GetLeft(direction), Is.EqualTo(105));
            Assert.That(Canvas.GetTop(direction), Is.EqualTo(206));
            Assert.That(double.IsNaN(Canvas.GetLeft(scrollDown)), Is.True);
        }
    }
}
