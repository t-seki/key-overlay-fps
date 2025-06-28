using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.UI;
using System.Windows.Shapes;

namespace KeyOverlayFPS
{
    // 定数はApplicationConstants.csに移動済み


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
        
        // 旧設定システム（移行期間用）
        private readonly string _settingsPath = ApplicationConstants.Paths.LegacySettingsFile;
        
        // 旧設定システム（安定動作用）
        private AppSettings _settings = new AppSettings();
        
        // マウス移動可視化
        private readonly MouseTracker _mouseTracker = new();
        private readonly Dictionary<MouseDirection, System.Windows.Shapes.Path> _directionIndicators = new();
        private DispatcherTimer? _directionHideTimer;
        
        // 動的レイアウトシステム
        private LayoutConfig? _currentLayout;
        private KeyEventBinder? _eventBinder;
        private Canvas? _dynamicCanvas;
        
        // マウス方向可視化設定はApplicationConstants.MouseVisualizationに移動済み

        public MainWindow()
        {
            // ブラシを統一ファクトリーから初期化
            _keyboardKeyDefaultBrush = BrushFactory.CreateKeyboardKeyBackground();
            _inactiveBrush = _keyboardKeyDefaultBrush;
            
            InitializeComponent();
            
            // 旧設定システムで初期化（安定動作）
            LoadLegacySettings();
            
            // 動的レイアウトシステムを初期化
            InitializeDynamicLayoutSystem();
            
            // コンテキストメニューを設定
            SetupContextMenu();
            
            // タイマー初期化
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.MainTimerInterval)
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
            
