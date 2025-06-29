using System;
using System.Windows;
using System.Windows.Media;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// MainWindowの設定UI管理を担当するクラス
    /// </summary>
    public class MainWindowSettings
    {
        private readonly Window _window;
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// 前景ブラシ
        /// </summary>
        public Brush ForegroundBrush => _settingsService.GetBrushFromColorName(_settingsService.Current.ForegroundColor);
        
        /// <summary>
        /// アクティブブラシ
        /// </summary>
        public Brush ActiveBrush => _settingsService.GetBrushFromColorName(_settingsService.Current.HighlightColor);
        
        /// <summary>
        /// 表示スケール
        /// </summary>
        public double DisplayScale => _settingsService.Current.DisplayScale;
        
        /// <summary>
        /// マウス要素の可視性
        /// </summary>
        public bool IsMouseVisible => _settingsService.Current.IsMouseVisible;

        /// <summary>
        /// 設定変更時のイベント
        /// </summary>
        public event EventHandler? SettingsChanged
        {
            add => _settingsService.SettingsChanged += value;
            remove => _settingsService.SettingsChanged -= value;
        }

        public MainWindowSettings(Window window, ISettingsService settingsService)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        /// <summary>
        /// 設定システムを初期化
        /// </summary>
        public void Initialize()
        {
            try
            {
                _settingsService.Load();
                var settings = _settingsService.Current;

                // ウィンドウ設定を適用
                ApplyWindowSettings(settings);
                
                // 背景色設定を適用
                ApplyBackgroundSettings(settings);
            }
            catch (Exception ex)
            {
                Logger.Error("設定読み込みでエラーが発生", ex);
                throw;
            }
        }

        /// <summary>
        /// 現在の設定を保存
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // ウィンドウ位置を更新
                _settingsService.UpdateWindowPosition(_window.Left, _window.Top);
                
                // MainWindowからプロファイル情報を取得して保存
                if (_window is MainWindow mainWindow && mainWindow.Input?.KeyboardHandler != null)
                {
                    _settingsService.SetCurrentProfile(mainWindow.Input.KeyboardHandler.CurrentProfile.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("設定保存でエラーが発生、処理は継続", ex);
            }
        }

        /// <summary>
        /// 背景色を設定
        /// </summary>
        public void SetBackgroundColor(Color color, bool transparent)
        {
            _settingsService.SetBackgroundColor(color, transparent);
            ApplyBackgroundSettings(_settingsService.Current);
        }

        /// <summary>
        /// 前景色を設定
        /// </summary>
        public void SetForegroundColor(Color color)
        {
            _settingsService.SetForegroundColor(color);
        }

        /// <summary>
        /// ハイライト色を設定
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            _settingsService.SetHighlightColor(color);
        }

        /// <summary>
        /// 最前面表示を切り替え
        /// </summary>
        public void ToggleTopmost()
        {
            _settingsService.ToggleTopmost();
            _window.Topmost = _settingsService.Current.IsTopmost;
        }

        /// <summary>
        /// マウス可視性を切り替え
        /// </summary>
        public void ToggleMouseVisibility()
        {
            _settingsService.ToggleMouseVisibility();
        }

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        public void SetDisplayScale(double scale)
        {
            _settingsService.SetDisplayScale(scale);
        }

        /// <summary>
        /// ウィンドウ設定を適用
        /// </summary>
        private void ApplyWindowSettings(AppSettings settings)
        {
            _window.Left = settings.WindowLeft;
            _window.Top = settings.WindowTop;
            _window.Topmost = settings.IsTopmost;
        }

        /// <summary>
        /// 背景色設定を適用
        /// </summary>
        private void ApplyBackgroundSettings(AppSettings settings)
        {
            if (settings.BackgroundColor == "Transparent")
            {
                _window.Background = BrushFactory.CreateTransparentBackground();
            }
            else
            {
                var backgroundBrush = _settingsService.GetBrushFromColorName(settings.BackgroundColor);
                _window.Background = backgroundBrush;
            }
        }
    }
}