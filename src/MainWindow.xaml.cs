using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Colors;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Utils;
using System.Windows.Shapes;

namespace KeyOverlayFPS
{


    public partial class MainWindow : Window
    {
        // UI要素イベント委譲用アクション
        public Action<object, MouseButtonEventArgs>? CanvasLeftButtonDownAction { get; private set; }
        public Action<object, MouseEventArgs>? CanvasMoveAction { get; private set; }
        public Action<object, MouseButtonEventArgs>? CanvasLeftButtonUpAction { get; private set; }
        public Action<object, MouseWheelEventArgs>? CanvasWheelAction { get; private set; }
        public Action<object, MouseButtonEventArgs>? KeyBorderLeftButtonDownAction { get; private set; }
        public Action<object, MouseButtonEventArgs>? KeyBorderRightButtonDownAction { get; private set; }
        
        // キーボード入力ハンドラー
        private readonly KeyboardInputHandler _keyboardHandler = new KeyboardInputHandler();

        // 設定管理
        private readonly MainWindowSettings _settings;
        
        // メニュー管理
        private readonly MainWindowMenu _menu;
        
        private readonly DispatcherTimer _timer;
        private readonly Brush _inactiveBrush;
        private readonly Brush _keyboardKeyDefaultBrush;
        
        // ドラッグ操作関連
        private bool _isDragging = false;
        private Point _dragStartPoint;
        
        // スクロール表示タイマー
        private int _scrollUpTimer = 0;
        private int _scrollDownTimer = 0;
        
        // 設定管理システム
        private readonly SettingsManager _settingsManager = SettingsManager.Instance;
        
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
            try
            {
                // ブラシを統一ファクトリーから初期化
                _keyboardKeyDefaultBrush = BrushFactory.CreateKeyboardKeyBackground();
                _inactiveBrush = _keyboardKeyDefaultBrush;
                
                InitializeComponent();
                
                // 設定管理システムを初期化
                _settings = new MainWindowSettings(this, _settingsManager);
                _settings.SettingsChanged += OnSettingsChanged;
                
                // メニュー管理システムを初期化
                _menu = new MainWindowMenu(this, _settings, _keyboardHandler);
                InitializeMenuActions();
                
                // イベント委譲アクションを初期化
                InitializeEventActions();
                
                // 設定システム初期化
                Logger.Info("設定システム初期化開始");
                _settings.Initialize();
                Logger.Info("設定システム初期化完了");
                
                // 動的レイアウトシステムを初期化
                Logger.Info("動的レイアウトシステム初期化開始");
                InitializeDynamicLayoutSystem();
                Logger.Info("動的レイアウトシステム初期化完了");
                
                // タイマー初期化
                Logger.Info("タイマー初期化開始");
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.MainTimerInterval)
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();
                Logger.Info("タイマー初期化完了");

                // イベントハンドラー設定
                Logger.Info("イベントハンドラー設定開始");
                MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
                MouseMove += MainWindow_MouseMove;
                MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
                this.MouseWheel += MainWindow_MouseWheel;
                Logger.Info("イベントハンドラー設定完了");
                
                // コンテキストメニュー設定
                Logger.Info("コンテキストメニュー設定開始");
                _menu.SetupContextMenu();
                Logger.Info("コンテキストメニュー設定完了");
                
                // アプリケーション終了時に設定を保存
                Logger.Info("終了時処理設定");
                Application.Current.Exit += (s, e) => _settingsManager.Save();
                
