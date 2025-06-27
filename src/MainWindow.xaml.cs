using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YamlDotNet.Serialization;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Colors;
using KeyOverlayFPS.Input;
using System.Windows.Shapes;

namespace KeyOverlayFPS
{
    // 定数定義
    public static class Constants
    {
        public const double TimerInterval = 16.67; // ms
        public const int ScrollDisplayFrames = 10;
        
        public static class WindowSizes
        {
            public const double FullKeyboardWidth = 580;
            public const double FullKeyboardHeight = 160;
            public const double FpsKeyboardWidth = 450;
            public const double FpsKeyboardWidthWithMouse = 520;
            public const double FpsKeyboardHeight = 160;
        }
        
        public static class ScaleOptions
        {
            public static readonly double[] Values = { 0.8, 1.0, 1.2, 1.5 };
            public static readonly string[] Labels = { "80%", "100%", "120%", "150%" };
        }
    }


    // 設定データクラス
    public class AppSettings
    {
        public string CurrentProfile { get; set; } = "FullKeyboard65";
        public double DisplayScale { get; set; } = 1.0;
        public bool IsMouseVisible { get; set; } = true;
        public bool IsTopmost { get; set; } = true;
        public string BackgroundColor { get; set; } = "Transparent";
        public string ForegroundColor { get; set; } = "White";
        public string HighlightColor { get; set; } = "Green";
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
    }


    public partial class MainWindow : Window
    {
        // キーボード入力ハンドラー
        private readonly KeyboardInputHandler _keyboardHandler = new KeyboardInputHandler();

        private readonly DispatcherTimer _timer;
        private Brush _activeBrush = new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
        private readonly Brush _inactiveBrush;
        private readonly Brush _keyboardKeyDefaultBrush;
        
        // UIエレメントキャッシュ
        private readonly Dictionary<string, FrameworkElement> _elementCache = new();
        // スクロール表示色（ハイライト色と同じ）
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private int _scrollUpTimer = 0;
        private int _scrollDownTimer = 0;
        
        // フォアグラウンド色管理
        private Brush _foregroundBrush = Brushes.White;
        
        // マウス表示切り替え
        private bool _isMouseVisible = true;
        
        // 表示スケール（0.8, 1.0, 1.2, 1.5）
        private double _displayScale = 1.0;
        
        // 統一設定システム（将来の拡張用として保持）
        // private readonly UnifiedSettingsManager _settingsManager = UnifiedSettingsManager.Instance;
        
        // 旧設定システム（移行期間用） - プロパティ化して統一設定に透明に橋渡し
        private readonly string _settingsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "KeyOverlayFPS", 
            "settings.yaml"
        );
        
        // 旧設定システム（安定動作用）
        private AppSettings _settings = new AppSettings();
        
        // マウス移動可視化
        private readonly MouseTracker _mouseTracker = new();
        private readonly Dictionary<MouseDirection, System.Windows.Shapes.Path> _directionIndicators = new();
        private DispatcherTimer? _directionHideTimer;
        private const double DIRECTION_HIGHLIGHT_DURATION = 100; // ミリ秒
        
        // マウス方向可視化の設定
        private const double MOUSE_DIRECTION_CIRCLE_RADIUS = 15.0; // 半径（調整可能）
        private const double MOUSE_DIRECTION_STROKE_THICKNESS = 3.0; // 線幅
        private const int MOUSE_DIRECTION_SEGMENTS = 32; // 分割数
        
        // マウス方向可視化のサイズ設定
        private const double MOUSE_DIRECTION_CANVAS_SIZE = MOUSE_DIRECTION_CIRCLE_RADIUS * 2; // Canvasサイズ

