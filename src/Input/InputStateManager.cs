using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// 入力状態管理クラス
    /// キーボードとマウスの両方の入力状態を統一的に管理する
    /// </summary>
    public class InputStateManager : DisposableBase
    {
        #region フィールド

        private readonly KeyboardHook _keyboardHook;
        private readonly MouseHook _mouseHook;
        private readonly HookThread _hookThread;
        private readonly ConcurrentDictionary<int, bool> _keyStates;
        private readonly ConcurrentDictionary<int, long> _releasedSince;
        private readonly Func<int, bool> _isKeyDown;
        private readonly Func<long> _getTickMs;
        private bool _isEnabled = false;

        #endregion

        #region 定数

        /// <summary>
        /// 押下中と記録しているキーが「離されている」と返され続けたら、離したことにするまでの時間（ミリ秒）
        /// </summary>
        /// <remarks>
        /// フックのイベント直後は GetAsyncKeyState に反映されていないことがあるため、一度の食い違いでは消さない
        /// </remarks>
        public const int ReconcileReleaseThresholdMs = 100;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        #endregion

        #region イベント

        /// <summary>
        /// キー状態が変化した時に発生するイベント
        /// </summary>
        /// <remarks>
        /// フックのスレッドで発火する。購読側で UI 要素に触るときは Dispatcher 経由にすること
        /// </remarks>
        public event EventHandler<KeyStateChangedEventArgs>? KeyStateChanged;

        /// <summary>
        /// マウスホイールが回転した時に発生するイベント
        /// </summary>
        /// <remarks>
        /// フックのスレッドで発火する。購読側で UI 要素に触るときは Dispatcher 経由にすること
        /// </remarks>
        public event EventHandler<MouseWheelEventArgs>? MouseWheelDetected;

        #endregion

        #region コンストラクタ・デストラクタ

        /// <summary>
        /// InputStateManagerクラスの新しいインスタンスを初期化
        /// </summary>
        public InputStateManager() : this(IsKeyDownByAsyncKeyState, () => Environment.TickCount64)
        {
        }

        /// <summary>
        /// キー状態の問い合わせと時刻を差し替えて、InputStateManagerクラスの新しいインスタンスを初期化
        /// </summary>
        /// <remarks>
        /// コンストラクタではフックを張らない（Start で張る）
        /// </remarks>
        /// <param name="isKeyDown">仮想キーコードを受け取り、そのキーが実際に押されているかを返す</param>
        /// <param name="getTickMs">現在時刻（ミリ秒）を返す</param>
        public InputStateManager(Func<int, bool> isKeyDown, Func<long> getTickMs)
        {
            _isKeyDown = isKeyDown ?? throw new ArgumentNullException(nameof(isKeyDown));
            _getTickMs = getTickMs ?? throw new ArgumentNullException(nameof(getTickMs));
            _releasedSince = new ConcurrentDictionary<int, long>();
            _keyStates = new ConcurrentDictionary<int, bool>();
            _keyboardHook = new KeyboardHook();
            _mouseHook = new MouseHook();
            _hookThread = new HookThread("KeyOverlayFPS.InputHook");
            
            // キーボードフックのイベントを購読
            _keyboardHook.KeyPressed += OnKeyPressed;
            _keyboardHook.KeyReleased += OnKeyReleased;
            
            // マウスフックのイベントを購読
            _mouseHook.MouseButtonPressed += OnMouseButtonPressed;
            _mouseHook.MouseButtonReleased += OnMouseButtonReleased;
            _mouseHook.MouseWheelDetected += OnMouseWheelDetected;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~InputStateManager()
        {
            Dispose(false);
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// キー状態管理を開始
        /// </summary>
        /// <remarks>
        /// キーボードとマウスの低レベルフックを、メッセージループ付きの専用スレッドで張る
        /// </remarks>
        /// <returns>開始に成功した場合true</returns>
        public bool Start()
        {
            if (_isEnabled)
            {
                return true; // 既に開始済み
            }

            try
            {
                bool keyboardSuccess = false;
                bool mouseSuccess = false;

                bool started = _hookThread.Start(
                    () =>
                    {
                        keyboardSuccess = _keyboardHook.StartHook();
                        mouseSuccess = _mouseHook.StartHook();
                        return keyboardSuccess && mouseSuccess;
                    },
                    () =>
                    {
                        // 部分的に成功したフックも含めて解除する（張られていなければ何もしない）
                        _keyboardHook.StopHook();
                        _mouseHook.StopHook();
                    },
                    HookThread.DefaultTimeout);

                if (started)
                {
                    _isEnabled = true;
                    Logger.Info("InputStateManager: 入力状態管理を開始しました");
                    return true;
                }

                Logger.Error($"InputStateManager: フックの開始に失敗しました - キーボード: {(keyboardSuccess ? "成功" : "失敗")}, マウス: {(mouseSuccess ? "成功" : "失敗")}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("InputStateManager.Start でエラーが発生", ex);
                return false;
            }
        }

        /// <summary>
        /// キー状態管理を停止
        /// </summary>
        public void Stop()
        {
            if (!_isEnabled)
            {
                return;
            }

            try
            {
                // フックはフックのスレッドの上で解除され、スレッドの終了を待つ
                _hookThread.Stop(HookThread.DefaultTimeout);
                _keyStates.Clear();
                _releasedSince.Clear();
                _isEnabled = false;
                Logger.Info("InputStateManager: 入力状態管理を停止しました");
            }
            catch (Exception ex)
            {
                Logger.Error("InputStateManager.Stop でエラーが発生", ex);
            }
        }

        /// <summary>
        /// 指定された仮想キーが現在押されているかを判定
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード</param>
        /// <returns>キーが押されている場合true</returns>
        public bool IsKeyPressed(int virtualKeyCode)
        {
            // 特殊キー（検出不可能）の場合は常にfalseを返す
            if (virtualKeyCode < 0)
            {
                return false;
            }

            return _keyStates.TryGetValue(virtualKeyCode, out bool isPressed) && isPressed;
        }

        /// <summary>
        /// 全てのキー状態をクリア
        /// </summary>
        public void ClearAllStates()
        {
            _keyStates.Clear();
            _releasedSince.Clear();
        }

        /// <summary>
        /// 押下中と記録しているキー・マウスボタンを、実際の状態と照合する
        /// </summary>
        /// <remarks>
        /// 離放イベントを取りこぼしても押下が残り続けないようにする。UI スレッド（表示タイマーの tick）から呼ぶ。
        /// 「離されている」状態が <see cref="ReconcileReleaseThresholdMs"/> 続いたキーだけを離したことにする。
        /// </remarks>
        public void ReconcileKeyStates()
        {
            long now = _getTickMs();

            foreach (var entry in _keyStates)
            {
                if (!entry.Value)
                {
                    continue;
                }

                int key = entry.Key;
                bool actuallyDown;
                try
                {
                    actuallyDown = _isKeyDown(key);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"InputStateManager.ReconcileKeyStates でエラーが発生: {ex.Message}");
                    continue;
                }

                if (actuallyDown)
                {
                    _releasedSince.TryRemove(key, out _);
                    continue;
                }

                long since = _releasedSince.GetOrAdd(key, now);
                if (now - since < ReconcileReleaseThresholdMs)
                {
                    continue;
                }

                // 押下のままのときだけ書き換える（照合の直後に来た押下を潰さない）
                if (_keyStates.TryUpdate(key, false, true))
                {
                    _releasedSince.TryRemove(key, out _);
                    KeyStateChanged?.Invoke(this, new KeyStateChangedEventArgs(key, false));
                }
            }
        }

        /// <summary>
        /// 管理が有効かどうかを取得
        /// </summary>
        public bool IsEnabled => _isEnabled && _keyboardHook.IsHookActive && _mouseHook.IsHookActive;

        /// <summary>
        /// 現在管理されているキーの数を取得
        /// </summary>
        public int TrackedKeyCount => _keyStates.Count;

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// GetAsyncKeyState の上位ビットでキーが押されているかを返す
        /// </summary>
        private static bool IsKeyDownByAsyncKeyState(int virtualKeyCode)
        {
            return (GetAsyncKeyState(virtualKeyCode) & 0x8000) != 0;
        }

        /// <summary>
        /// キー押下イベントハンドラー
        /// </summary>
        private void OnKeyPressed(object? sender, KeyboardEventArgs e)
        {
            try
            {
                bool previousState = _keyStates.TryGetValue(e.VirtualKeyCode, out bool current) && current;
                
                // キー状態を更新（押下状態にする）
                _keyStates.AddOrUpdate(e.VirtualKeyCode, true, (key, oldValue) => true);
                _releasedSince.TryRemove(e.VirtualKeyCode, out _);
                
                // 状態が変化した場合のみイベントを発火
                if (!previousState)
                {
                    KeyStateChanged?.Invoke(this, new KeyStateChangedEventArgs(e.VirtualKeyCode, true));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputStateManager.OnKeyPressed でエラーが発生: {ex.Message}");
            }
        }

        /// <summary>
        /// キー離放イベントハンドラー
        /// </summary>
        private void OnKeyReleased(object? sender, KeyboardEventArgs e)
        {
            try
            {
                bool previousState = _keyStates.TryGetValue(e.VirtualKeyCode, out bool current) && current;
                
                // キー状態を更新（離放状態にする）
                _keyStates.AddOrUpdate(e.VirtualKeyCode, false, (key, oldValue) => false);
                
                // 状態が変化した場合のみイベントを発火
                if (previousState)
                {
                    KeyStateChanged?.Invoke(this, new KeyStateChangedEventArgs(e.VirtualKeyCode, false));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputStateManager.OnKeyReleased でエラーが発生: {ex.Message}");
            }
        }

        /// <summary>
        /// マウスボタン押下イベントハンドラー
        /// </summary>
        private void OnMouseButtonPressed(object? sender, MouseButtonHookEventArgs e)
        {
            try
            {
                bool previousState = _keyStates.TryGetValue(e.VirtualKeyCode, out bool current) && current;
                
                // ボタン状態を更新（押下状態にする）
                _keyStates.AddOrUpdate(e.VirtualKeyCode, true, (key, oldValue) => true);
                _releasedSince.TryRemove(e.VirtualKeyCode, out _);
                
                // 状態が変化した場合のみイベントを発火
                if (!previousState)
                {
                    KeyStateChanged?.Invoke(this, new KeyStateChangedEventArgs(e.VirtualKeyCode, true));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputStateManager.OnMouseButtonPressed でエラーが発生: {ex.Message}");
            }
        }

        /// <summary>
        /// マウスボタン離放イベントハンドラー
        /// </summary>
        private void OnMouseButtonReleased(object? sender, MouseButtonHookEventArgs e)
        {
            try
            {
                bool previousState = _keyStates.TryGetValue(e.VirtualKeyCode, out bool current) && current;
                
                // ボタン状態を更新（離放状態にする）
                _keyStates.AddOrUpdate(e.VirtualKeyCode, false, (key, oldValue) => false);
                
                // 状態が変化した場合のみイベントを発火
                if (previousState)
                {
                    KeyStateChanged?.Invoke(this, new KeyStateChangedEventArgs(e.VirtualKeyCode, false));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputStateManager.OnMouseButtonReleased でエラーが発生: {ex.Message}");
            }
        }

        /// <summary>
        /// マウスホイールイベントハンドラー（そのまま転送する）
        /// </summary>
        private void OnMouseWheelDetected(object? sender, MouseWheelEventArgs e)
        {
            MouseWheelDetected?.Invoke(this, e);
        }

        #endregion

        #region DisposableBase実装

        /// <summary>
        /// マネージリソースの解放
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Stop();
            
            // イベントの購読解除
            _keyboardHook.KeyPressed -= OnKeyPressed;
            _keyboardHook.KeyReleased -= OnKeyReleased;
            _mouseHook.MouseButtonPressed -= OnMouseButtonPressed;
            _mouseHook.MouseButtonReleased -= OnMouseButtonReleased;
            _mouseHook.MouseWheelDetected -= OnMouseWheelDetected;
            
            _hookThread.Dispose();
            _keyboardHook?.Dispose();
            _mouseHook?.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// キー状態変化イベント引数
    /// </summary>
    public class KeyStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 仮想キーコード
        /// </summary>
        public int VirtualKeyCode { get; }

        /// <summary>
        /// キーが押されているかどうか
        /// </summary>
        public bool IsPressed { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード</param>
        /// <param name="isPressed">キー押下状態</param>
        public KeyStateChangedEventArgs(int virtualKeyCode, bool isPressed)
        {
            VirtualKeyCode = virtualKeyCode;
            IsPressed = isPressed;
        }
    }
}