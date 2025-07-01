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
        private readonly SettingsManager _settingsManager;

        /// <summary>
        /// 前景ブラシ
        /// </summary>
        public Brush ForegroundBrush => BrushFactory.CreateBrushFromString(_settingsManager.Current.ForegroundColor, Brushes.White);
        
        /// <summary>
        /// アクティブブラシ
        /// </summary>
        public Brush ActiveBrush => BrushFactory.CreateBrushFromString(_settingsManager.Current.HighlightColor, Brushes.White);
        
        /// <summary>
        /// 表示スケール
        /// </summary>
        public double DisplayScale => _settingsManager.Current.DisplayScale;
        
        /// <summary>
        /// マウス要素の可視性
        /// </summary>
        public bool IsMouseVisible => _settingsManager.Current.IsMouseVisible;

        /// <summary>
        /// 設定変更時のイベント
        /// </summary>
        public event EventHandler? SettingsChanged
        {
            add => _settingsManager.SettingsChanged += value;
            remove => _settingsManager.SettingsChanged -= value;
        }

        public MainWindowSettings(Window window, SettingsManager settingsManager)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        }

        /// <summary>
        /// 設定システムを初期化
        /// </summary>
        public void Initialize()
        {
            try
            {
                _settingsManager.Load();
                var settings = _settingsManager.Current;

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
                _settingsManager.UpdateWindowPosition(_window.Left, _window.Top);
                
                // MainWindowからプロファイル情報を取得して保存
                if (_window is MainWindow mainWindow && mainWindow.ProfileManager != null)
                {
                    _settingsManager.SetCurrentProfile(mainWindow.ProfileManager.GetCurrentProfileName());
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
            _settingsManager.SetBackgroundColor(color, transparent);
            ApplyBackgroundSettings(_settingsManager.Current);
        }

        /// <summary>
        /// 前景色を設定
        /// </summary>
        public void SetForegroundColor(Color color)
        {
            _settingsManager.SetForegroundColor(color);
        }

        /// <summary>
        /// ハイライト色を設定
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            _settingsManager.SetHighlightColor(color);
        }

        /// <summary>
        /// 最前面表示を切り替え
        /// </summary>
        public void ToggleTopmost()
        {
            _settingsManager.ToggleTopmost();
            _window.Topmost = _settingsManager.Current.IsTopmost;
        }

        /// <summary>
        /// マウス可視性を切り替え
        /// </summary>
        public void ToggleMouseVisibility()
        {
            _settingsManager.ToggleMouseVisibility();
        }

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        public void SetDisplayScale(double scale)
        {
            _settingsManager.SetDisplayScale(scale);
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
                var backgroundBrush = BrushFactory.CreateBrushFromString(settings.BackgroundColor, Brushes.White);
                _window.Background = backgroundBrush;
            }
        }
    }
}