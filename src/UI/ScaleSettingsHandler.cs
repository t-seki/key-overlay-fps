using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// スケール設定に関する処理を担当するクラス
    /// </summary>
    public class ScaleSettingsHandler
    {
        private readonly MainWindowSettings _settings;
        private readonly LayoutManager _layoutManager;
        private readonly Window _window;

        public ScaleSettingsHandler(MainWindowSettings settings, LayoutManager layoutManager, Window window)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        public void SetDisplayScale(double scale)
        {
            _settings.SetDisplayScale(scale);
            ApplyDisplayScale();
        }

        /// <summary>
        /// 表示スケールを適用
        /// </summary>
        public void ApplyDisplayScale()
        {
            var canvas = _window.Content as Canvas;
            if (canvas != null)
            {
                // Canvas全体にスケール変換を適用
                var transform = new ScaleTransform(_settings.DisplayScale, _settings.DisplayScale);
                canvas.RenderTransform = transform;
                
                // YAMLファイルからウィンドウサイズを取得
                var (baseWidth, baseHeight) = _layoutManager.GetWindowSize(_settings.IsMouseVisible);
                
                _window.Width = baseWidth * _settings.DisplayScale;
                _window.Height = baseHeight * _settings.DisplayScale;
            }
        }
    }
}