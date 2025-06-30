using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// キーボード入力のグローバル検知を担当するクラス
    /// Win32 低レベルキーボードフックを使用してフォーカスに依存しないキー入力を検知
    /// </summary>
    public class KeyboardHook : IDisposable
    {
        #region Win32 API定義

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        #region 定数

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int HC_ACTION = 0;

        #endregion

        #region 構造体

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region フィールド

        private readonly LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private bool _disposed = false;

        #endregion

        #region イベント

        /// <summary>
        /// キーが押された時に発生するイベント
        /// </summary>
        public event EventHandler<KeyboardEventArgs>? KeyPressed;

        /// <summary>
        /// キーが離された時に発生するイベント
        /// </summary>
        public event EventHandler<KeyboardEventArgs>? KeyReleased;

        #endregion

        #region コンストラクタ・デストラクタ

        /// <summary>
        /// KeyboardHookクラスの新しいインスタンスを初期化
        /// </summary>
        public KeyboardHook()
        {
            _proc = HookCallback;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~KeyboardHook()
        {
            Dispose(false);
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// キーボードフックを開始
        /// </summary>
        /// <returns>フック設定に成功した場合true</returns>
        public bool StartHook()
        {
            if (_hookID != IntPtr.Zero)
            {
                return true; // 既に開始済み
            }

            try
            {
                using (var curProcess = Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    if (curModule?.ModuleName != null)
                    {
                        _hookID = SetWindowsHookEx(
                            WH_KEYBOARD_LL,
                            _proc,
                            GetModuleHandle(curModule.ModuleName),
                            0
                        );
                    }
                }

                return _hookID != IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KeyboardHook.StartHook でエラーが発生: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// キーボードフックを停止
        /// </summary>
        public void StopHook()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        /// <summary>
        /// フックが有効かどうかを取得
        /// </summary>
        public bool IsHookActive => _hookID != IntPtr.Zero;

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// キーボードフックのコールバック処理
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= HC_ACTION)
                {
                    var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    var vkCode = (int)hookStruct.vkCode;
                    
                    bool isKeyDown = (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN);
                    bool isExtended = (hookStruct.flags & 0x01) != 0;
                    
                    var eventArgs = new KeyboardEventArgs(vkCode, isKeyDown, isExtended);
                    
                    if (isKeyDown)
                    {
                        KeyPressed?.Invoke(this, eventArgs);
                    }
                    else
                    {
                        KeyReleased?.Invoke(this, eventArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KeyboardHook.HookCallback でエラーが発生: {ex.Message}");
            }

            // 次のフックプロシージャに処理を渡す
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
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
                StopHook();
                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// キーボードイベント引数
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// 仮想キーコード
        /// </summary>
        public int VirtualKeyCode { get; }

        /// <summary>
        /// キーが押されているかどうか（true=押下、false=離放）
        /// </summary>
        public bool IsKeyDown { get; }

        /// <summary>
        /// 拡張キーかどうか
        /// </summary>
        public bool IsExtendedKey { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="virtualKeyCode">仮想キーコード</param>
        /// <param name="isKeyDown">キー押下状態</param>
        /// <param name="isExtendedKey">拡張キー</param>
        public KeyboardEventArgs(int virtualKeyCode, bool isKeyDown, bool isExtendedKey)
        {
            VirtualKeyCode = virtualKeyCode;
            IsKeyDown = isKeyDown;
            IsExtendedKey = isExtendedKey;
        }
    }
}