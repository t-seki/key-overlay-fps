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
        
        // 入力処理管理
        private MainWindowInput _input;
        
        // ドラッグ操作関連
        private bool _isDragging = false;
        private Point _dragStartPoint;
        
        // 設定管理システム
        private readonly SettingsManager _settingsManager = SettingsManager.Instance;
        
        // マウス移動可視化
        private readonly MouseTracker _mouseTracker = new();
        
        // 動的レイアウトシステム
        private LayoutConfig? _currentLayout;
        private KeyEventBinder? _eventBinder;
        private Canvas? _dynamicCanvas;
        
        // マウス方向可視化設定はApplicationConstants.MouseVisualizationに移動済み

        public MainWindow()
        {
            try
            {
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
                
                // 入力処理管理システムを初期化
                Logger.Info("入力処理システム初期化開始");
                // ブラシを統一ファクトリーから初期化
                var keyboardKeyBackgroundBrush = BrushFactory.CreateKeyboardKeyBackground();
                _input = new MainWindowInput(this, _settings, _keyboardHandler, _mouseTracker, _eventBinder, keyboardKeyBackgroundBrush);
                InitializeInputActions();
                _input.Start();
                Logger.Info("入力処理システム初期化完了");

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
                _dynamicCanvas = UIGenerator.GenerateCanvas(_currentLayout, this);

                // 既存のCanvasと置き換え
                Content = _dynamicCanvas;

                // ウィンドウ背景を設定
                Background = BrushFactory.CreateTransparentBackground();

                // 要素名を登録
                UIGenerator.RegisterElementNames(_dynamicCanvas, this);

                // KeyboardInputHandlerにLayoutConfigを設定
                _keyboardHandler.SetLayoutConfig(_currentLayout);

                // イベントバインディング
                _eventBinder = new KeyEventBinder(_dynamicCanvas, _currentLayout, _keyboardHandler, _mouseTracker);
                _eventBinder.BindAllEvents();

            }
            catch (Exception ex)
            {
                Logger.Error("動的レイアウトシステム初期化でエラーが発生", ex);
                throw;
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
                    baseWidth = _settings.IsMouseVisible ? ApplicationConstants.WindowSizes.FullKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FullKeyboardWidth;
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
                { "MouseButton4", (0, 64) },
                { "MouseButton5", (0, 42) },
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
            _input.HandleMouseWheel(e.Delta);
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
        /// 入力処理アクションを初期化
        /// </summary>
        private void InitializeInputActions()
        {
            _input.UpdateAllTextForegroundAction = UpdateAllTextForeground;
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
