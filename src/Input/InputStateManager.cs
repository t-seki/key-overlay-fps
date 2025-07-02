using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        private readonly ConcurrentDictionary<int, bool> _keyStates;
        private bool _isEnabled = false;

        #endregion

        #region イベント

        /// <summary>
        /// キー状態が変化した時に発生するイベント
        /// </summary>
        public event EventHandler<KeyStateChangedEventArgs>? KeyStateChanged;

        #endregion

        #region コンストラクタ・デストラクタ

        /// <summary>
        /// InputStateManagerクラスの新しいインスタンスを初期化
        /// </summary>
        public InputStateManager()
        {
            _keyStates = new ConcurrentDictionary<int, bool>();
            _keyboardHook = new KeyboardHook();
            _mouseHook = new MouseHook();
            
            // キーボードフックのイベントを購読
            _keyboardHook.KeyPressed += OnKeyPressed;
            _keyboardHook.KeyReleased += OnKeyReleased;
            
            // マウスフックのイベントを購読
            _mouseHook.MouseButtonPressed += OnMouseButtonPressed;
            _mouseHook.MouseButtonReleased += OnMouseButtonReleased;
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
        /// <returns>開始に成功した場合true</returns>
        public bool Start()
        {
            if (_isEnabled)
            {
                return true; // 既に開始済み
            }

            try
            {
                bool keyboardSuccess = _keyboardHook.StartHook();
                bool mouseSuccess = _mouseHook.StartHook();
                
                if (keyboardSuccess && mouseSuccess)
                {
                    _isEnabled = true;
                    Debug.WriteLine("InputStateManager: 入力状態管理を開始しました");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"InputStateManager: フック開始に失敗 - Keyboard: {keyboardSuccess}, Mouse: {mouseSuccess}");
                    // 部分的に成功したフックを停止
                    if (keyboardSuccess) _keyboardHook.StopHook();
                    if (mouseSuccess) _mouseHook.StopHook();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputStateManager.Start でエラーが発生: {ex.Message}");
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
                _keyboardHook.StopHook();
                _mouseHook.StopHook();
                _keyStates.Clear();
                _isEnabled = false;
                Debug.WriteLine("InputStateManager: 入力状態管理を停止しました");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputStateManager.Stop でエラーが発生: {ex.Message}");
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
        /// キー押下イベントハンドラー
        /// </summary>
        private void OnKeyPressed(object? sender, KeyboardEventArgs e)
        {
            try
            {
                bool previousState = _keyStates.TryGetValue(e.VirtualKeyCode, out bool current) && current;
                
                // キー状態を更新（押下状態にする）
                _keyStates.AddOrUpdate(e.VirtualKeyCode, true, (key, oldValue) => true);
                
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