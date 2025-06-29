using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// UI要素の可視性制御を管理するクラス
    /// </summary>
    public class VisibilityController
    {
        private readonly LayoutManager _layoutManager;
        private readonly Canvas _canvas;
        private readonly Func<bool> _getMouseVisibility;

        public VisibilityController(LayoutManager layoutManager, Canvas canvas, Func<bool> getMouseVisibility)
        {
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _getMouseVisibility = getMouseVisibility ?? throw new ArgumentNullException(nameof(getMouseVisibility));
        }

        /// <summary>
        /// フルキーボードレイアウトの表示制御
        /// </summary>
        public void ShowFullKeyboardLayout()
        {
            // YAMLファイルのisVisible設定に基づいて表示するキーを取得
            var visibleKeys = _layoutManager.GetVisibleKeys();
            
            foreach (UIElement child in _canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name))
                {
                    if (MouseElementManager.IsMouseElement(border.Name))
                    {
                        MouseElementManager.SetMouseElementVisibility(child, _getMouseVisibility());
                    }
                    else if (visibleKeys.Contains(border.Name))
                    {
                        border.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        border.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    MouseElementManager.SetMouseElementVisibility(child, _getMouseVisibility());
                }
            }
        }

        /// <summary>
        /// FPSキーボードレイアウトの表示制御
        /// </summary>
        public void ShowFPSKeyboardLayout()
        {
            // YAMLファイルのisVisible設定に基づいて表示するキーを取得
            var visibleKeys = _layoutManager.GetVisibleKeys();
            
            foreach (UIElement child in _canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name))
                {
                    if (MouseElementManager.IsMouseElement(border.Name))
                    {
                        MouseElementManager.SetMouseElementVisibility(child, _getMouseVisibility());
                    }
                    else if (visibleKeys.Contains(border.Name))
                    {
                        // FPSキーは表示
                        border.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        // その他のキーは非表示
                        border.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    MouseElementManager.SetMouseElementVisibility(child, _getMouseVisibility());
                }
            }
        }
    }
}