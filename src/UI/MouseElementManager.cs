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
        private readonly Func<string, FrameworkElement?> _elementFinder;
        
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
            
            public static readonly Dictionary<string, (double Left, double Top)> Offsets = new()
            {
                { "MouseBody", (0, 0) },           // 基準位置
                { "MouseLeft", (3, 3) },
                { "MouseRight", (32, 3) },
                { "MouseWheelButton", (25, 10) },
                { "MouseButton4", (0, 64) },
                { "MouseButton5", (0, 42) },
                { "ScrollUp", (35, 10) },
                { "ScrollDown", (35, 24) },
                { "MouseDirectionCanvas", (15, 50) } // マウス本体中央下に配置
            };
        }

        public MouseElementManager(LayoutManager layoutManager, Func<string, FrameworkElement?> elementFinder)
        {
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _elementFinder = elementFinder ?? throw new ArgumentNullException(nameof(elementFinder));
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
            var position = _layoutManager.GetMousePosition();
            
            // 全マウス要素の位置を一括更新
            foreach (var (elementName, offset) in MouseElements.Offsets)
            {
                var element = _elementFinder(elementName);
                if (element != null)
                {
                    Canvas.SetLeft(element, position.Left + offset.Left);
                    Canvas.SetTop(element, position.Top + offset.Top);
                }
            }
        }
    }
}