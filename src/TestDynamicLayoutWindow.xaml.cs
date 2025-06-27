using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.MouseVisualization;

namespace KeyOverlayFPS
{
    /// <summary>
    /// 動的レイアウトシステムのテスト用ウィンドウ
    /// </summary>
    public partial class TestDynamicLayoutWindow : Window
    {
        private LayoutConfig? _currentLayout;
        private KeyEventBinder? _eventBinder;
        private readonly KeyboardInputHandler _keyboardHandler = new();
        private readonly MouseTracker _mouseTracker = new();
        private readonly DispatcherTimer _timer;
        private Canvas? _generatedCanvas;
        private DispatcherTimer? _directionHideTimer;
        private readonly Dictionary<KeyOverlayFPS.MouseVisualization.MouseDirection, System.Windows.Shapes.Path> _directionIndicators = new();
        
        // ブラシキャッシュ（パフォーマンス向上）
        private readonly Brush _activeBrush = new SolidColorBrush(Color.FromArgb(180, 0, 255, 0));
        private readonly Brush _inactiveBrush = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(Color.FromRgb(0x2A, 0x2A, 0x2A), 0),
                new GradientStop(Color.FromRgb(0x1A, 0x1A, 0x1A), 1)
            },
            new Point(0, 0),
            new Point(1, 1)
        );

        public TestDynamicLayoutWindow()
        {
            InitializeComponent();
            
            // タイマー初期化
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16.67) // 約60FPS
            };
            _timer.Tick += Timer_Tick;
            
            LoadAndApplyDynamicLayout();
            
            // 方向表示を非表示にするタイマーを初期化
            _directionHideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 100ms後に非表示
            };
            _directionHideTimer.Tick += OnDirectionHideTimer_Tick;
            
            // タイマー開始
            _timer.Start();
        }

        /// <summary>
        /// 動的レイアウトの読み込みとテスト
        /// </summary>
        private void LoadAndApplyDynamicLayout()
        {
            try
            {
                // 65%キーボードレイアウトを読み込み
                var layoutPath = "/home/tseki/dev/key-overlay-fps/layouts/65_keyboard.yaml";
                
                if (System.IO.File.Exists(layoutPath))
                {
                    // YAMLファイルからレイアウトを読み込み
                    _currentLayout = LayoutManager.ImportLayout(layoutPath);
                    MessageBox.Show($"レイアウト読み込み成功: {_currentLayout.Profile.Name}", "テスト結果", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // デフォルトレイアウトを作成
                    _currentLayout = LayoutManager.CreateDefault65KeyboardLayout();
                    MessageBox.Show("デフォルトレイアウトを作成しました", "テスト結果", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // UIを動的生成
                _generatedCanvas = UIGenerator.GenerateCanvas(_currentLayout, this);
                
                // 既存のCanvasと置き換え
                Content = _generatedCanvas;
                
                // ウィンドウ背景を透明に設定（元のMainWindowと同じ）
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                
                // 要素名を登録
                UIGenerator.RegisterElementNames(_generatedCanvas, this);
                
                // イベントバインディング
                _eventBinder = new KeyEventBinder(_generatedCanvas, _currentLayout, _keyboardHandler, _mouseTracker);
                _eventBinder.BindAllEvents();
                
                // マウス移動イベントの登録
                _mouseTracker.MouseMoved += OnMouseMoved;
                
                // 方向インジケーターのキャッシュを構築
                BuildDirectionIndicatorCache();
                
                MessageBox.Show("動的レイアウトシステムのテストが完了しました！", "テスト成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"動的レイアウトテストでエラーが発生しました: {ex.Message}\n\nスタックトレース:\n{ex.StackTrace}", 
                              "テストエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// タイマーティック処理 - キーボード・マウス状態更新
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_currentLayout?.Keys == null) return;

            try
            {
                // キーボードキーの状態更新
                foreach (var (keyName, keyDef) in _currentLayout.Keys)
                {
                    if (!keyDef.IsVisible || keyDef.VirtualKey == 0) continue;

                    var keyBorder = _eventBinder?.FindElement<Border>(keyName);
                    if (keyBorder != null)
                    {
                        bool isPressed = KeyboardInputHandler.IsKeyPressed(keyDef.VirtualKey);
                        keyBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
                    }
                }

                // マウスボタンの状態更新
                if (_currentLayout.Mouse?.Buttons != null)
                {
                    foreach (var (buttonName, buttonConfig) in _currentLayout.Mouse.Buttons)
                    {
                        if (!buttonConfig.IsVisible || buttonConfig.VirtualKey == 0) continue;

                        var buttonBorder = _eventBinder?.FindElement<Border>(buttonName);
                        if (buttonBorder != null)
                        {
                            bool isPressed = _keyboardHandler.IsMouseButtonPressed(buttonConfig.VirtualKey);
                            buttonBorder.Background = isPressed ? _activeBrush : _inactiveBrush;
                        }
                    }
                }

                // マウス移動追跡
                _mouseTracker?.Update(5.0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Timer_Tick エラー: {ex.Message}");
            }
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
                    if (Enum.TryParse<KeyOverlayFPS.MouseVisualization.MouseDirection>(directionName, out var direction))
                    {
                        _directionIndicators[direction] = path;
                    }
                }
            }
        }

        /// <summary>
        /// マウス移動イベントハンドラー
        /// </summary>
        private void OnMouseMoved(object? sender, KeyOverlayFPS.MouseVisualization.MouseMoveEventArgs e)
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

        protected override void OnClosed(EventArgs e)
        {
            // タイマー停止
            _timer?.Stop();
            _directionHideTimer?.Stop();
            
            // マウスイベント解除
            _mouseTracker.MouseMoved -= OnMouseMoved;
            
            // イベントハンドラーを適切に解除
            _eventBinder?.UnbindAllEvents();
            base.OnClosed(e);
        }
    }
}