using System;
using System.Threading;
using System.Windows.Controls;
using NUnit.Framework;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.Tests.Layout
{
    /// <summary>
    /// UIElementLocatorのテストクラス（WPF要素を作るため STA で実行）
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class UIElementLocatorTests
    {
        [Test]
        public void BuildCache_ShouldThrow_ForNullCanvas()
        {
            var locator = new UIElementLocator();

            Assert.Throws<ArgumentNullException>(() => locator.BuildCache(null!));
        }

        [Test]
        public void FindElement_ShouldReturnNull_BeforeBuildCache()
        {
            var locator = new UIElementLocator();

            Assert.That(locator.FindElement<Border>("Anything"), Is.Null);
        }

        [Test]
        public void FindElement_ShouldFindNamedElements_InNestedContainers()
        {
            var canvas = new Canvas();
            var direct = new Border { Name = "Direct" };
            var nestedChild = new TextBlock { Name = "InBorder" };
            var stackChild = new Button { Name = "InStackPanel" };
            var stack = new StackPanel { Name = "Stack" };
            stack.Children.Add(stackChild);
            var wrapper = new Border { Name = "Wrapper", Child = nestedChild };
            canvas.Children.Add(direct);
            canvas.Children.Add(wrapper);
            canvas.Children.Add(stack);
            var locator = new UIElementLocator();

            locator.BuildCache(canvas);

            Assert.That(locator.FindElement<Border>("Direct"), Is.SameAs(direct));
            Assert.That(locator.FindElement<Border>("Wrapper"), Is.SameAs(wrapper));
            Assert.That(locator.FindElement<TextBlock>("InBorder"), Is.SameAs(nestedChild));
            Assert.That(locator.FindElement<StackPanel>("Stack"), Is.SameAs(stack));
            Assert.That(locator.FindElement<Button>("InStackPanel"), Is.SameAs(stackChild));
        }

        [Test]
        public void FindElement_ShouldReturnNull_ForUnknownNameOrWrongType()
        {
            var canvas = new Canvas();
            canvas.Children.Add(new Border { Name = "Direct" });
            var locator = new UIElementLocator();
            locator.BuildCache(canvas);

            Assert.That(locator.FindElement<Border>("Unknown"), Is.Null);
            Assert.That(locator.FindElement<TextBlock>("Direct"), Is.Null);
        }

        [Test]
        public void BuildCache_ShouldClearPreviousCache()
        {
            var first = new Canvas();
            first.Children.Add(new Border { Name = "Old" });
            var second = new Canvas();
            second.Children.Add(new Border { Name = "New" });
            var locator = new UIElementLocator();

            locator.BuildCache(first);
            locator.BuildCache(second);

            Assert.That(locator.FindElement<Border>("Old"), Is.Null);
            Assert.That(locator.FindElement<Border>("New"), Is.Not.Null);
        }
    }
}
