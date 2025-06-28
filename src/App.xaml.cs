using System;
using System.Windows;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // ログシステム初期化
            Logger.Initialize();
            Logger.Info("アプリケーション開始");

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