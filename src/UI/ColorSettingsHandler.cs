using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// 色設定に関する処理を担当するクラス
    /// </summary>
    public class ColorSettingsHandler
    {
        private readonly MainWindowSettings _settings;
        private readonly Window _window;

        public ColorSettingsHandler(MainWindowSettings settings, Window window)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        /// <summary>
        /// 背景色を設定
        /// </summary>
        public void SetBackgroundColor(Color color, bool transparent)
        {
            _settings.SetBackgroundColor(color, transparent);
        }

        /// <summary>
        /// 前景色を設定
        /// </summary>
        public void SetForegroundColor(Color color)
        {
            _settings.SetForegroundColor(color);
            UpdateAllTextForeground();
        }

        /// <summary>
        /// 全てのテキスト要素の前景色を更新（設定変更なし）
        /// </summary>
        public void UpdateAllTextForeground()
        {
            var canvas = _window.Content as Canvas;
            if (canvas != null)
            {
                foreach (var child in canvas.Children)
                {
                    if (child is Border border)
                    {
                        UpdateBorderTextForeground(border);
                    }
                }
            }
        }

        /// <summary>
        /// ハイライト色を設定
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            _settings.SetHighlightColor(color);
        }

        /// <summary>
        /// Border内のテキスト要素の前景色を更新
        /// </summary>
        private void UpdateBorderTextForeground(Border border)
        {
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Foreground = _settings.ForegroundBrush;
            }
            else if (border.Child is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = _settings.ForegroundBrush;
                    }
                }
            }
        }
    }
}