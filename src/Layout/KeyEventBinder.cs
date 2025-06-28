using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.MouseVisualization;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// 動的生成されたUI要素にイベントハンドラーを登録するクラス
    /// </summary>
    public class KeyEventBinder
    {
        private readonly KeyboardInputHandler _keyboardHandler;
        private readonly MouseTracker _mouseTracker;
        private readonly Canvas _canvas;
        private readonly LayoutConfig _layout;
        private readonly Dictionary<string, FrameworkElement> _elementCache = new();

        public KeyEventBinder(Canvas canvas, LayoutConfig layout, KeyboardInputHandler keyboardHandler, MouseTracker mouseTracker)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
            _mouseTracker = mouseTracker ?? throw new ArgumentNullException(nameof(mouseTracker));
        }

        /// <summary>
        /// すべてのイベントハンドラーを登録
        /// </summary>
        public void BindAllEvents()
        {
            BindCanvasEvents();
            BindKeyboardEvents();
            BindMouseEvents();
            BuildElementCache();
        }

        /// <summary>
        /// Canvas共通イベントを登録
        /// </summary>
        private void BindCanvasEvents()
        {
            // 右クリックメニュー表示
            _canvas.MouseRightButtonDown += Canvas_MouseRightButtonDown;
            
            // ドラッグ移動用イベント（MainWindowで処理）
            _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            _canvas.MouseWheel += Canvas_MouseWheel;
        }

        /// <summary>
        /// キーボードイベントを登録
        /// </summary>
        private void BindKeyboardEvents()
        {
            if (_layout.Keys == null) return;

            foreach (var (keyName, keyDef) in _layout.Keys)
            {
                if (!keyDef.IsVisible) continue;

                var keyBorder = FindElement<Border>(keyName);
                if (keyBorder != null)
                {
                    keyBorder.MouseLeftButtonDown += KeyBorder_MouseLeftButtonDown;
                    keyBorder.MouseRightButtonDown += KeyBorder_MouseRightButtonDown;
                }
            }
        }

        /// <summary>
        /// マウスイベントを登録
        /// </summary>
        private void BindMouseEvents()
        {
            if (_layout.Mouse?.Buttons == null) return;

            foreach (var (buttonName, buttonConfig) in _layout.Mouse.Buttons)
            {
                if (!buttonConfig.IsVisible) continue;

                var buttonBorder = FindElement<Border>(buttonName);
                if (buttonBorder != null)
                {
                    buttonBorder.MouseLeftButtonDown += MouseButton_MouseLeftButtonDown;
                    buttonBorder.MouseRightButtonDown += MouseButton_MouseRightButtonDown;
                }
            }

            // マウス移動可視化のイベント登録
            if (_mouseTracker != null)
            {
                _mouseTracker.MouseMoved += OnMouseMoved;
            }
        }

        /// <summary>
        /// 要素キャッシュを構築
        /// </summary>
        private void BuildElementCache()
        {
            _elementCache.Clear();
            BuildElementCacheRecursive(_canvas);
        }

        /// <summary>
        /// 要素キャッシュを再帰的に構築
        /// </summary>
        private void BuildElementCacheRecursive(DependencyObject parent)
        {
            if (parent is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
            {
                _elementCache[element.Name] = element;
            }

            // 子要素を再帰的に処理
            var childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                BuildElementCacheRecursive(child);
            }

            // Canvas子要素の特別処理
            if (parent is Canvas canvas)
            {
                foreach (UIElement child in canvas.Children)
                {
                    BuildElementCacheRecursive(child);
                }
            }

            // Border子要素の特別処理
            if (parent is Border border && border.Child != null)
            {
                BuildElementCacheRecursive(border.Child);
            }

            // StackPanel子要素の特別処理
            if (parent is StackPanel stackPanel)
            {
                foreach (UIElement child in stackPanel.Children)
                {
                    BuildElementCacheRecursive(child);
                }
            }
        }

        /// <summary>
        /// 要素を名前で検索（キャッシュ付き）
        /// </summary>
        public T? FindElement<T>(string name) where T : FrameworkElement
        {
            return _elementCache.TryGetValue(name, out var element) ? element as T : null;
        }

        #region Canvas Events

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 親ウィンドウのコンテキストメニューを表示
            if (_canvas.Parent is Window window && window.ContextMenu != null)
            {
                window.ContextMenu.IsOpen = true;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 親ウィンドウのドラッグ開始処理を呼び出し
            if (_canvas.Parent is MainWindow mainWindow)
            {
                mainWindow.CanvasLeftButtonDownAction?.Invoke(sender, e);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // 親ウィンドウのドラッグ処理を呼び出し
            if (_canvas.Parent is MainWindow mainWindow)
            {
                mainWindow.CanvasMoveAction?.Invoke(sender, e);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 親ウィンドウのドラッグ終了処理を呼び出し
            if (_canvas.Parent is MainWindow mainWindow)
            {
                mainWindow.CanvasLeftButtonUpAction?.Invoke(sender, e);
            }
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 親ウィンドウのマウスホイール処理を呼び出し
            if (_canvas.Parent is MainWindow mainWindow)
            {
                mainWindow.CanvasWheelAction?.Invoke(sender, e);
            }
        }

        #endregion

        #region Keyboard Events

        private void KeyBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 親ウィンドウのキーボーダーマウス処理を呼び出し
            if (_canvas.Parent is MainWindow mainWindow)
            {
                mainWindow.KeyBorderLeftButtonDownAction?.Invoke(sender, e);
            }
        }

        private void KeyBorder_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 親ウィンドウのキーボーダー右クリック処理を呼び出し
            if (_canvas.Parent is MainWindow mainWindow)
            {
                mainWindow.KeyBorderRightButtonDownAction?.Invoke(sender, e);
            }
        }

        #endregion

        #region Mouse Events

        private void MouseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // マウスボタンのクリック処理（キーボードと同じ処理）
            KeyBorder_MouseLeftButtonDown(sender, e);
        }

        private void MouseButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // マウスボタンの右クリック処理（キーボードと同じ処理）
            KeyBorder_MouseRightButtonDown(sender, e);
        }

        private void OnMouseMoved(object? sender, MouseMoveEventArgs e)
        {
            // マウス移動方向インジケーターの表示処理
            var directionCanvas = FindElement<Canvas>("MouseDirectionCanvas");
            if (directionCanvas == null) return;

            // 方向インジケーターを探して更新
            var directionName = $"Direction{e.Direction}";
            var indicator = FindElement<System.Windows.Shapes.Path>(directionName);
            
            if (indicator != null)
            {
                // 他のインジケーターをリセット
                ResetDirectionIndicators(directionCanvas);
                
                // 該当方向をハイライト
                indicator.Opacity = 0.9;
            }
        }

        /// <summary>
        /// 方向インジケーターをリセット
        /// </summary>
        private void ResetDirectionIndicators(Canvas directionCanvas)
        {
            foreach (UIElement child in directionCanvas.Children)
            {
                if (child is System.Windows.Shapes.Path path && path.Name?.StartsWith("Direction") == true)
                {
                    path.Opacity = 0.0;
                }
            }
        }

        #endregion

    }
}