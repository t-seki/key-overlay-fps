using System.Windows;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// 最終設定の適用
    /// </summary>
    public class FinalSetupInitializer : IInitializationStep
    {
        public string Name => "最終設定適用";

        public void Execute(MainWindow window, InitializationContext context)
        {
            if (context.Settings == null)
                throw new InitializationException(Name, "Settingsが初期化されていません");

            // アプリケーション終了時に設定を保存
            Application.Current.Exit += (s, e) => context.SettingsManager.Save();
            
            // 設定適用
            window.ApplyProfileLayout();
            window.ApplyDisplayScale();
            window.UpdateMousePositions();
            window.UpdateAllTextForeground();
            
            Logger.Info("最終設定適用完了");
        }
    }
}