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
using KeyOverlayFPS.Initialization;
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
        
        // 設定管理
        public MainWindowSettings Settings { get; set; } = null!;
        
        // メニュー管理
        public MainWindowMenu Menu { get; set; } = null!;
        
        // 入力処理管理
        public MainWindowInput Input { get; set; } = null!;
        
        // ドラッグ操作関連
        private bool _isDragging = false;
        private Point _dragStartPoint;
        
        // 動的レイアウトシステム
        public LayoutManager LayoutManager { get; }
        public KeyEventBinder? EventBinder { get; set; }
        public Canvas? DynamicCanvas { get; set; }
        
        // マウス方向可視化設定はApplicationConstants.MouseVisualizationに移動済み

        public MainWindow()
        {
            LayoutManager = new LayoutManager();
            try
            {
                var initializer = new WindowInitializer();
                initializer.Initialize(this);
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow 初期化でエラーが発生", ex);
                throw;
            }
        }
        
        
                
        private void SetBackgroundColor(Color color, bool transparent)
        {
            Settings.SetBackgroundColor(color, transparent);
        }
        
        private void SetForegroundColor(Color color)
        {
            Settings.SetForegroundColor(color);
            UpdateAllTextForeground();
        }
        
        private void SetHighlightColor(Color color)
        {
            Settings.SetHighlightColor(color);
        }
        
        private void ToggleTopmost()
        {
            Settings.ToggleTopmost();
        }
        
        private void ToggleMouseVisibility()
        {
            Settings.ToggleMouseVisibility();
            UpdateMouseVisibility();
        }
        
        private void UpdateMouseVisibility()
        {
            // プロファイルレイアウトを再適用してマウス表示状態を反映
            ApplyProfileLayout();
        }
        
        private void SetDisplayScale(double scale)
        {
            Settings.SetDisplayScale(scale);
            ApplyDisplayScale();
        }
        
        internal void ApplyDisplayScale()
        {
            var canvas = Content as Canvas;
            if (canvas != null)
            {
                // Canvas全体にスケール変換を適用
                var transform = new ScaleTransform(Settings.DisplayScale, Settings.DisplayScale);
                canvas.RenderTransform = transform;
                
                // プロファイルに応じたウィンドウサイズ調整
                double baseWidth, baseHeight;
                if (Input.KeyboardHandler.CurrentProfile == KeyboardProfile.FPSKeyboard)
                {
                    baseWidth = Settings.IsMouseVisible ? ApplicationConstants.WindowSizes.FpsKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FpsKeyboardWidth;
                    baseHeight = ApplicationConstants.WindowSizes.FpsKeyboardHeight;
                }
                else
                {
                    baseWidth = Settings.IsMouseVisible ? ApplicationConstants.WindowSizes.FullKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FullKeyboardWidth;
                    baseHeight = ApplicationConstants.WindowSizes.FullKeyboardHeight;
                }
                
                Width = baseWidth * Settings.DisplayScale;
                Height = baseHeight * Settings.DisplayScale;
            }
        }
        
        
        private void SwitchProfile(KeyboardProfile profile)
        {
            Input.KeyboardHandler.CurrentProfile = profile;
            
            // プロファイルに対応する新しいYAMLファイルを読み込み
            try
            {
                LayoutManager.LoadLayout(profile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"レイアウトファイル読み込みエラー: {ex.Message}");
            }
            
            ApplyProfileLayout();
            UpdateMousePositions();
            Settings.SaveSettings();
            Menu.UpdateMenuCheckedState();
        }
        
        internal void ApplyProfileLayout()
        {
            var canvas = Content as Canvas;
            if (canvas == null) return;
            
            switch (Input.KeyboardHandler.CurrentProfile)
            {
                case KeyboardProfile.FullKeyboard65:
                    ShowFullKeyboardLayout();
                    // ウィンドウサイズ調整
                    Width = (Settings.IsMouseVisible ? ApplicationConstants.WindowSizes.FullKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FullKeyboardWidth) * Settings.DisplayScale;
                    Height = ApplicationConstants.WindowSizes.FullKeyboardHeight * Settings.DisplayScale;
                    break;
                    
                case KeyboardProfile.FPSKeyboard:
                    ShowFPSKeyboardLayout();
                    // ウィンドウサイズ調整（FPS用サイズ、マウス表示考慮）
                    Width = (Settings.IsMouseVisible ? ApplicationConstants.WindowSizes.FpsKeyboardWidthWithMouse : ApplicationConstants.WindowSizes.FpsKeyboardWidth) * Settings.DisplayScale;
                    Height = ApplicationConstants.WindowSizes.FpsKeyboardHeight * Settings.DisplayScale;
                    break;
            }
        }
        
        private void ShowFullKeyboardLayout()
        {
            // YAMLファイルのisVisible設定に基づいて表示するキーを取得
            var visibleKeys = LayoutManager.GetVisibleKeys();
            
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
                    else if (visibleKeys.Contains(border.Name))
                    {
                        border.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        border.Visibility = Visibility.Collapsed;
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
            // YAMLファイルのisVisible設定に基づいて表示するキーを取得
            var visibleKeys = LayoutManager.GetVisibleKeys();
            
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
                    else if (visibleKeys.Contains(border.Name))
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
                element.Visibility = Settings.IsMouseVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // マウス本体や他のマウス要素
                element.Visibility = Settings.IsMouseVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        /// <summary>
        /// UIエレメントを取得（KeyEventBinderのキャッシュを使用）
        /// </summary>
        private T? GetCachedElement<T>(string name) where T : FrameworkElement
        {
            return EventBinder?.FindElement<T>(name);
        }
        
        internal void UpdateMousePositions()
        {
            var position = LayoutManager.GetMousePosition();
            
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

        internal void UpdateAllTextForeground()
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
                textBlock.Foreground = Settings.ForegroundBrush;
            }
            else if (border.Child is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = Settings.ForegroundBrush;
                    }
                }
            }
        }

        /// <summary>
        /// ドラッグ操作を開始
        /// </summary>
        private void StartDrag(MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            CaptureMouse();
        }
        
        internal void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDrag(e);
        }

        internal void MainWindow_MouseMove(object sender, MouseEventArgs e)
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

        internal void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            StartDrag(e);
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
        
        internal void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Input.HandleMouseWheel(e.Delta);
        }
        
        /// <summary>
        /// メニューアクションを初期化
        /// </summary>
        internal void InitializeMenuActions(MainWindowMenu menu)
        {
            menu.SetBackgroundColorAction = SetBackgroundColor;
            menu.SetForegroundColorAction = SetForegroundColor;
            menu.SetHighlightColorAction = SetHighlightColor;
            menu.ToggleTopmostAction = ToggleTopmost;
            menu.ToggleMouseVisibilityAction = ToggleMouseVisibility;
            menu.SetDisplayScaleAction = SetDisplayScale;
            menu.SwitchProfileAction = SwitchProfile;
        }
        
        
        /// <summary>
        /// イベント委譲アクションを初期化
        /// </summary>
        internal void InitializeEventActions()
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
        internal void OnSettingsChanged(object? sender, EventArgs e)
        {
            // メニューのチェック状態を更新
            Menu.UpdateMenuCheckedState();
        }
    }
}
