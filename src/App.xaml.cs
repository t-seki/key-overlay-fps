using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS
{
    public partial class App : Application
    {
        // DPI認識の確認用 Win32 API 定義
        [DllImport("user32.dll")]
        private static extern IntPtr GetThreadDpiAwarenessContext();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA, IntPtr dpiContextB);

        private static readonly IntPtr DpiContextUnaware = new IntPtr(-1);
        private static readonly IntPtr DpiContextSystemAware = new IntPtr(-2);
        private static readonly IntPtr DpiContextPerMonitorAware = new IntPtr(-3);
        private static readonly IntPtr DpiContextPerMonitorAwareV2 = new IntPtr(-4);

        protected override void OnStartup(StartupEventArgs e)
        {
            // ログシステム初期化。Initialize は既存のログファイルを削除するので、ほかのログより先に呼ぶ
            Logger.Initialize();
            Logger.Info("アプリケーション開始");

            // DPI認識はマニフェスト（app.manifest）で指定している。実際の認識をログに残す
            LogDpiAwareness();

            // 未処理例外のハンドリング
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += App_UnhandledException;

            try
            {
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Logger.Error("OnStartup中にエラーが発生", ex);
                throw;
            }

            // StartupUri は使わず、生成の失敗を捕まえられるようにここでウィンドウを作る
            MainWindow window;
            try
            {
                window = new MainWindow();
                MainWindow = window;
                window.Show();
            }
            catch (Exception ex)
            {
                Logger.Error("起動時に致命的なエラーが発生、アプリケーションを終了", ex);
                MessageBox.Show(
                    "KeyOverlayFPS を起動できませんでした。\n\n" +
                    $"詳しくはログを確認してください: {Logger.LogFilePath}\n\n" +
                    $"エラー: {ex.Message}",
                    "KeyOverlayFPS - 起動エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // ウィンドウの無いプロセスを残さない
                Shutdown(1);
                return;
            }

            NotifySettingsRecovery(window);
        }

        /// <summary>
        /// 実際の DPI 認識をログに出す
        /// </summary>
        /// <remarks>
        /// DPI 認識はマニフェストで決まる。マニフェストの指定が効いているかを、debug.log で確かめるために残している。
        /// API を呼べなくても起動は続ける
        /// </remarks>
        private static void LogDpiAwareness()
        {
            try
            {
                IntPtr context = GetThreadDpiAwarenessContext();
                string name;
                if (AreDpiAwarenessContextsEqual(context, DpiContextPerMonitorAwareV2))
                {
                    name = "Per-Monitor V2";
                }
                else if (AreDpiAwarenessContextsEqual(context, DpiContextPerMonitorAware))
                {
                    name = "Per-Monitor";
                }
                else if (AreDpiAwarenessContextsEqual(context, DpiContextSystemAware))
                {
                    name = "System";
                }
                else if (AreDpiAwarenessContextsEqual(context, DpiContextUnaware))
                {
                    name = "Unaware";
                }
                else
                {
                    name = "不明";
                }

                Logger.Info($"DPI認識: {name}");
            }
            catch (Exception ex)
            {
                Logger.Warning("DPI認識を取得できませんでした", ex);
            }
        }

        /// <summary>
        /// 壊れた設定ファイルを既定値で復旧していたら、ウィンドウを表示した後にユーザーへ知らせる
        /// </summary>
        /// <param name="window">表示済みのメインウィンドウ</param>
        private static void NotifySettingsRecovery(MainWindow window)
        {
            var recovery = window.SettingsManager.Recovery;
            if (recovery == null)
            {
                return;
            }

            try
            {
                MessageBox.Show(
                    window,
                    BuildRecoveryMessage(recovery),
                    "KeyOverlayFPS - 設定の初期化",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Logger.Error("設定の初期化の通知でエラーが発生", ex);
            }
        }

        /// <summary>
        /// 設定の復旧を知らせる文面を作る
        /// </summary>
        /// <param name="recovery">復旧の結果</param>
        /// <returns>通知の文面</returns>
        private static string BuildRecoveryMessage(SettingsRecoveryInfo recovery)
        {
            if (recovery.BackupSucceeded)
            {
                return "設定ファイルを読み込めなかったため、設定を初期化しました。\n\n" +
                       $"元のファイルは {recovery.BackupPath} に保存しています。";
            }

            return "設定ファイルを読み込めなかったため、既定の設定で起動しました。\n\n" +
                   $"元のファイルを {recovery.BackupPath} に退避できなかったため、{recovery.SettingsPath} にそのまま残しています。\n" +
                   "元のファイルを上書きしないよう、今回の起動中は設定を保存しません。";
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error("UI スレッドで未処理例外が発生", e.Exception);
            
            // 重大なエラーの場合はアプリケーションを終了
            if (e.Exception is OutOfMemoryException || e.Exception is StackOverflowException)
            {
                Logger.Error("重大なエラーのためアプリケーションを終了");
                return;
            }

            // その他のエラーは処理済みとしてマークして続行
            e.Handled = true;
            Logger.Info("例外を処理済みとしてマーク、アプリケーション続行");
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error("非UIスレッドで未処理例外が発生", e.ExceptionObject as Exception);
            Logger.Info($"アプリケーション終了中: {e.IsTerminating}");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("アプリケーション終了");
            base.OnExit(e);
        }
    }
}