            // 設定を適用（動的レイアウトシステムが初期化された後）
            ApplyLegacySettings();
        }
        
        /// <summary>
        /// 動的レイアウトシステムの初期化
        /// </summary>
        private void InitializeDynamicLayoutSystem()
        {
            try
            {
                // プロファイルに応じたレイアウトファイルを読み込み
                string layoutPath = GetLayoutPath(_keyboardHandler.CurrentProfile);
                
                if (File.Exists(layoutPath))
                {
                    _currentLayout = LayoutManager.ImportLayout(layoutPath);
                }
                else
                {
                    // デフォルトレイアウトを使用
                    _currentLayout = _keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard 
                        ? LayoutManager.CreateDefaultFPSLayout()
                        : LayoutManager.CreateDefault65KeyboardLayout();
                }
                
                // UIを動的生成
                _dynamicCanvas = UIGenerator.GenerateCanvas(_currentLayout, this);
                
                // 既存のCanvasと置き換え
                Content = _dynamicCanvas;
                
                // ウィンドウ背景を設定
                Background = BrushFactory.CreateTransparentBackground();
                
                // 要素名を登録
                UIGenerator.RegisterElementNames(_dynamicCanvas, this);
                
                // イベントバインディング
                _eventBinder = new KeyEventBinder(_dynamicCanvas, _currentLayout, _keyboardHandler, _mouseTracker);
                _eventBinder.BindAllEvents();
                
                // マウス移動イベントの登録
                _mouseTracker.MouseMoved += OnMouseMoved;
                
                // 方向インジケーターのキャッシュを構築
                BuildDirectionIndicatorCache();
                
                // 方向表示を非表示にするタイマーを初期化
                _directionHideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.DirectionHideDelay)
                };
                _directionHideTimer.Tick += OnDirectionHideTimer_Tick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"動的レイアウトシステム初期化エラー: {ex.Message}");
                // エラー時はデフォルトレイアウトにフォールバック
                _currentLayout = LayoutManager.CreateDefault65KeyboardLayout();
                _dynamicCanvas = UIGenerator.GenerateCanvas(_currentLayout, this);
                Content = _dynamicCanvas;
            }
        }
        
        /// <summary>
        /// プロファイルに応じたレイアウトファイルパスを取得
        /// </summary>
        private string GetLayoutPath(KeyboardProfile profile)
        {
            return profile switch
            {
                KeyboardProfile.FPSKeyboard => ApplicationConstants.Paths.FpsLayout,
                _ => ApplicationConstants.Paths.Keyboard65Layout
            };
        }
        
        /// <summary>
        /// 方向インジケーターのキャッシュを構築
        /// </summary>
        private void BuildDirectionIndicatorCache()
        {
            var directionCanvas = _eventBinder?.FindElement<Canvas>("MouseDirectionCanvas");
            if (directionCanvas == null) return;

            _directionIndicators.Clear();
            
            foreach (UIElement child in directionCanvas.Children)
            {
                if (child is System.Windows.Shapes.Path path && path.Name?.StartsWith("Direction") == true)
                {
                    var directionName = path.Name.Substring("Direction".Length);
                    if (Enum.TryParse<MouseDirection>(directionName, out var direction))
                    {
                        _directionIndicators[direction] = path;
                    }
                }
            }
        }

        /// <summary>
        /// マウス移動イベントハンドラー（新バージョン）
        /// </summary>
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

        /// <summary>
        /// 方向インジケーター非表示タイマー
        /// </summary>
        private void OnDirectionHideTimer_Tick(object? sender, EventArgs e)
        {
            // 方向表示を非表示にする
            ResetDirectionIndicators();
            _directionHideTimer?.Stop();
        }

        /// <summary>
        /// 方向インジケーターをリセット
        /// </summary>
        private void ResetDirectionIndicators()
        {
            foreach (var indicator in _directionIndicators.Values)
            {
                indicator.Opacity = 0.0;
            }
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
            
            // Canvasサイズを設定
            canvas.Width = ApplicationConstants.MouseVisualization.CanvasSize;
            canvas.Height = ApplicationConstants.MouseVisualization.CanvasSize;
            
            // 基準円と中心点を作成
            DirectionArcGenerator.CreateBaseCircleAndCenter(canvas);
            
            // 32方向の円弧を一括生成
            var indicators = DirectionArcGenerator.CreateAllDirectionArcs(canvas);
            foreach (var (direction, path) in indicators)
            {
                _directionIndicators[direction] = path;
            }
            
            // マウストラッカーのイベントハンドラを登録
            _mouseTracker.MouseMoved += OnMouseMoved;
            
            // 方向表示を非表示にするタイマーを初期化
            _directionHideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.DirectionHideDelay)
            };
            _directionHideTimer.Tick += OnDirectionHideTimer_Tick;
        }
        
        // CreateDirectionArcメソッドはDirectionArcGenerator.csに移動済み

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            contextMenu.Items.Add(CreateBackgroundColorMenu());
            contextMenu.Items.Add(CreateForegroundColorMenu());
            contextMenu.Items.Add(CreateHighlightColorMenu());
            contextMenu.Items.Add(CreateViewOptionsMenu());
            contextMenu.Items.Add(CreateProfileMenu());
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
            
            for (int i = 0; i < ApplicationConstants.ScaleOptions.Values.Length; i++)
            {
                var scale = ApplicationConstants.ScaleOptions.Values[i];
                var label = ApplicationConstants.ScaleOptions.Labels[i];
                
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
                Background = BrushFactory.CreateTransparentBackground();
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
                    baseWidth = _isMouseVisible ? ApplicationConstants.WindowSizes.FpsKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FpsKeyboardWidth;
                    baseHeight = ApplicationConstants.WindowSizes.FpsKeyboardHeight;
                }
                else
                {
                    baseWidth = ApplicationConstants.WindowSizes.FullKeyboardWidth;
                    baseHeight = ApplicationConstants.WindowSizes.FullKeyboardHeight;
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
                    Width = ApplicationConstants.WindowSizes.FullKeyboardWidth * _displayScale;
                    Height = ApplicationConstants.WindowSizes.FullKeyboardHeight * _displayScale;
                    break;
                    
                case KeyboardProfile.FPSKeyboard:
                    ShowFPSKeyboardLayout();
                    // ウィンドウサイズ調整（FPS用サイズ、マウス表示考慮）
                    Width = (_isMouseVisible ? ApplicationConstants.WindowSizes.FpsKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FpsKeyboardWidth) * _displayScale;
                    Height = ApplicationConstants.WindowSizes.FpsKeyboardHeight * _displayScale;
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
                    _scrollUpTimer = ApplicationConstants.Timing.ScrollDisplayFrames;
                }
                else if (e.Delta < 0)
                {
                    // 下スクロール
                    _scrollDownTimer = ApplicationConstants.Timing.ScrollDisplayFrames;
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
