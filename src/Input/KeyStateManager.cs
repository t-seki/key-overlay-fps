using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// キーボード状態管理クラス
    /// KeyboardHookからのイベントを受け取り、各キーの押下状態を管理する
    /// </summary>
    public class KeyStateManager : IDisposable
    {
        #region フィールド

        private readonly KeyboardHook _keyboardHook;
        private readonly ConcurrentDictionary<int, bool> _keyStates;
        private bool _disposed = false;
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
        /// KeyStateManagerクラスの新しいインスタンスを初期化
        /// </summary>
        public KeyStateManager()
        {
            _keyStates = new ConcurrentDictionary<int, bool>();
            _keyboardHook = new KeyboardHook();
            
            // キーボードフックのイベントを購読
            _keyboardHook.KeyPressed += OnKeyPressed;
            _keyboardHook.KeyReleased += OnKeyReleased;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~KeyStateManager()
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
                bool success = _keyboardHook.StartHook();
                if (success)
                {
                    _isEnabled = true;
                    Debug.WriteLine("KeyStateManager: キー状態管理を開始しました");
                }
                else
                {
                    Debug.WriteLine("KeyStateManager: キーボードフックの開始に失敗しました");
                }
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KeyStateManager.Start でエラーが発生: {ex.Message}");
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
                _keyStates.Clear();
                _isEnabled = false;
                Debug.WriteLine("KeyStateManager: キー状態管理を停止しました");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KeyStateManager.Stop でエラーが発生: {ex.Message}");
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
        public bool IsEnabled => _isEnabled && _keyboardHook.IsHookActive;

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
                Debug.WriteLine($"KeyStateManager.OnKeyPressed でエラーが発生: {ex.Message}");
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
                Debug.WriteLine($"KeyStateManager.OnKeyReleased でエラーが発生: {ex.Message}");
            }
        }

        #endregion

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
                    Stop();
                    
                    // イベントの購読解除
                    _keyboardHook.KeyPressed -= OnKeyPressed;
                    _keyboardHook.KeyReleased -= OnKeyReleased;
                    
                    _keyboardHook?.Dispose();
                }
                _disposed = true;
            }
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