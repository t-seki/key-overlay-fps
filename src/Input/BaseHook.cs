using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// Win32フックの基底クラス
    /// KeyboardHookとMouseHookの共通機能を提供
    /// </summary>
    public abstract class BaseHook : IDisposable
    {
        #region Win32 API定義（共通）

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr GetModuleHandle(string? lpModuleName);

        protected const int HC_ACTION = 0;

        #endregion

        #region フィールド

        protected IntPtr _hookID = IntPtr.Zero;
        private bool _disposed = false;

        #endregion

        #region プロパティ

        /// <summary>
        /// フックがアクティブかどうか
        /// </summary>
        public bool IsHookActive => _hookID != IntPtr.Zero;

        #endregion

        #region 抽象メソッド

        /// <summary>
        /// フック固有のID取得
        /// </summary>
        protected abstract int GetHookType();

        /// <summary>
        /// フック固有のデリゲート取得
        /// </summary>
        protected abstract Delegate GetHookDelegate();

        #endregion

        #region フック管理

        /// <summary>
        /// フックを開始
        /// </summary>
        public virtual bool StartHook()
        {
            if (IsHookActive)
            {
                Debug.WriteLine("フックは既にアクティブです");
                return true;
            }

            try
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                
                if (curModule?.ModuleName != null)
                {
                    _hookID = SetWindowsHookEx(
                        GetHookType(),
                        GetHookDelegate(),
                        GetModuleHandle(curModule.ModuleName),
                        0
                    );
                }

                if (_hookID == IntPtr.Zero)
                {
                    Debug.WriteLine("フックの設定に失敗しました");
                    return false;
                }

                Debug.WriteLine("フックが正常に設定されました");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"フック設定でエラーが発生: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// フックを停止
        /// </summary>
        public virtual void StopHook()
        {
            if (!IsHookActive)
            {
                return;
            }

            try
            {
                if (UnhookWindowsHookEx(_hookID))
                {
                    _hookID = IntPtr.Zero;
                    Debug.WriteLine("フックが正常に解除されました");
                }
                else
                {
                    Debug.WriteLine("フック解除に失敗しました");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"フック解除でエラーが発生: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable

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
                    StopHook();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// ファイナライザ
        /// </summary>
        ~BaseHook()
        {
            Dispose(false);
        }

        #endregion
    }
}