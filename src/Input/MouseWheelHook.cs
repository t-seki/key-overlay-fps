using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// マウスホイールのグローバル検知を担当するクラス
    /// Win32 低レベルマウスフックを使用してフォーカスに依存しないホイール入力を検知
    /// </summary>
    public class MouseWheelHook : IDisposable
    {
        #region Win32 API定義

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        #region 定数

        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int HC_ACTION = 0;

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
        private IntPtr _hookID = IntPtr.Zero;
        private bool _disposed = false;

        #endregion

        #region イベント

        /// <summary>
        /// マウスホイールが回転した時に発生するイベント
        /// </summary>
        public event EventHandler<MouseWheelEventArgs>? MouseWheelDetected;

        #endregion

        #region コンストラクタ・デストラクタ

        /// <summary>
        /// MouseWheelHookクラスの新しいインスタンスを初期化
        /// </summary>
        public MouseWheelHook()
        {
            _proc = HookCallback;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MouseWheelHook()
        {
            Dispose(false);
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// マウスホイールフックを開始
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
                            WH_MOUSE_LL,
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
                Debug.WriteLine($"MouseWheelHook.StartHook でエラーが発生: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// マウスホイールフックを停止
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
        /// マウスフックのコールバック処理
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= HC_ACTION && wParam == (IntPtr)WM_MOUSEWHEEL)
                {
                    var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    
                    // マウスホイールのdelta値を取得（上位16ビット）
                    var delta = (short)((hookStruct.mouseData >> 16) & 0xFFFF);
                    
                    // イベントを発火
                    MouseWheelDetected?.Invoke(this, new MouseWheelEventArgs(delta));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MouseWheelHook.HookCallback でエラーが発生: {ex.Message}");
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
}