        public MainWindow()
        {
            // キーボードキーの背景ブラシを初期化（マウス本体と同じグラデーション）
            _keyboardKeyDefaultBrush = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(0x2A, 0x2A, 0x2A), 0),
                    new GradientStop(Color.FromRgb(0x1A, 0x1A, 0x1A), 1)
                },
                new Point(0, 0),
                new Point(1, 1)
            );
            
            _inactiveBrush = _keyboardKeyDefaultBrush;
            
            InitializeComponent();
            
            // 旧設定システムで初期化（安定動作）
            LoadLegacySettings();
            
            // コンテキストメニューを設定
            SetupContextMenu();
            
            // 設定を適用
            ApplyLegacySettings();
            
            // タイマー初期化
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Constants.TimerInterval)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // イベントハンドラー設定
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            this.MouseWheel += MainWindow_MouseWheel;
            
            // アプリケーション終了時に設定を保存
            Application.Current.Exit += (s, e) => SaveLegacySettings();
            
            // キーボードキーの背景色を初期化
            InitializeKeyboardKeyBackgrounds();
            
            // マウス移動可視化の初期化
            InitializeMouseVisualization();
        }
        
        private void InitializeKeyboardKeyBackgrounds()
        {
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            foreach (UIElement child in canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name) && !IsMouseElement(border.Name))
                {
                    // マウス要素以外のBorderの背景をキーボードキー用背景に設定
                    border.Background = _keyboardKeyDefaultBrush;
                }
            }
        }
        
        private void InitializeMouseVisualization()
        {
            var canvas = GetCachedElement<Canvas>("MouseDirectionCanvas");
            if (canvas == null) return;
            
            // Canvasサイズを設定（位置はUpdateMousePositions()で設定）
            canvas.Width = MOUSE_DIRECTION_CANVAS_SIZE;
            canvas.Height = MOUSE_DIRECTION_CANVAS_SIZE;
            
            // 基準円周を作成（常時表示）
            var baseCircle = new Ellipse
            {
                Name = "MouseDirectionBaseCircle",
                Width = MOUSE_DIRECTION_CANVAS_SIZE,
                Height = MOUSE_DIRECTION_CANVAS_SIZE,
                Stroke = Brushes.White,
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                Opacity = 0.3
            };
            Canvas.SetLeft(baseCircle, 0);
            Canvas.SetTop(baseCircle, 0);
            canvas.Children.Add(baseCircle);
            
            // 中心点を作成（常時表示）
            var centerPoint = new Ellipse
            {
                Name = "MouseDirectionCenterPoint",
                Width = 1,
                Height = 1,
                Fill = new SolidColorBrush(Color.FromRgb(255, 68, 68)), // #FF4444
                Opacity = 0.8
            };
            Canvas.SetLeft(centerPoint, MOUSE_DIRECTION_CIRCLE_RADIUS - 0.5);
            Canvas.SetTop(centerPoint, MOUSE_DIRECTION_CIRCLE_RADIUS - 0.5);
            canvas.Children.Add(centerPoint);
            
            // 32方向の円弧セグメントを動的に生成
            var directions = new MouseDirection[]
            {
                MouseDirection.East, MouseDirection.East_11_25, MouseDirection.EastNorthEast, MouseDirection.East_33_75,
                MouseDirection.NorthEast, MouseDirection.North_56_25, MouseDirection.NorthNorthEast, MouseDirection.North_78_75,
                MouseDirection.North, MouseDirection.North_101_25, MouseDirection.NorthNorthWest, MouseDirection.North_123_75,
                MouseDirection.NorthWest, MouseDirection.West_146_25, MouseDirection.WestNorthWest, MouseDirection.West_168_75,
                MouseDirection.West, MouseDirection.West_191_25, MouseDirection.WestSouthWest, MouseDirection.West_213_75,
                MouseDirection.SouthWest, MouseDirection.South_236_25, MouseDirection.SouthSouthWest, MouseDirection.South_258_75,
                MouseDirection.South, MouseDirection.South_281_25, MouseDirection.SouthSouthEast, MouseDirection.South_303_75,
                MouseDirection.SouthEast, MouseDirection.South_326_25, MouseDirection.EastSouthEast, MouseDirection.East_348_75
            };
            
            for (int i = 0; i < directions.Length; i++)
            {
                var direction = directions[i];
                var arc = CreateDirectionArc(direction, i);
                arc.Name = $"Direction{direction}";
                canvas.Children.Add(arc);
                _directionIndicators[direction] = arc;
            }
            
            // マウストラッカーのイベントハンドラを登録
            _mouseTracker.MouseMoved += OnMouseMoved;
            
            // 方向表示を非表示にするタイマーを初期化
            _directionHideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DIRECTION_HIGHLIGHT_DURATION)
            };
            _directionHideTimer.Tick += OnDirectionHideTimer_Tick;
        }
        
        private System.Windows.Shapes.Path CreateDirectionArc(MouseDirection direction, int segmentIndex)
        {
            var anglePerSegment = 360.0 / MOUSE_DIRECTION_SEGMENTS;
            var startAngle = segmentIndex * anglePerSegment;
            var endAngle = startAngle + anglePerSegment;
            
            // 角度をラジアンに変換（East=0度を3時方向に配置）
            var startRadians = startAngle * Math.PI / 180; // East=0度は3時方向
            var endRadians = endAngle * Math.PI / 180;
            
            // 円周上の開始点と終了点を計算
            var centerX = MOUSE_DIRECTION_CIRCLE_RADIUS;
            var centerY = MOUSE_DIRECTION_CIRCLE_RADIUS;
            
            var startX = centerX + MOUSE_DIRECTION_CIRCLE_RADIUS * Math.Cos(startRadians);
            var startY = centerY - MOUSE_DIRECTION_CIRCLE_RADIUS * Math.Sin(startRadians); // Y軸反転（WPF座標系）
            var endX = centerX + MOUSE_DIRECTION_CIRCLE_RADIUS * Math.Cos(endRadians);
            var endY = centerY - MOUSE_DIRECTION_CIRCLE_RADIUS * Math.Sin(endRadians); // Y軸反転（WPF座標系）
            
            // 円弧のPath要素を作成
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(startX, startY)
            };
            
            var arcSegment = new ArcSegment
            {
                Point = new Point(endX, endY),
                Size = new Size(MOUSE_DIRECTION_CIRCLE_RADIUS, MOUSE_DIRECTION_CIRCLE_RADIUS),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            };
            
            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
            
            return new System.Windows.Shapes.Path
            {
                Data = pathGeometry,
                Stroke = _activeBrush, // キーボードと同じハイライト色
                StrokeThickness = MOUSE_DIRECTION_STROKE_THICKNESS,
                Opacity = 0.0
            };
        }
        
        private void OnMouseMoved(object? sender, MouseMoveEventArgs e)
        {
            // 指定方向のインジケータをハイライト表示
            if (_directionIndicators.TryGetValue(e.Direction, out var indicator))
            {
                // 現在表示中の方向をリセット
                ResetDirectionIndicators();
                
                // 新しい方向をハイライト
                indicator.Opacity = 0.9;
                
                // タイマーをリセットして再開
                _directionHideTimer?.Stop();
                _directionHideTimer?.Start();
            }
        }
        
        private void OnDirectionHideTimer_Tick(object? sender, EventArgs e)
        {
            // 方向表示を非表示にする
            ResetDirectionIndicators();
            _directionHideTimer?.Stop();
        }
        
        private void ResetDirectionIndicators()
        {
            foreach (var indicator in _directionIndicators.Values)
            {
                indicator.Opacity = 0.0;
            }
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            contextMenu.Items.Add(CreateBackgroundColorMenu());
            contextMenu.Items.Add(CreateForegroundColorMenu());
            contextMenu.Items.Add(CreateHighlightColorMenu());
            contextMenu.Items.Add(CreateViewOptionsMenu());
            contextMenu.Items.Add(CreateProfileMenu());
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(CreateLayoutManagementMenu());
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(CreateExitMenu());
            
            ContextMenu = contextMenu;
        }
        
        private MenuItem CreateBackgroundColorMenu()
        {
            var backgroundMenuItem = new MenuItem { Header = "背景色" };
            
            foreach (var (name, color, transparent) in ColorManager.BackgroundMenuOptions)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetBackgroundColor(color, transparent);
                backgroundMenuItem.Items.Add(menuItem);
            }
            
            return backgroundMenuItem;
        }
        
        private MenuItem CreateForegroundColorMenu()
        {
            var foregroundMenuItem = new MenuItem { Header = "文字色" };
            
            foreach (var (name, color) in ColorManager.ForegroundMenuOptions)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetForegroundColor(color);
                foregroundMenuItem.Items.Add(menuItem);
            }
            
            return foregroundMenuItem;
        }
        
        private MenuItem CreateHighlightColorMenu()
        {
            var highlightMenuItem = new MenuItem { Header = "ハイライト色" };
            
            foreach (var (name, color) in ColorManager.HighlightMenuOptions)
            {
                var menuItem = new MenuItem { Header = name };
                menuItem.Click += (s, e) => SetHighlightColor(color);
                highlightMenuItem.Items.Add(menuItem);
            }
            
            return highlightMenuItem;
        }
        
        private MenuItem CreateViewOptionsMenu()
        {
            var viewMenuItem = new MenuItem { Header = "表示オプション" };
            
            var topmostMenuItem = new MenuItem { Header = "最前面固定", IsCheckable = true, IsChecked = _settings.IsTopmost };
            topmostMenuItem.Click += (s, e) => ToggleTopmost();
            
            var mouseVisibilityMenuItem = new MenuItem { Header = "マウス表示", IsCheckable = true, IsChecked = _settings.IsMouseVisible };
            mouseVisibilityMenuItem.Click += (s, e) => ToggleMouseVisibility();
            
            var scaleMenuItem = CreateDisplayScaleMenu();
            
            viewMenuItem.Items.Add(topmostMenuItem);
            viewMenuItem.Items.Add(mouseVisibilityMenuItem);
            viewMenuItem.Items.Add(scaleMenuItem);
            
            return viewMenuItem;
        }
        
        private MenuItem CreateDisplayScaleMenu()
        {
            var scaleMenuItem = new MenuItem { Header = "表示サイズ" };
            
            for (int i = 0; i < Constants.ScaleOptions.Values.Length; i++)
            {
                var scale = Constants.ScaleOptions.Values[i];
                var label = Constants.ScaleOptions.Labels[i];
                
                var menuItem = new MenuItem 
                { 
                    Header = label, 
                    IsCheckable = true, 
                    IsChecked = Math.Abs(_settings.DisplayScale - scale) < 0.01 
                };
                menuItem.Click += (s, e) => 
                {
                    SetDisplayScale(scale);
                    UpdateMenuCheckedState(scaleMenuItem, menuItem);
                };
                scaleMenuItem.Items.Add(menuItem);
            }
            
            return scaleMenuItem;
        }
        
        private MenuItem CreateProfileMenu()
        {
            var profileMenuItem = new MenuItem { Header = "プロファイル" };
            
            var profileOptions = new[]
            {
                ("65%キーボード", KeyboardProfile.FullKeyboard65, "FullKeyboard65"),
                ("FPSキーボード", KeyboardProfile.FPSKeyboard, "FPSKeyboard")
            };
            
            foreach (var (name, profile, settingName) in profileOptions)
            {
                var menuItem = new MenuItem 
                { 
                    Header = name, 
                    IsCheckable = true, 
                    IsChecked = _settings.CurrentProfile == settingName 
                };
                menuItem.Click += (s, e) => 
                {
                    SwitchProfile(profile);
                    UpdateMenuCheckedState(profileMenuItem, menuItem);
                };
                profileMenuItem.Items.Add(menuItem);
            }
            
            return profileMenuItem;
        }
        
        private MenuItem CreateLayoutManagementMenu()
        {
            var layoutMenuItem = new MenuItem { Header = "レイアウト管理" };
            layoutMenuItem.Click += (s, e) => OpenLayoutEditor();
            return layoutMenuItem;
        }
        
        private void OpenLayoutEditor()
        {
            try
            {
                var layoutEditor = new KeyOverlayFPS.Layout.LayoutEditorWindow();
                var result = layoutEditor.ShowDialog();
                
                if (result == true && layoutEditor.Result != null)
                {
                    // レイアウトを実際のUIに適用
                    if (ApplyLayoutToMainWindow(layoutEditor.Result))
                    {
                        MessageBox.Show("レイアウトが適用されました。", "適用完了", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("レイアウトの適用中にエラーが発生しました。デフォルト設定に戻します。", "適用エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"レイアウトエディターの起動に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// レイアウト設定をMainWindowに適用
        /// </summary>
        /// <param name="layout">適用するレイアウト設定</param>
        /// <returns>成功時true、失敗時false</returns>
        private bool ApplyLayoutToMainWindow(KeyOverlayFPS.Layout.LayoutConfig layout)
        {
            if (layout == null) return false;
            
            try
            {
                // 1. タイマーを一時停止（スレッドセーフのため）
                bool wasTimerEnabled = _timer.IsEnabled;
                if (wasTimerEnabled)
                {
                    _timer.Stop();
                }
                
                // 2. ウィンドウサイズを適用
                if (layout.Global != null)
                {
                    Width = layout.Global.WindowWidth;
                    Height = layout.Global.WindowHeight;
                }
                
                // 3. 各要素の設定を適用
                if (layout.Elements != null && layout.Global != null)
                {
                    foreach (var element in layout.Elements)
                    {
                        var elementName = element.Key;
                        var config = element.Value;
                        
                        if (!ApplyElementConfig(elementName, config, layout.Global))
                        {
                            // 個別要素の適用に失敗した場合でも続行
                            System.Diagnostics.Debug.WriteLine($"Warning: Failed to apply config for element: {elementName}");
                        }
                    }
                }
                
                // 4. タイマーを再開
                if (wasTimerEnabled)
                {
                    _timer.Start();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying layout: {ex.Message}");
                
                // エラー時もタイマーを確実に再開
                if (!_timer.IsEnabled)
                {
                    _timer.Start();
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// 個別要素にレイアウト設定を適用
        /// </summary>
        /// <param name="elementName">要素名</param>
        /// <param name="config">要素設定</param>
        /// <param name="globalSettings">グローバル設定</param>
        /// <returns>成功時true、失敗時false</returns>
        private bool ApplyElementConfig(string elementName, KeyOverlayFPS.Layout.ElementConfig config, KeyOverlayFPS.Layout.GlobalSettings globalSettings)
        {
            if (config == null || globalSettings == null) return false;
            
            try
            {
                // Border要素を取得
                var borderElement = FindName(elementName) as Border;
                if (borderElement == null) return false;
                
                // 表示/非表示を設定
                borderElement.Visibility = config.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                
                // 位置を設定
                Canvas.SetLeft(borderElement, config.X);
                Canvas.SetTop(borderElement, config.Y);
                
                // サイズを設定（個別設定優先、なければグローバル設定）
                var size = config.Size ?? globalSettings.KeySize;
                if (size != null)
                {
                    borderElement.Width = size.Width;
                    borderElement.Height = size.Height;
                }
                
                // TextBlock要素を取得してテキスト・フォント設定を適用
                var textElement = FindName(elementName + "Text") as TextBlock;
                if (textElement != null)
                {
                    // テキストを設定
                    if (!string.IsNullOrEmpty(config.Text))
                    {
                        textElement.Text = config.Text;
                    }
                    
                    // フォントサイズを設定（個別設定優先、なければグローバル設定）
                    var fontSize = config.FontSize ?? globalSettings.FontSize;
                    textElement.FontSize = fontSize;
                    
                    // フォントファミリーを設定
                    if (!string.IsNullOrEmpty(globalSettings.FontFamily))
                    {
                        textElement.FontFamily = new System.Windows.Media.FontFamily(globalSettings.FontFamily);
                    }
                    
                    // 前景色を設定
                    if (!string.IsNullOrEmpty(globalSettings.ForegroundColor))
                    {
                        try
                        {
                            var color = (Color)ColorConverter.ConvertFromString(globalSettings.ForegroundColor);
                            textElement.Foreground = new SolidColorBrush(color);
                        }
                        catch
                        {
                            // 色変換エラー時はデフォルトを使用
                            textElement.Foreground = Brushes.White;
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying element config for {elementName}: {ex.Message}");
                return false;
            }
        }
        
        private MenuItem CreateExitMenu()
        {
            var exitMenuItem = new MenuItem { Header = "終了" };
            exitMenuItem.Click += (s, e) => Application.Current.Shutdown();
            return exitMenuItem;
        }
        
        private void SetBackgroundColor(Color color, bool transparent)
        {
            if (transparent)
            {
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
            else
            {
                Background = new SolidColorBrush(color);
            }
            // 背景色を設定に更新
            _settings.BackgroundColor = GetDirectBackgroundColorName(color);
            SaveLegacySettings();
        }
        
        private void SetForegroundColor(Color color)
        {
            _foregroundBrush = new SolidColorBrush(color);
            UpdateAllTextForeground();
            SaveLegacySettings();
        }
        
        private void SetHighlightColor(Color color)
        {
            _activeBrush = new SolidColorBrush(color);
            SaveLegacySettings();
        }
        
        private void ToggleTopmost()
        {
            Topmost = !Topmost;
            SaveLegacySettings();
        }
        
        private void ToggleMouseVisibility()
        {
            _isMouseVisible = !_isMouseVisible;
            UpdateMouseVisibility();
            SaveLegacySettings();
        }
        
        private void UpdateMouseVisibility()
        {
            // プロファイルレイアウトを再適用してマウス表示状態を反映
            ApplyProfileLayout();
        }
        
        private void SetDisplayScale(double scale)
        {
            _displayScale = scale;
            ApplyDisplayScale();
            SaveLegacySettings();
        }
        
        private void ApplyDisplayScale()
        {
            var canvas = Content as Canvas;
            if (canvas != null)
            {
                // Canvas全体にスケール変換を適用
                var transform = new ScaleTransform(_displayScale, _displayScale);
                canvas.RenderTransform = transform;
                
                // プロファイルに応じたウィンドウサイズ調整
                double baseWidth, baseHeight;
                if (_keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard)
                {
                    baseWidth = _isMouseVisible ? Constants.WindowSizes.FpsKeyboardWidthWithMouse : Constants.WindowSizes.FpsKeyboardWidth;
                    baseHeight = Constants.WindowSizes.FpsKeyboardHeight;
                }
                else
                {
                    baseWidth = Constants.WindowSizes.FullKeyboardWidth;
                    baseHeight = Constants.WindowSizes.FullKeyboardHeight;
                }
                
                Width = baseWidth * _displayScale;
                Height = baseHeight * _displayScale;
            }
        }
        
        private static void UpdateMenuCheckedState(MenuItem parentMenu, MenuItem selectedItem)
        {
            foreach (MenuItem item in parentMenu.Items.OfType<MenuItem>())
            {
                if (item.IsCheckable)
                {
                    item.IsChecked = item == selectedItem;
                }
            }
        }
        
        private void SwitchProfile(KeyboardProfile profile)
        {
            _keyboardHandler.CurrentProfile = profile;
            ApplyProfileLayout();
            UpdateMousePositions();
            SaveLegacySettings();
        }
        
        private void ApplyProfileLayout()
        {
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            switch (_keyboardHandler.CurrentProfile)
            {
                case KeyboardProfile.FullKeyboard65:
                    ShowFullKeyboardLayout();
                    // ウィンドウサイズ調整
                    Width = Constants.WindowSizes.FullKeyboardWidth * _displayScale;
                    Height = Constants.WindowSizes.FullKeyboardHeight * _displayScale;
                    break;
                    
                case KeyboardProfile.FPSKeyboard:
                    ShowFPSKeyboardLayout();
                    // ウィンドウサイズ調整（FPS用サイズ、マウス表示考慮）
                    Width = (_isMouseVisible ? Constants.WindowSizes.FpsKeyboardWidthWithMouse : Constants.WindowSizes.FpsKeyboardWidth) * _displayScale;
                    Height = Constants.WindowSizes.FpsKeyboardHeight * _displayScale;
                    break;
            }
        }
        
        private void ShowFullKeyboardLayout()
        {
            // 全キーを表示
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            foreach (UIElement child in canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name))
                {
                    // マウス要素は現在の設定に従う
                    if (IsMouseElement(border.Name))
                    {
                        border.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        border.Visibility = Visibility.Visible;
                    }
                }
                else if (child is Canvas childCanvas && childCanvas.Name == "MouseDirectionCanvas")
                {
                    // マウス移動可視化キャンバス
                    child.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    // マウス本体など名前なし要素の処理
                    child.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        
        private void ShowFPSKeyboardLayout()
        {
            // FPSに重要なキーのみ表示
            var fpsKeys = KeyboardInputHandler.GetProfileKeyElements(KeyboardProfile.FPSKeyboard);
            
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            foreach (UIElement child in canvas.Children)
            {
                if (child is Border border && !string.IsNullOrEmpty(border.Name))
                {
                    if (IsMouseElement(border.Name))
                    {
                        // マウス要素は設定に従う
                        border.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else if (fpsKeys.Contains(border.Name))
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
                else if (child is Canvas childCanvas && childCanvas.Name == "MouseDirectionCanvas")
                {
                    // マウス移動可視化キャンバス
                    child.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    // マウス本体など名前なし要素
                    child.Visibility = _isMouseVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        
        // マウス要素管理クラス
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
                { "MouseButton4", (0, 42) },
                { "MouseButton5", (0, 64) },
                { "ScrollUp", (35, 10) },
                { "ScrollDown", (35, 24) },
                { "MouseDirectionCanvas", (15, 50) } // マウス本体中央下に配置
            };
        }
        
        private static bool IsMouseElement(string elementName) => MouseElements.Names.Contains(elementName);
        
        /// <summary>
        /// キャッシュ付きでUIエレメントを取得
        /// </summary>
        private T? GetCachedElement<T>(string name) where T : FrameworkElement
        {
            if (!_elementCache.TryGetValue(name, out var element))
            {
                element = FindName(name) as T;
                if (element != null)
                {
                    _elementCache[name] = element;
                }
            }
            return element as T;
        }
        
        private void UpdateMousePositions()
        {
            var position = _keyboardHandler.GetMousePosition(_keyboardHandler.CurrentProfile);
            
            // 全マウス要素の位置を一括更新
            foreach (var (elementName, offset) in MouseElements.Offsets)
            {
                var element = GetCachedElement<FrameworkElement>(elementName);
                if (element != null)
                {
                    Canvas.SetLeft(element, position.Left + offset.Left);
                    Canvas.SetTop(element, position.Top + offset.Top);
                }
            }
        }
        
        // InitializeSettingsSyncメソッドは統一設定システム用のため一時的に無効化（LoadLegacySettingsを使用）
        
        // SyncLegacySettingsFromUnifiedメソッドは削除 - _settingsプロパティが直接統一設定を参照するため不要
        
        // OnSettingsChangedメソッドは統一設定システム用のため一時的に無効化
        
        // LoadSettingsメソッドは統一設定システム用のため一時的に無効化（LoadLegacySettingsを使用）
        
        // SaveSettingsメソッドは統一設定システム用のため一時的に無効化（SaveLegacySettingsを使用）
        
        // UpdateSettingsDirectlyメソッドは統一設定システム用のため一時的に無効化
        
        // ApplyWindowSettingsメソッドは統一設定システム用のため一時的に無効化
        
        // ApplyDisplaySettingsメソッドは統一設定システム用のため一時的に無効化
        
        // ApplyColorSettingsメソッドは統一設定システム用のため一時的に無効化
        
        // ApplyProfileSettingsメソッドは統一設定システム用のため一時的に無効化
        
        // ApplySettingsメソッドは統一設定システム用のため一時的に無効化（ApplyLegacySettingsを使用）
        
        /// <summary>
        /// 循環参照を防ぐための直接色取得メソッド
        /// </summary>
        private Brush GetDirectForegroundBrush(string colorName)
        {
            var defaultColors = new Dictionary<string, string>
            {
                { "White", "#FFFFFF" },
                { "Black", "#000000" },
                { "Gray", "#808080" },
                { "Blue", "#0000FF" },
                { "Green", "#008000" },
                { "Red", "#FF0000" },
                { "Yellow", "#FFFF00" }
            };
            
            if (defaultColors.TryGetValue(colorName, out var colorValue))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorValue);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return Brushes.White;
                }
            }
            return Brushes.White;
        }
        
        private Brush GetDirectHighlightBrush(string colorName)
        {
            var defaultColors = new Dictionary<string, string>
            {
                { "White", "#B4FFFFFF" },
                { "Black", "#B4000000" },
                { "Gray", "#B4808080" },
                { "Blue", "#B40000FF" },
                { "Green", "#B4008000" },
                { "Red", "#B4FF0000" },
                { "Yellow", "#B4FFFF00" }
            };
            
            if (defaultColors.TryGetValue(colorName, out var colorValue))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorValue);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
                }
            }
            return new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
        }
        
        private Color GetDirectBackgroundColor(string colorName)
        {
            var defaultColors = new Dictionary<string, string>
            {
                { "Transparent", "Transparent" },
                { "Lime", "#00FF00" },
                { "Blue", "#0000FF" },
                { "Black", "#000000" }
            };
            
            if (defaultColors.TryGetValue(colorName, out var colorValue))
            {
                if (colorValue.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                    return System.Windows.Media.Colors.Transparent;
                    
                try
                {
                    return (Color)ColorConverter.ConvertFromString(colorValue);
                }
                catch
                {
                    return System.Windows.Media.Colors.Transparent;
                }
            }
            return System.Windows.Media.Colors.Transparent;
        }
        
        private string GetDirectColorName(Brush brush, string type)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                
                // 最も近い色を探す
                if (type == "Foreground")
                {
                    if (color == System.Windows.Media.Colors.White) return "White";
                    if (color == System.Windows.Media.Colors.Black) return "Black";
                    if (color == System.Windows.Media.Colors.Gray) return "Gray";
                    if (color == System.Windows.Media.Colors.Blue) return "Blue";
                    if (color == System.Windows.Media.Colors.Green) return "Green";
                    if (color == System.Windows.Media.Colors.Red) return "Red";
                    if (color == System.Windows.Media.Colors.Yellow) return "Yellow";
                }
                else // Highlight
                {
                    // アルファ値を考慮して一番近い色を探す
                    if (color.A == 180 && color.R == 0 && color.G == 255 && color.B == 0) return "Green";
                    if (color.A == 180 && color.R == 255 && color.G == 255 && color.B == 255) return "White";
                    if (color.A == 180 && color.R == 0 && color.G == 0 && color.B == 0) return "Black";
                    if (color.A == 180 && color.R == 128 && color.G == 128 && color.B == 128) return "Gray";
                    if (color.A == 180 && color.R == 0 && color.G == 0 && color.B == 255) return "Blue";
                    if (color.A == 180 && color.R == 255 && color.G == 0 && color.B == 0) return "Red";
                    if (color.A == 180 && color.R == 255 && color.G == 255 && color.B == 0) return "Yellow";
                }
            }
            
            return type == "Foreground" ? "White" : "Green";
        }
        
        private string GetDirectBackgroundColorName(Color color)
        {
            if (color == System.Windows.Media.Colors.Transparent) return "Transparent";
            if (color == System.Windows.Media.Color.FromRgb(0, 255, 0)) return "Lime";
            if (color == System.Windows.Media.Colors.Blue) return "Blue";
            if (color == System.Windows.Media.Colors.Black) return "Black";
            
            return "Transparent";
        }
        
        /// <summary>
        /// 旧設定システムの読み込み（安定動作用）
        /// </summary>
        private void LoadLegacySettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var yaml = File.ReadAllText(_settingsPath);
                    var deserializer = new DeserializerBuilder().Build();
                    _settings = deserializer.Deserialize<AppSettings>(yaml) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定読み込みエラー: {ex.Message}");
                _settings = new AppSettings();
            }
        }
        
        /// <summary>
        /// 旧設定システムの保存（安定動作用）
        /// </summary>
        private void SaveLegacySettings()
        {
            try
            {
                // 現在の設定を保存用データに反映
                _settings.CurrentProfile = _keyboardHandler.CurrentProfile.ToString();
                _settings.DisplayScale = _displayScale;
                _settings.IsMouseVisible = _isMouseVisible;
                _settings.IsTopmost = Topmost;
                _settings.WindowLeft = Left;
                _settings.WindowTop = Top;
                
                // 色設定の保存
                _settings.ForegroundColor = GetDirectColorName(_foregroundBrush, "Foreground");
                _settings.HighlightColor = GetDirectColorName(_activeBrush, "Highlight");
                
                // ディレクトリが存在しない場合は作成
                var directory = System.IO.Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // YAML形式で保存
                var serializer = new SerializerBuilder().Build();
                var yaml = serializer.Serialize(_settings);
                File.WriteAllText(_settingsPath, yaml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 旧設定システムの適用（安定動作用）
        /// </summary>
        private void ApplyLegacySettings()
        {
            // プロファイル設定
            if (Enum.TryParse<KeyboardProfile>(_settings.CurrentProfile, out var profile))
            {
                _keyboardHandler.CurrentProfile = profile;
            }
            
            // 表示設定
            _displayScale = _settings.DisplayScale;
            _isMouseVisible = _settings.IsMouseVisible;
            Topmost = _settings.IsTopmost;
            
            // ウィンドウ位置設定
            if (_settings.WindowLeft > 0 && _settings.WindowTop > 0)
            {
                Left = _settings.WindowLeft;
                Top = _settings.WindowTop;
            }
            
            // 色設定
            _foregroundBrush = GetDirectForegroundBrush(_settings.ForegroundColor);
            _activeBrush = GetDirectHighlightBrush(_settings.HighlightColor);
            
            // 背景色設定を適用
            var color = GetDirectBackgroundColor(_settings.BackgroundColor ?? "Transparent");
            bool transparent = color == System.Windows.Media.Colors.Transparent;
            SetBackgroundColor(color, transparent);
            
            // レイアウトとスケールを適用
            ApplyProfileLayout();
            ApplyDisplayScale();
            UpdateMousePositions();
            UpdateAllTextForeground();
        }
        
        // ApplyBackgroundColorFromSettingsメソッドは統一設定システム用のため一時的に無効化
        
        
        private void UpdateAllTextForeground()
        {
            // Canvas内のすべてのTextBlockを探してフォアグラウンド色を更新
            var canvas = Content as Canvas;
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
        
        private void UpdateBorderTextForeground(Border border)
        {
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Foreground = _foregroundBrush;
            }
            else if (border.Child is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = _foregroundBrush;
                    }
                }
            }
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            CaptureMouse();
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var screen = PointToScreen(currentPosition);
                var window = PointToScreen(_dragStartPoint);
                
                Left = screen.X - window.X + Left;
                Top = screen.Y - window.Y + Top;
            }
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bool isShiftPressed = _keyboardHandler.IsShiftPressed();
            
            // キーボードキー更新
            UpdateKeys(isShiftPressed);
            
            // マウス入力（表示時のみ更新）
            if (_isMouseVisible)
            {
                UpdateMouseKeys();
                UpdateScrollIndicators();
                
                // マウス移動追跡（5.0は閾値）
                _mouseTracker.Update(5.0);
            }
        }
        
        private void UpdateKeys(bool isShiftPressed)
        {
            var activeKeys = KeyboardInputHandler.GetProfileKeyElements(_keyboardHandler.CurrentProfile);
            bool shouldShowShiftText = isShiftPressed && _keyboardHandler.IsShiftDisplayEnabled(_keyboardHandler.CurrentProfile);
            
            foreach (var keyName in activeKeys)
            {
                var config = _keyboardHandler.GetKeyConfig(keyName);
                if (config != null)
                {
                    if (config.HasShiftVariant)
                    {
                        UpdateKeyStateWithShift(config.Name, config.VirtualKey, config.NormalText, config.ShiftText, shouldShowShiftText);
                    }
                    else
                    {
                        UpdateKeyStateByName(config.Name, config.VirtualKey);
                    }
                }
            }
        }
        
        private void UpdateMouseKeys()
        {
            var mouseKeys = new[]
            {
                ("MouseLeft", 1),
                ("MouseRight", 2),
                ("MouseWheelButton", 3),
                ("MouseButton4", 5),
                ("MouseButton5", 4)
            };
            
            foreach (var (keyName, buttonId) in mouseKeys)
            {
                UpdateKeyStateByName(keyName, buttonId);
            }
        }
        


        private void UpdateKeyStateByName(string keyName, int keyCode)
        {
            if (keyCode == 0) return; // Fnキーなど検出不可のキー
            
            var keyBorder = GetCachedElement<Border>(keyName);
            if (keyBorder != null)
            {
                bool isPressed;
                if (keyName.StartsWith("Mouse"))
                {
                    // マウスボタンの場合
                    isPressed = _keyboardHandler.IsMouseButtonPressed(keyCode);
                }
                else
                {
                    // キーボードキーの場合
                    isPressed = KeyboardInputHandler.IsKeyPressed(keyCode);
                }
                keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
            }
        }

        private void UpdateKeyStateWithShift(string keyName, int virtualKeyCode, string normalText, string shiftText, bool isShiftPressed)
        {
            var keyBorder = GetCachedElement<Border>(keyName);
            var textBlock = GetCachedElement<TextBlock>(keyName + "Text");
            
            if (keyBorder != null && textBlock != null)
            {
                bool isPressed = KeyboardInputHandler.IsKeyPressed(virtualKeyCode);
                keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
                
                // プロファイルのShift表示設定を確認
                bool shouldShowShiftText = isShiftPressed && _keyboardHandler.IsShiftDisplayEnabled(_keyboardHandler.CurrentProfile);
                textBlock.Text = shouldShowShiftText ? shiftText : normalText;
            }
        }


        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = true;
            }
        }

        private void KeyBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            CaptureMouse();
            e.Handled = true;
        }

        private void KeyBorder_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = true;
            }
            e.Handled = true;
        }
        
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isMouseVisible)
            {
                // スクロール表示（マウス表示時のみ）
                if (e.Delta > 0)
                {
                    // 上スクロール
                    _scrollUpTimer = Constants.ScrollDisplayFrames;
                }
                else if (e.Delta < 0)
                {
                    // 下スクロール
                    _scrollDownTimer = Constants.ScrollDisplayFrames;
                }
            }
        }
        
        private void UpdateScrollIndicators()
        {
            // スクロールアップ表示
            var scrollUpIndicator = GetCachedElement<TextBlock>("ScrollUpIndicator");
            if (scrollUpIndicator != null)
            {
                if (_scrollUpTimer > 0)
                {
                    scrollUpIndicator.Foreground = _activeBrush; // ハイライト色を使用
                    _scrollUpTimer--;
                }
                else
                {
                    scrollUpIndicator.Foreground = _inactiveBrush;
                }
            }
            
            // スクロールダウン表示
            var scrollDownIndicator = GetCachedElement<TextBlock>("ScrollDownIndicator");
            if (scrollDownIndicator != null)
            {
                if (_scrollDownTimer > 0)
                {
                    scrollDownIndicator.Foreground = _activeBrush; // ハイライト色を使用
                    _scrollDownTimer--;
                }
                else
                {
                    scrollDownIndicator.Foreground = _inactiveBrush;
                }
            }
        }
    }
}
