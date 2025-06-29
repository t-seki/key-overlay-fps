using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// マウス要素の管理とレイアウトを担当するクラス
    /// </summary>
    public class MouseElementManager
    {
        private readonly LayoutManager _layoutManager;
        private readonly UIElementLocator _elementLocator;
        
        /// <summary>
        /// マウス要素管理クラス
        /// </summary>
        private static class MouseElements
        {
            public static readonly HashSet<string> Names = new()
            {
                "MouseBody", "MouseLeft", "MouseRight", "MouseWheelButton", 
                "MouseButton4", "MouseButton5", "ScrollUp", "ScrollDown",
                "MouseDirectionCanvas"
            };
        }

        public MouseElementManager(LayoutManager layoutManager, UIElementLocator elementLocator)
        {
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _elementLocator = elementLocator ?? throw new ArgumentNullException(nameof(elementLocator));
        }

        /// <summary>
        /// 要素がマウス要素かどうか判定
        /// </summary>
        public static bool IsMouseElement(string elementName) => MouseElements.Names.Contains(elementName);

        /// <summary>
        /// マウス要素の可視性を設定
        /// </summary>
        public static void SetMouseElementVisibility(UIElement element, bool isMouseVisible)
        {
            if (element is Canvas childCanvas && childCanvas.Name == "MouseDirectionCanvas")
            {
                // マウス移動可視化キャンバス
                element.Visibility = isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // マウス本体や他のマウス要素
                element.Visibility = isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 全マウス要素の位置を更新
        /// </summary>
        public void UpdateMousePositions()
        {
            var layout = _layoutManager.CurrentLayout?.Mouse;
            if (layout == null) return;

            var position = _layoutManager.GetMousePosition();
            
            // ボタン要素の位置を更新
            foreach (var (buttonName, buttonConfig) in layout.Buttons)
            {
                var element = _elementLocator.FindElement<FrameworkElement>(buttonName);
                if (element != null)
                {
                    Canvas.SetLeft(element, position.Left + buttonConfig.Offset.X);
                    Canvas.SetTop(element, position.Top + buttonConfig.Offset.Y);
                }
            }
            
            // マウス本体の位置を更新
            var bodyElement = _elementLocator.FindElement<FrameworkElement>("MouseBody");
            if (bodyElement != null)
            {
                Canvas.SetLeft(bodyElement, position.Left + layout.Body.Offset.X);
                Canvas.SetTop(bodyElement, position.Top + layout.Body.Offset.Y);
            }
            
            // スクロール要素の位置を更新
            var scrollUpElement = _elementLocator.FindElement<FrameworkElement>("ScrollUp");
            if (scrollUpElement != null && layout.Buttons.ContainsKey("ScrollUp"))
            {
                var scrollUpConfig = layout.Buttons["ScrollUp"];
                Canvas.SetLeft(scrollUpElement, position.Left + scrollUpConfig.Offset.X);
                Canvas.SetTop(scrollUpElement, position.Top + scrollUpConfig.Offset.Y);
            }
            
            var scrollDownElement = _elementLocator.FindElement<FrameworkElement>("ScrollDown");
            if (scrollDownElement != null && layout.Buttons.ContainsKey("ScrollDown"))
            {
                var scrollDownConfig = layout.Buttons["ScrollDown"];
                Canvas.SetLeft(scrollDownElement, position.Left + scrollDownConfig.Offset.X);
                Canvas.SetTop(scrollDownElement, position.Top + scrollDownConfig.Offset.Y);
            }
            
            // 方向表示キャンバスの位置を更新
            var directionElement = _elementLocator.FindElement<FrameworkElement>("MouseDirectionCanvas");
            if (directionElement != null)
            {
                Canvas.SetLeft(directionElement, position.Left + layout.DirectionCanvas.Offset.X);
                Canvas.SetTop(directionElement, position.Top + layout.DirectionCanvas.Offset.Y);
            }
        }
    }
}