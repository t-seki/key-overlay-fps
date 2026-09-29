using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using NUnit.Framework;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Tests.Layout
{
    /// <summary>
    /// レイアウト YAML の mouse 設定（body.size / directionCanvas.offset / buttons.*.virtualKey）が効くことのテスト
    /// （WPF 要素を作るため STA で実行）
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class MouseLayoutFieldsTests
    {
        private static string MissingLayoutsDirectory =>
            Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests_missing_" + Guid.NewGuid().ToString("N")[..8]);

        private static LayoutManager LoadEmbedded(KeyboardProfile profile)
        {
            var layoutManager = new LayoutManager(MissingLayoutsDirectory, typeof(LayoutManager).Assembly);
            layoutManager.LoadLayout(profile);
            return layoutManager;
        }

        [Test]
        [TestCase(KeyboardProfile.FullKeyboard65)]
        [TestCase(KeyboardProfile.FPSKeyboard)]
        public void EmbeddedLayout_ShouldHaveMouseButtonVirtualKeys(KeyboardProfile profile)
        {
            var mouse = LoadEmbedded(profile).CurrentLayout!.Mouse;

            var keys = mouse.GetButtonVirtualKeys().ToDictionary(k => k.Name, k => k.VirtualKey);

            Assert.That(keys, Is.EquivalentTo(new Dictionary<string, int>
            {
                ["MouseLeft"] = VirtualKeyCodes.VK_LBUTTON,
                ["MouseRight"] = VirtualKeyCodes.VK_RBUTTON,
                ["MouseWheelButton"] = VirtualKeyCodes.VK_MBUTTON,
                ["MouseButton4"] = VirtualKeyCodes.VK_XBUTTON1,
                ["MouseButton5"] = VirtualKeyCodes.VK_XBUTTON2,
            }));
        }

        [Test]
        [TestCase(KeyboardProfile.FullKeyboard65)]
        [TestCase(KeyboardProfile.FPSKeyboard)]
        public void EmbeddedLayout_ShouldKeepMouseBodySizeAndDirectionCanvasOffset(KeyboardProfile profile)
        {
            var mouse = LoadEmbedded(profile).CurrentLayout!.Mouse;

            // 定数で描いていたころの見た目（本体 60x100、方向表示は位置 + (15, 50)）を保つ
            Assert.That(mouse.Body.Size.Width, Is.EqualTo(60));
            Assert.That(mouse.Body.Size.Height, Is.EqualTo(100));
            Assert.That(mouse.DirectionCanvas.Offset.X, Is.EqualTo(15));
            Assert.That(mouse.DirectionCanvas.Offset.Y, Is.EqualTo(50));
        }

        [Test]
        public void DefaultMouseSettings_ShouldHaveDefaultBodySize()
        {
            // YAML に body が無いとき（フォールバックのレイアウトなど）でも本体が消えない
            var mouse = new MouseSettings();

            Assert.That(mouse.Body.Size.Width, Is.EqualTo(MouseSettings.DefaultBodyWidth));
            Assert.That(mouse.Body.Size.Height, Is.EqualTo(MouseSettings.DefaultBodyHeight));
        }

        [Test]
        public void GetButtonVirtualKeys_ShouldFollowButtonNames_AndSkipButtonsWithoutVirtualKey()
        {
            var mouse = new MouseSettings
            {
                Buttons = new Dictionary<string, ButtonConfig>
                {
                    ["LeftClick"] = new ButtonConfig { VirtualKey = VirtualKeyCodes.VK_LBUTTON },
                    ["Side"] = new ButtonConfig { VirtualKey = VirtualKeyCodes.VK_XBUTTON1 },
                    ["ScrollUp"] = new ButtonConfig(),
                    ["ScrollDown"] = new ButtonConfig { IsVisible = false },
                }
            };

            var keys = mouse.GetButtonVirtualKeys().ToList();

            Assert.That(keys, Is.EqualTo(new List<(string, int)>
            {
                ("LeftClick", VirtualKeyCodes.VK_LBUTTON),
                ("Side", VirtualKeyCodes.VK_XBUTTON1),
            }));
        }

        [Test]
        public void GenerateMouseElements_ShouldUseBodySizeAndOffsets_AndMatchPositionsAfterProfileSwitch()
        {
            var layoutManager = LoadEmbedded(KeyboardProfile.FullKeyboard65);
            var layout = layoutManager.CurrentLayout!;
            layout.Mouse.Position = new PositionConfig { X = 100, Y = 200 };
            layout.Mouse.Body.Offset = new PositionConfig { X = 3, Y = 4 };
            layout.Mouse.Body.Size = new SizeConfig { Width = 70, Height = 90 };
            layout.Mouse.DirectionCanvas.Offset = new PositionConfig { X = 11, Y = 22 };

            var canvas = new Canvas();
            MouseElementGenerator.GenerateMouseElements(canvas, layout);
            var locator = new UIElementLocator();
            locator.BuildCache(canvas);

            var body = locator.FindElement<Border>("MouseBody")!;
            var direction = locator.FindElement<Canvas>("MouseDirectionCanvas")!;
            var left = locator.FindElement<Border>("MouseLeft")!;
            Assert.That(body, Is.Not.Null);
            Assert.That(direction, Is.Not.Null);
            Assert.That(left, Is.Not.Null);

            // 起動直後（初回の生成）
            Assert.That(body.Width, Is.EqualTo(70));
            Assert.That(body.Height, Is.EqualTo(90));
            Assert.That(Canvas.GetLeft(body), Is.EqualTo(103));
            Assert.That(Canvas.GetTop(body), Is.EqualTo(204));
            Assert.That(Canvas.GetLeft(direction), Is.EqualTo(111));
            Assert.That(Canvas.GetTop(direction), Is.EqualTo(222));
            var leftOffset = layout.Mouse.Buttons["MouseLeft"].Offset;
            Assert.That(Canvas.GetLeft(left), Is.EqualTo(100 + leftOffset.X));
            Assert.That(Canvas.GetTop(left), Is.EqualTo(200 + leftOffset.Y));

            // プロファイル切替後の再配置でも位置が変わらない
            new MouseElementManager(layoutManager, locator).UpdateMousePositions();

            Assert.That(Canvas.GetLeft(body), Is.EqualTo(103));
            Assert.That(Canvas.GetTop(body), Is.EqualTo(204));
            Assert.That(Canvas.GetLeft(direction), Is.EqualTo(111));
            Assert.That(Canvas.GetTop(direction), Is.EqualTo(222));
            Assert.That(Canvas.GetLeft(left), Is.EqualTo(100 + leftOffset.X));
            Assert.That(Canvas.GetTop(left), Is.EqualTo(200 + leftOffset.Y));
        }
    }
}
