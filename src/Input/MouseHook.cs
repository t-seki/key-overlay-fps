using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// マウス入力のグローバル検知を担当するクラス
    /// Win32 低レベルマウスフックを使用してフォーカスに依存しないマウス入力を検知
    /// </summary>
    public class MouseHook : BaseHook
    {
        #region デリゲート定義

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        #region 定数

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;

        #endregion

        #region 構造体

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region フィールド

        private readonly LowLevelMouseProc _proc;
        
        // マウスボタン状態管理
        private readonly ConcurrentDictionary<int, bool> _buttonStates;

        #endregion

        #region イベント

        /// <summary>
        /// マウスホイールが回転した時に発生するイベント
        /// </summary>
        public event EventHandler<MouseWheelEventArgs>? MouseWheelDetected;
        
        /// <summary>
        /// マウスボタンが押された時に発生するイベント
        /// </summary>
        public event EventHandler<MouseButtonHookEventArgs>? MouseButtonPressed;
        
        /// <summary>
        /// マウスボタンが離された時に発生するイベント
        /// </summary>
        public event EventHandler<MouseButtonHookEventArgs>? MouseButtonReleased;

        #endregion

        #region コンストラクタ・デストラクタ

        /// <summary>
        /// MouseHookクラスの新しいインスタンスを初期化
        /// </summary>
        public MouseHook()
        {
            _proc = HookCallback;
            _buttonStates = new ConcurrentDictionary<int, bool>();
        }

        #endregion

        #region BaseHook実装

        /// <summary>
        /// マウスフック固有のID取得
        /// </summary>
        protected override int GetHookType()
        {
            return WH_MOUSE_LL;
        }

        /// <summary>
        /// マウスフック固有のデリゲート取得
        /// </summary>
        protected override Delegate GetHookDelegate()
        {
            return _proc;
        }

        /// <summary>
        /// フック停止時の追加処理
        /// </summary>
        public override void StopHook()
        {
            base.StopHook();
            _buttonStates.Clear();
        }

        #endregion

        #region ユーティリティメソッド
        /// <summary>
        /// 指定されたマウスボタンが現在押されているかを判定
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード</param>
        /// <returns>ボタンが押されている場合true</returns>
        public bool IsButtonPressed(int virtualKeyCode)
        {
            return _buttonStates.TryGetValue(virtualKeyCode, out bool isPressed) && isPressed;
        }
        
        /// <summary>
        /// 全てのボタン状態をクリア
        /// </summary>
        public void ClearAllButtonStates()
        {
            _buttonStates.Clear();
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// マウスフックのコールバック処理
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= BaseHook.HC_ACTION)
                {
                    var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    var message = wParam.ToInt32();
                    
                    switch (message)
                    {
                        case WM_MOUSEWHEEL:
                            // マウスホイールのdelta値を取得（上位16ビット）
                            var delta = (short)((hookStruct.mouseData >> 16) & 0xFFFF);
                            MouseWheelDetected?.Invoke(this, new MouseWheelEventArgs(delta));
                            break;
                            
                        case WM_LBUTTONDOWN:
                            _buttonStates.AddOrUpdate(VirtualKeyCodes.VK_LBUTTON, true, (key, oldValue) => true);
                            MouseButtonPressed?.Invoke(this, new MouseButtonHookEventArgs(VirtualKeyCodes.VK_LBUTTON));
                            break;
                            
                        case WM_LBUTTONUP:
                            _buttonStates.AddOrUpdate(VirtualKeyCodes.VK_LBUTTON, false, (key, oldValue) => false);
                            MouseButtonReleased?.Invoke(this, new MouseButtonHookEventArgs(VirtualKeyCodes.VK_LBUTTON));
                            break;
                            
                        case WM_RBUTTONDOWN:
                            _buttonStates.AddOrUpdate(VirtualKeyCodes.VK_RBUTTON, true, (key, oldValue) => true);
                            MouseButtonPressed?.Invoke(this, new MouseButtonHookEventArgs(VirtualKeyCodes.VK_RBUTTON));
                            break;
                            
                        case WM_RBUTTONUP:
                            _buttonStates.AddOrUpdate(VirtualKeyCodes.VK_RBUTTON, false, (key, oldValue) => false);
                            MouseButtonReleased?.Invoke(this, new MouseButtonHookEventArgs(VirtualKeyCodes.VK_RBUTTON));
                            break;
                            
                        case WM_MBUTTONDOWN:
                            _buttonStates.AddOrUpdate(VirtualKeyCodes.VK_MBUTTON, true, (key, oldValue) => true);
                            MouseButtonPressed?.Invoke(this, new MouseButtonHookEventArgs(VirtualKeyCodes.VK_MBUTTON));
                            break;
                            
                        case WM_MBUTTONUP:
                            _buttonStates.AddOrUpdate(VirtualKeyCodes.VK_MBUTTON, false, (key, oldValue) => false);
                            MouseButtonReleased?.Invoke(this, new MouseButtonHookEventArgs(VirtualKeyCodes.VK_MBUTTON));
                            break;
                            
                        case WM_XBUTTONDOWN:
                            // Xボタンの種類を取得（上位16ビット）
                            var xButtonDown = (short)((hookStruct.mouseData >> 16) & 0xFFFF);
                            var xButtonVkDown = xButtonDown == 1 ? VirtualKeyCodes.VK_XBUTTON1 : VirtualKeyCodes.VK_XBUTTON2;
                            _buttonStates.AddOrUpdate(xButtonVkDown, true, (key, oldValue) => true);
                            MouseButtonPressed?.Invoke(this, new MouseButtonHookEventArgs(xButtonVkDown));
                            break;
                            
                        case WM_XBUTTONUP:
                            // Xボタンの種類を取得（上位16ビット）
                            var xButtonUp = (short)((hookStruct.mouseData >> 16) & 0xFFFF);
                            var xButtonVkUp = xButtonUp == 1 ? VirtualKeyCodes.VK_XBUTTON1 : VirtualKeyCodes.VK_XBUTTON2;
                            _buttonStates.AddOrUpdate(xButtonVkUp, false, (key, oldValue) => false);
                            MouseButtonReleased?.Invoke(this, new MouseButtonHookEventArgs(xButtonVkUp));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MouseHook.HookCallback でエラーが発生: {ex.Message}");
            }

            // 次のフックプロシージャに処理を渡す
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        #endregion

    }

    /// <summary>
    /// マウスホイールイベント引数
    /// </summary>
    public class MouseWheelEventArgs : EventArgs
    {
        /// <summary>
        /// ホイール回転量（正数=上方向、負数=下方向）
        /// </summary>
        public int Delta { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="delta">ホイール回転量</param>
        public MouseWheelEventArgs(int delta)
        {
            Delta = delta;
        }
    }
    
    /// <summary>
    /// マウスボタンフックイベント引数
    /// </summary>
    public class MouseButtonHookEventArgs : EventArgs
    {
        /// <summary>
        /// 仮想キーコード
        /// </summary>
        public int VirtualKeyCode { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード</param>
        public MouseButtonHookEventArgs(int virtualKeyCode)
        {
            VirtualKeyCode = virtualKeyCode;
        }
    }
}