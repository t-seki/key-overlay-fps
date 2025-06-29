using System;
using System.Windows;
using System.Windows.Media;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// MainWindowの設定管理責務を担当するクラス
    /// </summary>
    public class MainWindowSettings
    {
        private readonly Window _window;
        private readonly SettingsManager _settingsManager;
        
        private Brush _foregroundBrush = Brushes.White;
        private Brush _activeBrush = new SolidColorBrush(ApplicationConstants.Colors.DefaultHighlight);
        private double _displayScale = 1.0;
        private bool _isMouseVisible = true;

        /// <summary>
        /// 前景ブラシ
        /// </summary>
        public Brush ForegroundBrush => _foregroundBrush;
        
        /// <summary>
        /// アクティブブラシ
        /// </summary>
        public Brush ActiveBrush => _activeBrush;
        
        /// <summary>
        /// 表示スケール
        /// </summary>
        public double DisplayScale => _displayScale;
        
        /// <summary>
        /// マウス要素の可視性
        /// </summary>
        public bool IsMouseVisible => _isMouseVisible;

        /// <summary>
        /// 設定変更時のイベント
        /// </summary>
        public event EventHandler? SettingsChanged;

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

                // 色設定を適用
                ApplyColorSettings(settings);

                // その他の設定を適用
                _displayScale = settings.DisplayScale;
                _isMouseVisible = settings.IsMouseVisible;
                
                // プロファイル設定を適用（MainWindowが設定された後）
                // 背景色設定を適用
                ApplyBackgroundSettings(settings);
            }
            catch (Exception ex)
            {
                // 設定読み込みエラー時はデフォルト設定を使用
                System.Diagnostics.Debug.WriteLine($"設定読み込みエラー: {ex.Message}");
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
                // 現在の状態をAppSettingsに反映
                _settingsManager.Current.WindowLeft = _window.Left;
                _settingsManager.Current.WindowTop = _window.Top;
                _settingsManager.Current.IsTopmost = _window.Topmost;
                _settingsManager.Current.ForegroundColor = GetColorNameFromBrush(_foregroundBrush);
                _settingsManager.Current.HighlightColor = GetColorNameFromBrush(_activeBrush);
                _settingsManager.Current.BackgroundColor = GetColorNameFromBrush(_window.Background);
                _settingsManager.Current.DisplayScale = _displayScale;
                _settingsManager.Current.IsMouseVisible = _isMouseVisible;
                
                // MainWindowからプロファイル情報を取得して保存
                if (_window is MainWindow mainWindow && mainWindow.Input?.KeyboardHandler != null)
                {
                    _settingsManager.Current.CurrentProfile = mainWindow.Input.KeyboardHandler.CurrentProfile.ToString();
                }
                
                _settingsManager.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 背景色を設定
        /// </summary>
        public void SetBackgroundColor(Color color, bool transparent)
        {
            if (transparent)
            {
                _window.Background = BrushFactory.CreateTransparentBackground();
            }
            else
            {
                _window.Background = new SolidColorBrush(color);
            }
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 前景色を設定
        /// </summary>
        public void SetForegroundColor(Color color)
        {
            _foregroundBrush = new SolidColorBrush(color);
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// ハイライト色を設定
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            _activeBrush = new SolidColorBrush(color);
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 最前面表示を切り替え
        /// </summary>
        public void ToggleTopmost()
        {
            _window.Topmost = !_window.Topmost;
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// マウス可視性を切り替え
        /// </summary>
        public void ToggleMouseVisibility()
        {
            _isMouseVisible = !_isMouseVisible;
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        public void SetDisplayScale(double scale)
        {
            _displayScale = scale;
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
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
        /// 色設定を適用
        /// </summary>
        private void ApplyColorSettings(AppSettings settings)
        {
            // 前景色設定
            _foregroundBrush = GetBrushFromColorName(settings.ForegroundColor);

            // ハイライト色設定
            _activeBrush = GetBrushFromColorName(settings.HighlightColor);
        }

        /// <summary>
        /// 色名からBrushを取得
        /// </summary>
        private Brush GetBrushFromColorName(string colorName)
        {
            return BrushFactory.CreateBrushFromString(colorName, Brushes.White);
        }

        /// <summary>
        /// Brushから色名を取得
        /// </summary>
        private string GetColorNameFromBrush(Brush brush)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                
                // よく使用される色の名前を返す
                if (ColorsAreEqual(color, System.Windows.Media.Colors.White)) return "White";
                if (ColorsAreEqual(color, System.Windows.Media.Colors.Red)) return "Red";
                if (ColorsAreEqual(color, System.Windows.Media.Colors.Green)) return "Green";
                if (ColorsAreEqual(color, System.Windows.Media.Colors.Blue)) return "Blue";
                if (ColorsAreEqual(color, System.Windows.Media.Colors.Yellow)) return "Yellow";
                if (ColorsAreEqual(color, System.Windows.Media.Colors.Orange)) return "Orange";
                if (ColorsAreEqual(color, System.Windows.Media.Colors.Purple)) return "Purple";
                if (ColorsAreEqual(color, System.Windows.Media.Colors.Pink)) return "Pink";
                if (ColorsAreEqual(color, ApplicationConstants.Colors.DefaultHighlight)) return "LimeGreen";
                
                // RGB形式で返す
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            
            return "White";
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
                var backgroundBrush = GetBrushFromColorName(settings.BackgroundColor);
                _window.Background = backgroundBrush;
            }
        }

        /// <summary>
        /// 色の比較（アルファ値を無視）
        /// </summary>
        private static bool ColorsAreEqual(Color color1, Color color2)
        {
            return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B;
        }
    }
}