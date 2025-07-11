using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// MainWindowの入力処理責務を担当するクラス
    /// </summary>
    public class MainWindowInput : DisposableBase
    {
        private readonly Window _window;
        private readonly MainWindowSettings _settings;
        private readonly MouseTracker _mouseTracker;
        private readonly UIElementLocator? _elementLocator;
        private readonly LayoutManager _layoutManager;
        private readonly Brush _inactiveBrush;

        // タイマー管理
        private readonly DispatcherTimer _timer;
        
        // 入力状態管理（キーボードとマウスを統合）
        private readonly InputStateManager _inputStateManager;
        
        // マウスホイールフック（ホイールイベントのみ）
        private readonly MouseHook _mouseWheelHook;
        
        // スクロール表示タイマー
        private int _scrollUpTimer = 0;
        private int _scrollDownTimer = 0;
        
        // マウスホイールフラグ（フック検知用）
        private volatile bool _wheelUpDetected = false;
        private volatile bool _wheelDownDetected = false;
        
        // リソース管理
        

        /// <summary>
        /// UI更新アクション
        /// </summary>
        public Action? UpdateAllTextForegroundAction { get; set; }
        

        /// <summary>
        /// 入力状態管理へのアクセス
        /// </summary>
        public InputStateManager InputStateManager => _inputStateManager;

        public MainWindowInput(
            Window window,
            MainWindowSettings settings,
            MouseTracker mouseTracker,
            UIElementLocator? elementLocator,
            LayoutManager layoutManager,
            Brush inactiveBrush)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mouseTracker = mouseTracker ?? throw new ArgumentNullException(nameof(mouseTracker));
            _elementLocator = elementLocator ?? throw new ArgumentNullException(nameof(elementLocator));
            _layoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
            _inactiveBrush = inactiveBrush ?? throw new ArgumentNullException(nameof(inactiveBrush));

            // タイマー初期化
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.MainTimerInterval)
            };
            _timer.Tick += Timer_Tick;
            
            // 入力状態管理初期化（キーボードとマウスを統合）
            _inputStateManager = new InputStateManager();
            
            // マウスホイールフック初期化（ホイールイベントのみ）
            _mouseWheelHook = new MouseHook();
            _mouseWheelHook.MouseWheelDetected += OnMouseWheelDetected;
            
            // マウス移動可視化はMouseDirectionVisualizerで処理
        }

        /// <summary>
        /// 入力処理を開始
        /// </summary>
        public void Start()
        {
            _timer.Start();
            _inputStateManager.Start();
            _mouseWheelHook.StartHook();
        }

        /// <summary>
        /// 入力処理を停止
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            _inputStateManager.Stop();
            _mouseWheelHook.StopHook();
        }

        /// <summary>
        /// タイマーティック処理
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // マウス位置更新
            _mouseTracker.Update();

            // キーボード入力更新
            bool isShiftPressed = _inputStateManager.IsKeyPressed(VirtualKeyCodes.VK_LSHIFT) || 
                                  _inputStateManager.IsKeyPressed(VirtualKeyCodes.VK_RSHIFT);
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

            if (_layoutManager.CurrentLayout?.Keys?.TryGetValue(keyName, out var keyDefinition) != true) return;

            // ハイライトの表示
            bool isPressed = _inputStateManager.IsKeyPressed(keyDefinition!.VirtualKey);
            keyBorder.Background = isPressed ? _settings.ActiveBrush : _inactiveBrush;

            // テキストの更新
            var textBlock = keyBorder.Child as TextBlock;
            if (textBlock != null)
            {
                bool shouldShowShiftText = isShiftPressed && _layoutManager.IsShiftDisplayEnabled();
                bool hasShiftVariant = !string.IsNullOrEmpty(keyDefinition.ShiftText);

                if (shouldShowShiftText && hasShiftVariant)
                {
                    textBlock.Text = keyDefinition.ShiftText;
                }
                else
                {
                    textBlock.Text = keyDefinition.Text ?? "";
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
                bool isPressed = _inputStateManager.IsKeyPressed(virtualKeyCode);
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
                    scrollUpIndicator.Visibility = Visibility.Visible;
                    scrollUpIndicator.Foreground = _settings.ActiveBrush; // ハイライト色を使用
                    _scrollUpTimer--;
                }
                else
                {
                    scrollUpIndicator.Visibility = Visibility.Hidden;
                    scrollUpIndicator.Foreground = _inactiveBrush;
                }
            }
            
            // スクロールダウン表示
            var scrollDownIndicator = GetCachedElement<TextBlock>("ScrollDownIndicator");
            if (scrollDownIndicator != null)
            {
                if (_scrollDownTimer > 0)
                {
                    scrollDownIndicator.Visibility = Visibility.Visible;
                    scrollDownIndicator.Foreground = _settings.ActiveBrush; // ハイライト色を使用
                    _scrollDownTimer--;
                }
                else
                {
                    scrollDownIndicator.Visibility = Visibility.Hidden;
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
        /// キャッシュされた要素を取得
        /// </summary>
        private T? GetCachedElement<T>(string name) where T : FrameworkElement
        {
            return _elementLocator?.FindElement<T>(name);
        }

        #region DisposableBase実装

        /// <summary>
        /// マネージリソースの解放
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Stop(); // タイマーとフックを停止
            _inputStateManager?.Dispose();
            _mouseWheelHook?.Dispose();
        }

        #endregion
    }
}