using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Layout;
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
        private readonly LayoutManager _layoutManager;
        private readonly ProfileManager _profileManager;

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
        /// ProfileSwitcherを設定（後から注入）
        /// </summary>
        public ProfileSwitcher? ProfileSwitcher { get; set; }

        /// <summary>
        /// 設定変更時のイベント
        /// </summary>
        public event EventHandler? SettingsChanged
        {
            add => _settingsManager.SettingsChanged += value;
            remove => _settingsManager.SettingsChanged -= value;
        }

        public MainWindowSettings(Window window, SettingsManager settingsManager, LayoutManager layoutManager, ProfileManager profileManager)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
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
            UpdateAllTextForeground();
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
            UpdateMouseVisibility();
        }

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        public void SetDisplayScale(double scale)
        {
            _settingsManager.SetDisplayScale(scale);
            ApplyDisplayScale();
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

        /// <summary>
        /// 全てのテキスト要素の前景色を更新（設定変更なし）
        /// </summary>
        public void UpdateAllTextForeground()
        {
            var canvas = _window.Content as Canvas;
            if (canvas != null)
            {
                CanvasElementHelper.ForEachBorderElement(canvas, UpdateBorderTextForeground);
            }
        }

        /// <summary>
        /// Border内のテキスト要素の前景色を更新
        /// </summary>
        private void UpdateBorderTextForeground(Border border)
        {
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Foreground = ForegroundBrush;
            }
            else if (border.Child is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = ForegroundBrush;
                    }
                }
            }
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
                var transform = new ScaleTransform(DisplayScale, DisplayScale);
                canvas.RenderTransform = transform;
                
                // YAMLファイルからウィンドウサイズを取得
                var (baseWidth, baseHeight) = _layoutManager.GetWindowSize(IsMouseVisible);
                
                _window.Width = baseWidth * DisplayScale;
                _window.Height = baseHeight * DisplayScale;
            }
        }

        /// <summary>
        /// マウス表示状態の変更を反映
        /// </summary>
        private void UpdateMouseVisibility()
        {
            var canvas = _window.Content as Canvas;
            if (canvas != null)
            {
                // Canvas内のマウス要素の可視性を直接制御
                CanvasElementHelper.ForEachMatchingElement(canvas, 
                    element => IsMouseElement(element.Name),
                    element => CanvasElementHelper.SetVisibility(element, IsMouseVisible));
                
                // ウィンドウサイズをマウス可視性に応じて調整
                var (baseWidth, baseHeight) = _layoutManager.GetWindowSize(IsMouseVisible);
                _window.Width = baseWidth * DisplayScale;
                _window.Height = baseHeight * DisplayScale;
            }
        }
        
        /// <summary>
        /// 指定された要素名がマウス要素かどうかを判定
        /// </summary>
        private bool IsMouseElement(string? elementName)
        {
            return MouseElementManager.IsMouseElement(elementName ?? string.Empty);
        }

        /// <summary>
        /// プロファイルを切り替え
        /// </summary>
        public void SwitchProfile(KeyboardProfile profile)
        {
            ProfileSwitcher?.SwitchProfile(profile);
        }

        /// <summary>
        /// 設定オーバーライドを適用
        /// レイアウトYAMLファイルの設定値をsettings.yamlの設定値でオーバーライドする
        /// currentProfile以外の全ての設定項目を対象とする
        /// </summary>
        public void ApplySettingsOverride()
        {
            var settings = _settingsManager.Current;
            
            // マウス可視性のオーバーライド
            UpdateMouseVisibility();
            
            // 表示スケールのオーバーライド
            ApplyDisplayScale();
            
            // 色設定のオーバーライド
            UpdateAllTextForeground();
            
            // ウィンドウ設定のオーバーライド
            ApplyWindowSettings(settings);
            
            // 背景色設定のオーバーライド
            ApplyBackgroundSettings(settings);
        }
    }
}