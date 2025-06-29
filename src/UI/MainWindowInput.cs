using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// MainWindowの入力処理責務を担当するクラス
    /// </summary>
    public class MainWindowInput : IDisposable
    {
        private readonly Window _window;
        private readonly MainWindowSettings _settings;
        private readonly KeyboardInputHandler _keyboardHandler;
        private readonly MouseTracker _mouseTracker;
        private readonly KeyEventBinder? _eventBinder;
        private readonly LayoutManager _layoutManager;
        private readonly Brush _inactiveBrush;

        // タイマー管理
        private readonly DispatcherTimer _timer;
        
        // マウスホイールフック
        private readonly MouseWheelHook _mouseWheelHook;
        
        // スクロール表示タイマー
        private int _scrollUpTimer = 0;
        private int _scrollDownTimer = 0;
        
        // マウスホイールフラグ（フック検知用）
        private volatile bool _wheelUpDetected = false;
        private volatile bool _wheelDownDetected = false;
        
        // リソース管理
        private bool _disposed = false;
        
        // マウス方向インジケーター管理
        private readonly Dictionary<MouseDirection, System.Windows.Shapes.Path> _directionIndicators = new();
        private DispatcherTimer? _directionHideTimer;

        /// <summary>
        /// UI更新アクション
        /// </summary>
        public Action? UpdateAllTextForegroundAction { get; set; }
        
        /// <summary>
        /// キーボード入力ハンドラーへのアクセス
        /// </summary>
        public KeyboardInputHandler KeyboardHandler => _keyboardHandler;

        public MainWindowInput(
            Window window,
            MainWindowSettings settings,
            KeyboardInputHandler keyboardHandler,
            MouseTracker mouseTracker,
            KeyEventBinder? eventBinder,
            LayoutManager layoutManager,
            Brush inactiveBrush)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
            _mouseTracker = mouseTracker ?? throw new ArgumentNullException(nameof(mouseTracker));
            _eventBinder = eventBinder;
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _inactiveBrush = inactiveBrush ?? throw new ArgumentNullException(nameof(inactiveBrush));

            // タイマー初期化
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.MainTimerInterval)
            };
            _timer.Tick += Timer_Tick;
            
            // マウスホイールフック初期化
            _mouseWheelHook = new MouseWheelHook();
            _mouseWheelHook.MouseWheelDetected += OnMouseWheelDetected;
            
            // マウス移動イベント設定
            _mouseTracker.MouseMoved += OnMouseMoved;
        }

        /// <summary>
        /// 入力処理を開始
        /// </summary>
        public void Start()
        {
            _timer.Start();
            _mouseWheelHook.StartHook();
            BuildDirectionIndicatorCache();
        }

        /// <summary>
        /// 入力処理を停止
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            _mouseWheelHook.StopHook();
            _directionHideTimer?.Stop();
        }

        /// <summary>
        /// タイマーティック処理
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // マウス位置更新
            _mouseTracker.Update();

            // キーボード入力更新
            bool isShiftPressed = KeyboardInputHandler.IsKeyPressed(VirtualKeyCodes.VK_SHIFT);
            UpdateKeys(isShiftPressed);
            
            // マウス入力（表示時のみ更新）
            if (_settings.IsMouseVisible)
            {
                UpdateMouseKeys();
                UpdateScrollIndicators();
                ProcessWheelFlags();
            }
        }

        /// <summary>
        /// キーボード入力状態を更新
        /// </summary>
        private void UpdateKeys(bool isShiftPressed)
        {
            var canvas = _window.Content as Canvas;
            if (canvas == null) return;

            foreach (UIElement child in canvas.Children)
            {
                if (child is Border keyBorder && !string.IsNullOrEmpty(keyBorder.Name))
                {
                    UpdateKeyStateByName(keyBorder.Name, isShiftPressed);
                }
            }
        }

        /// <summary>
        /// 名前でキー状態を更新
        /// </summary>
        private void UpdateKeyStateByName(string keyName, bool isShiftPressed)
        {
            var keyBorder = GetCachedElement<Border>(keyName);
            if (keyBorder == null) return;

            var keyConfig = _keyboardHandler.GetKeyConfig(keyName);
            if (keyConfig == null) return;

            // ハイライトの表示
            bool isPressed = KeyboardInputHandler.IsKeyPressed(keyConfig.VirtualKey);
            keyBorder.Background = isPressed ? _settings.ActiveBrush : _inactiveBrush;

            // テキストの更新
            var textBlock = keyBorder.Child as TextBlock;
            if (textBlock != null)
            {
                bool shouldShowShiftText = isShiftPressed && _layoutManager.IsShiftDisplayEnabled();

                if (shouldShowShiftText && keyConfig.HasShiftVariant)
                {
                    textBlock.Text = keyConfig.ShiftText;
                }
                else
                {
                    textBlock.Text = keyConfig.NormalText;
                }
            }
        }

        /// <summary>
        /// マウスキー状態を更新
        /// </summary>
        private void UpdateMouseKeys()
        {
            UpdateMouseKey("MouseLeft", VirtualKeyCodes.VK_LBUTTON);
            UpdateMouseKey("MouseRight", VirtualKeyCodes.VK_RBUTTON);
            UpdateMouseKey("MouseWheelButton", VirtualKeyCodes.VK_MBUTTON);
            UpdateMouseKey("MouseButton4", VirtualKeyCodes.VK_XBUTTON1);
            UpdateMouseKey("MouseButton5", VirtualKeyCodes.VK_XBUTTON2);
        }

        /// <summary>
        /// 指定マウスキーの状態を更新
        /// </summary>
        private void UpdateMouseKey(string keyName, int virtualKeyCode)
        {
            var keyBorder = GetCachedElement<Border>(keyName);
            if (keyBorder != null)
            {
                bool isPressed = KeyboardInputHandler.IsKeyPressed(virtualKeyCode);
                keyBorder.Background = isPressed ? _settings.ActiveBrush : _inactiveBrush;
            }
        }

        /// <summary>
        /// マウスホイールスクロール表示を更新
        /// </summary>
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
        /// マウスホイール処理
        /// </summary>
        public void HandleMouseWheel(int delta)
        {
            if (_settings.IsMouseVisible)
            {
                // スクロール表示（マウス表示時のみ）
                if (delta > 0)
                {
                    _scrollUpTimer = ApplicationConstants.Timing.ScrollDisplayFrames;
                }
                else if (delta < 0)
                {
                    _scrollDownTimer = ApplicationConstants.Timing.ScrollDisplayFrames;
                }
            }
        }

        /// <summary>
        /// マウスホイールフック検知イベントハンドラー
        /// </summary>
        private void OnMouseWheelDetected(object? sender, MouseWheelEventArgs e)
        {
            // フラグを設定（タイマーティックで処理される）
            if (e.Delta > 0)
            {
                _wheelUpDetected = true;
            }
            else if (e.Delta < 0)
            {
                _wheelDownDetected = true;
            }
        }

        /// <summary>
        /// ホイールフラグを処理してタイマーを設定
        /// </summary>
        private void ProcessWheelFlags()
        {
            if (_wheelUpDetected)
            {
                _scrollUpTimer = ApplicationConstants.Timing.ScrollDisplayFrames;
                _wheelUpDetected = false;
            }

            if (_wheelDownDetected)
            {
                _scrollDownTimer = ApplicationConstants.Timing.ScrollDisplayFrames;
                _wheelDownDetected = false;
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        private void OnMouseMoved(object? sender, MouseMoveEventArgs e)
        {
            // 方向インジケーターの表示
            var directionName = $"Direction{e.Direction}";
            if (_directionIndicators.TryGetValue(e.Direction, out var indicator))
            {
                // 他のインジケーターをリセット
                ResetDirectionIndicators();
                
                // 該当方向をハイライト
                indicator.Opacity = 0.9;
                
                // 自動非表示タイマーを開始
                StartDirectionHideTimer();
            }
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

        /// <summary>
        /// 方向非表示タイマーを開始
        /// </summary>
        private void StartDirectionHideTimer()
        {
            _directionHideTimer?.Stop();
            _directionHideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.DirectionHideDelay)
            };
            _directionHideTimer.Tick += OnDirectionHideTimer_Tick;
            _directionHideTimer.Start();
        }

        /// <summary>
        /// 方向非表示タイマーティック
        /// </summary>
        private void OnDirectionHideTimer_Tick(object? sender, EventArgs e)
        {
            ResetDirectionIndicators();
            _directionHideTimer?.Stop();
        }

        /// <summary>
        /// 方向インジケーターキャッシュを構築
        /// </summary>
        private void BuildDirectionIndicatorCache()
        {
            _directionIndicators.Clear();
            
            for (int i = 0; i < ApplicationConstants.MouseVisualization.DirectionSegments; i++)
            {
                var direction = (MouseDirection)i;
                var directionName = $"Direction{direction}";
                var indicator = GetCachedElement<System.Windows.Shapes.Path>(directionName);
                
                if (indicator != null)
                {
                    _directionIndicators[direction] = indicator;
                }
            }
        }

        /// <summary>
        /// キャッシュされた要素を取得
        /// </summary>
        private T? GetCachedElement<T>(string name) where T : FrameworkElement
        {
            return _eventBinder?.FindElement<T>(name);
        }

        #region IDisposable実装

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // マネージリソースの解放
                    Stop(); // タイマーとフックを停止
                    _mouseWheelHook?.Dispose();
                    _directionHideTimer?.Stop();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}