using System;
using System.Runtime.InteropServices;
using System.Threading;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Input
{
    /// <summary>
    /// 低レベルフックを張るための、メッセージループ付き専用スレッド
    /// </summary>
    /// <remarks>
    /// 低レベルフック（WH_KEYBOARD_LL / WH_MOUSE_LL）のコールバックは、フックを張ったスレッドの
    /// メッセージループの上で呼ばれる。UI スレッドで張ると、UI スレッドの同期 I/O や描画に
    /// コールバックが待たされ、LowLevelHooksTimeout を超えると Windows がフックを黙って外す。
    /// そこでフックの設定・メッセージループ・フックの解除を、すべてこの専用スレッドの上で行う。
    /// </remarks>
    public sealed class HookThread : IDisposable
    {
        #region Win32 API定義

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
            public uint lPrivate;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private const uint WM_QUIT = 0x0012;
        private const uint WM_USER = 0x0400;
        private const uint PM_NOREMOVE = 0x0000;

        #endregion

        #region フィールド

        /// <summary>
        /// Start / Stop で既定として使う待ち時間
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        private readonly string _name;
        private readonly object _lock = new object();
        private Thread? _thread;
        private ManualResetEventSlim? _started;
        private volatile uint _nativeThreadId;
        private volatile bool _installSucceeded;
        private volatile bool _stopRequested;
        private bool _disposed;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// HookThreadクラスの新しいインスタンスを初期化
        /// </summary>
        /// <param name="name">スレッド名（デバッグ用）</param>
        public HookThread(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// スレッドが動いているかどうか
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return _thread != null && _thread.IsAlive;
                }
            }
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 専用スレッドを起動し、その上でフックを設定してメッセージループを回す
        /// </summary>
        /// <param name="install">
        /// 専用スレッドの上で呼ばれる、フックを設定する処理。
        /// すべて成功したら true、1 つでも失敗したら false を返す
        /// </param>
        /// <param name="uninstall">
        /// 専用スレッドの上で、スレッドを終える前に必ず呼ばれる、フックを解除する処理。
        /// install が途中で失敗したときも呼ばれるので、張られていないフックに対しても安全であること
        /// </param>
        /// <param name="timeout">install の完了を待つ時間</param>
        /// <returns>install が時間内に成功した場合 true</returns>
        public bool Start(Func<bool> install, Action uninstall, TimeSpan timeout)
        {
            if (install == null) throw new ArgumentNullException(nameof(install));
            if (uninstall == null) throw new ArgumentNullException(nameof(uninstall));

            Thread thread;
            ManualResetEventSlim started;

            lock (_lock)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(HookThread));

                if (_thread != null)
                {
                    // 既に起動済み
                    return _installSucceeded;
                }

                _nativeThreadId = 0;
                _installSucceeded = false;
                _stopRequested = false;
                started = new ManualResetEventSlim(false);
                _started = started;

                thread = new Thread(() => Run(install, uninstall, started))
                {
                    IsBackground = true,
                    Name = _name
                };
                _thread = thread;
            }

            thread.Start();

            if (!started.Wait(timeout))
            {
                Logger.Error($"{_name}: フックの設定が {timeout.TotalSeconds} 秒以内に終わりませんでした");
                Stop(timeout);
                return false;
            }

            if (!_installSucceeded)
            {
                // install に失敗したスレッドは uninstall を済ませて自分で終わる
                Stop(timeout);
                return false;
            }

            return true;
        }

        /// <summary>
        /// メッセージループを抜けさせ、フックを解除してスレッドを終える
        /// </summary>
        /// <param name="timeout">スレッドの終了を待つ時間</param>
        /// <returns>スレッドが時間内に終了した（または起動していなかった）場合 true</returns>
        public bool Stop(TimeSpan timeout)
        {
            Thread? thread;
            ManualResetEventSlim? started;

            lock (_lock)
            {
                thread = _thread;
                started = _started;
                if (thread == null)
                {
                    return true;
                }

                _stopRequested = true;
            }

            // _stopRequested の書き込みと _installSucceeded の読み込みを入れ替えさせない
            // （スレッド側は _installSucceeded を書いてから _stopRequested を読む）
            Thread.MemoryBarrier();

            // install が成功していなければ、スレッドはメッセージループに入らずに終わる
            // （install の途中なら、終わった後に _stopRequested を見て抜ける）ので、WM_QUIT は要らない
            uint threadId = _nativeThreadId;
            if (_installSucceeded && threadId != 0 && thread.IsAlive
                && !PostThreadMessage(threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero))
            {
                Logger.Warning($"{_name}: WM_QUIT の送信に失敗しました（Win32 エラー {Marshal.GetLastWin32Error()}）");
            }

            if (!thread.Join(timeout))
            {
                Logger.Warning($"{_name}: スレッドが {timeout.TotalSeconds} 秒以内に終了しませんでした");
                return false;
            }

            lock (_lock)
            {
                if (ReferenceEquals(_thread, thread))
                {
                    _thread = null;
                    _started = null;
                }
            }

            started?.Dispose();
            return true;
        }

        /// <summary>
        /// スレッドを止めてリソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            Stop(DefaultTimeout);
            _disposed = true;
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// 専用スレッドの本体
        /// </summary>
        private void Run(Func<bool> install, Action uninstall, ManualResetEventSlim started)
        {
            try
            {
                _nativeThreadId = GetCurrentThreadId();

                // PostThreadMessage を受け取れるよう、先にメッセージキューを作っておく
                PeekMessage(out _, IntPtr.Zero, WM_USER, WM_USER, PM_NOREMOVE);

                bool installed = false;
                try
                {
                    installed = install();
                }
                catch (Exception ex)
                {
                    Logger.Error($"{_name}: フックの設定でエラーが発生", ex);
                }

                _installSucceeded = installed;
                Thread.MemoryBarrier();
                started.Set();

                if (installed && !_stopRequested)
                {
                    RunMessageLoop();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{_name}: フックのスレッドでエラーが発生", ex);
            }
            finally
            {
                try
                {
                    uninstall();
                }
                catch (Exception ex)
                {
                    Logger.Error($"{_name}: フックの解除でエラーが発生", ex);
                }

                // 例外で install の前に抜けたときも、Start を待たせない
                started.Set();
            }
        }

        /// <summary>
        /// WM_QUIT を受け取るまでメッセージループを回す
        /// </summary>
        private void RunMessageLoop()
        {
            while (true)
            {
                int result = GetMessage(out MSG msg, IntPtr.Zero, 0, 0);
                if (result == 0)
                {
                    // WM_QUIT
                    return;
                }

                if (result == -1)
                {
                    Logger.Error($"{_name}: GetMessage が失敗しました（Win32 エラー {Marshal.GetLastWin32Error()}）");
                    return;
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        #endregion
    }
}