                // 設定適用
                Logger.Info("設定適用開始");
                ApplyProfileLayout();
                ApplyDisplayScale();
                UpdateMousePositions();
                UpdateAllTextForeground();
                Logger.Info("設定適用完了");
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow コンストラクタでエラーが発生", ex);
                throw;
            }
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
                    Logger.Info($"レイアウトファイルが存在、読み込み中: {layoutPath}");
                    _currentLayout = LayoutManager.ImportLayout(layoutPath);
                }
                else
                {
                    Logger.Info($"レイアウトファイルが存在しない、デフォルトレイアウトを使用: {layoutPath}");
                    // デフォルトレイアウトを使用
                    _currentLayout = _keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard 
                        ? LayoutManager.CreateDefaultFPSLayout()
                        : LayoutManager.CreateDefault65KeyboardLayout();
                }
                
                
                // UIを動的生成
                if (_currentLayout != null)
                {
                    _dynamicCanvas = UIGenerator.GenerateCanvas(_currentLayout, this);
                }
                else
                {
                    Logger.Error("レイアウトがnullのため、UI生成をスキップ");
                    throw new InvalidOperationException("レイアウトの初期化に失敗しました");
                }
                
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
                Logger.Error("動的レイアウトシステム初期化でエラーが発生", ex);
                try
                {
                    // エラー時はデフォルトレイアウトにフォールバック
                    Logger.Info("フォールバック: デフォルトレイアウトを使用");
                    _currentLayout = LayoutManager.CreateDefault65KeyboardLayout();
                    _dynamicCanvas = UIGenerator.GenerateCanvas(_currentLayout, this);
                    Content = _dynamicCanvas;
                    Logger.Info("フォールバック完了");
                }
                catch (Exception fallbackEx)
                {
                    Logger.Error("フォールバック処理でもエラーが発生", fallbackEx);
                    throw;
                }
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

        
        private void SetBackgroundColor(Color color, bool transparent)
        {
            _settings.SetBackgroundColor(color, transparent);
        }
        
        private void SetForegroundColor(Color color)
        {
            _settings.SetForegroundColor(color);
            UpdateAllTextForeground();
        }
        
        private void SetHighlightColor(Color color)
        {
            _settings.SetHighlightColor(color);
        }
        
        private void ToggleTopmost()
        {
            _settings.ToggleTopmost();
        }
        
        private void ToggleMouseVisibility()
        {
            _settings.ToggleMouseVisibility();
            UpdateMouseVisibility();
        }
        
        private void UpdateMouseVisibility()
        {
            // プロファイルレイアウトを再適用してマウス表示状態を反映
            ApplyProfileLayout();
        }
        
        private void SetDisplayScale(double scale)
        {
            _settings.SetDisplayScale(scale);
            ApplyDisplayScale();
        }
        
        private void ApplyDisplayScale()
        {
            var canvas = Content as Canvas;
            if (canvas != null)
            {
                // Canvas全体にスケール変換を適用
                var transform = new ScaleTransform(_settings.DisplayScale, _settings.DisplayScale);
                canvas.RenderTransform = transform;
                
                // プロファイルに応じたウィンドウサイズ調整
                double baseWidth, baseHeight;
                if (_keyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard)
                {
                    baseWidth = _settings.IsMouseVisible ? ApplicationConstants.WindowSizes.FpsKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FpsKeyboardWidth;
                    baseHeight = ApplicationConstants.WindowSizes.FpsKeyboardHeight;
                }
                else
                {
                    baseWidth = ApplicationConstants.WindowSizes.FullKeyboardWidth;
                    baseHeight = ApplicationConstants.WindowSizes.FullKeyboardHeight;
                }
                
                Width = baseWidth * _settings.DisplayScale;
                Height = baseHeight * _settings.DisplayScale;
            }
        }
        
        
        private void SwitchProfile(KeyboardProfile profile)
        {
            _keyboardHandler.CurrentProfile = profile;
            ApplyProfileLayout();
            UpdateMousePositions();
            _settings.SaveSettings();
            _menu.UpdateMenuCheckedState();
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
                    Width = ApplicationConstants.WindowSizes.FullKeyboardWidth * _settings.DisplayScale;
                    Height = ApplicationConstants.WindowSizes.FullKeyboardHeight * _settings.DisplayScale;
                    break;
                    
                case KeyboardProfile.FPSKeyboard:
                    ShowFPSKeyboardLayout();
                    // ウィンドウサイズ調整（FPS用サイズ、マウス表示考慮）
                    Width = (_settings.IsMouseVisible ? ApplicationConstants.WindowSizes.FpsKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FpsKeyboardWidth) * _settings.DisplayScale;
                    Height = ApplicationConstants.WindowSizes.FpsKeyboardHeight * _settings.DisplayScale;
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
                    if (IsMouseElement(border.Name))
                    {
                        SetMouseElementVisibility(child);
                    }
                    else
                    {
                        border.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    SetMouseElementVisibility(child);
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
                        SetMouseElementVisibility(child);
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
                else
                {
                    SetMouseElementVisibility(child);
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
        /// マウス要素の可視性を設定
        /// </summary>
        private void SetMouseElementVisibility(UIElement element)
        {
            if (element is Canvas childCanvas && childCanvas.Name == "MouseDirectionCanvas")
            {
                // マウス移動可視化キャンバス
                element.Visibility = _settings.IsMouseVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // マウス本体や他のマウス要素
                element.Visibility = _settings.IsMouseVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        /// <summary>
        /// UIエレメントを取得（KeyEventBinderのキャッシュを使用）
        /// </summary>
        private T? GetCachedElement<T>(string name) where T : FrameworkElement
        {
            return _eventBinder?.FindElement<T>(name);
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
        
        /// <summary>
        /// 設定システムの初期化
        /// </summary>
        private void InitializeSettings()
        {
            try
            {
                _settingsManager.Load();
                Logger.Info($"設定読み込み完了 - Profile: {_settingsManager.Current.CurrentProfile}, Scale: {_settingsManager.Current.DisplayScale}");
            }
            catch (Exception ex)
            {
                Logger.Error("設定システム初期化でエラーが発生", ex);
                throw;
            }
        }



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
            if (_settings.IsMouseVisible)
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
                keyBorder.Background = isPressed ? _settings.ActiveBrush : _inactiveBrush;
            }
        }

        private void UpdateKeyStateWithShift(string keyName, int virtualKeyCode, string normalText, string shiftText, bool isShiftPressed)
        {
            var keyBorder = GetCachedElement<Border>(keyName);
            var textBlock = GetCachedElement<TextBlock>(keyName + "Text");
            
            if (keyBorder != null && textBlock != null)
            {
                bool isPressed = KeyboardInputHandler.IsKeyPressed(virtualKeyCode);
                keyBorder.Background = isPressed ? _settings.ActiveBrush : _inactiveBrush;
                
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
            if (_settings.IsMouseVisible)
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
                    scrollUpIndicator.Foreground = _settings.ActiveBrush; // ハイライト色を使用
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
                    scrollDownIndicator.Foreground = _settings.ActiveBrush; // ハイライト色を使用
                    _scrollDownTimer--;
                }
                else
                {
                    scrollDownIndicator.Foreground = _inactiveBrush;
                }
            }
        }
        
        /// <summary>
        /// メニューアクションを初期化
        /// </summary>
        private void InitializeMenuActions()
        {
            _menu.SetBackgroundColorAction = SetBackgroundColor;
            _menu.SetForegroundColorAction = SetForegroundColor;
            _menu.SetHighlightColorAction = SetHighlightColor;
            _menu.ToggleTopmostAction = ToggleTopmost;
            _menu.ToggleMouseVisibilityAction = ToggleMouseVisibility;
            _menu.SetDisplayScaleAction = SetDisplayScale;
            _menu.SwitchProfileAction = SwitchProfile;
        }
        
        /// <summary>
        /// イベント委譲アクションを初期化
        /// </summary>
        private void InitializeEventActions()
        {
            CanvasLeftButtonDownAction = MainWindow_MouseLeftButtonDown;
            CanvasMoveAction = MainWindow_MouseMove;
            CanvasLeftButtonUpAction = MainWindow_MouseLeftButtonUp;
            CanvasWheelAction = MainWindow_MouseWheel;
            KeyBorderLeftButtonDownAction = KeyBorder_MouseLeftButtonDown;
            KeyBorderRightButtonDownAction = KeyBorder_MouseRightButtonDown;
        }
        
        /// <summary>
        /// 設定変更時のイベントハンドラー
        /// </summary>
        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            // メニューのチェック状態を更新
            _menu.UpdateMenuCheckedState();
        }
    }
}
