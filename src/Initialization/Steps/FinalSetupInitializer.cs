using System;
using System.Windows;
using KeyOverlayFPS.Utils;
using KeyOverlayFPS.Input;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// 最終設定の適用
    /// </summary>
    public class FinalSetupInitializer : IInitializationStep
    {
        public string Name => "最終設定適用";

        public void Execute(MainWindow window)
        {
            if (window.Settings == null)
                throw new InitializationException(Name, "Settingsが初期化されていません");
            if (window.ProfileManager == null)
                throw new InitializationException(Name, "ProfileManagerが初期化されていません");

            // アプリケーション終了時に設定を保存
            Application.Current.Exit += (s, e) => {
                window.Settings?.SaveSettings();
                Logger.Info("アプリケーション終了時の設定保存完了");
            };
            
            // UI管理クラスを初期化
            window.InitializeUIManagers();
            window.InitializeVisibilityController();
            
            // 保存されたプロファイル設定を復元
            RestoreProfileSettings(window);
            
            // 設定適用
            window.ApplyProfileLayout();
            window.ApplyDisplayScale();
            window.UpdateMousePositions();
            window.UpdateAllTextForeground();
        }

        /// <summary>
        /// 保存されたプロファイル設定を復元
        /// </summary>
        private void RestoreProfileSettings(MainWindow window)
        {
            try
            {
                // ProfileManagerは自動的にファイルから設定を読み込んでいる
                if (window.ProfileManager != null)
                {
                    Logger.Info($"プロファイル設定を復元: {window.ProfileManager.CurrentProfile}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("プロファイル設定復元でエラー", ex);
            }
        }
    }
}