using System;
using System.Windows;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Utils;
using KeyOverlayFPS.Settings;

namespace KeyOverlayFPS.Initialization
{
    /// <summary>
    /// MainWindowの初期化を管理するクラス
    /// </summary>
    public class WindowInitializer
    {
        private CanvasRebuilder? _canvasRebuilder;

        public WindowInitializer()
        {
        }

        /// <summary>
        /// MainWindowを初期化
        /// </summary>
        /// <param name="window">初期化対象のMainWindow</param>
        public void Initialize(MainWindow window)
        {
            try
            {
                // 1. 設定管理システム初期化
                Logger.Info("設定管理システム初期化開始");
                InitializeSettings(window);
                Logger.Info("設定管理システム初期化完了");

                // 2. 動的レイアウトシステム初期化
                Logger.Info("動的レイアウトシステム初期化開始");
                InitializeLayout(window);
                Logger.Info("動的レイアウトシステム初期化完了");

                // 3. 入力処理システム初期化
                Logger.Info("入力処理システム初期化開始");
                InitializeInput(window);
                Logger.Info("入力処理システム初期化完了");

                // 4. メニューとイベント初期化
                Logger.Info("メニューとイベント初期化開始");
                InitializeMenu(window);
                Logger.Info("メニューとイベント初期化完了");

                // 5. 最終設定適用
                Logger.Info("最終設定適用開始");
                ApplyFinalSetup(window);
                Logger.Info("最終設定適用完了");
            }
            catch (Exception ex)
            {
                Logger.Error("MainWindow初期化でエラーが発生", ex);
                throw new InitializationException("MainWindow初期化", ex);
            }
            
            Logger.Info("MainWindow初期化完了");
        }

        /// <summary>
        /// 設定管理システムの初期化
        /// </summary>
        private void InitializeSettings(MainWindow window)
        {
            window.Settings.SettingsChanged += window.OnSettingsChanged;
            window.Settings.Initialize();
        }

        /// <summary>
        /// 動的レイアウトシステムの初期化
        /// </summary>
        private void InitializeLayout(MainWindow window)
        {
            // SettingsManagerをプロパティから取得してCanvasRebuilderを作成
            _canvasRebuilder = new CanvasRebuilder(window.SettingsManager);
            _canvasRebuilder.RebuildCanvas(window, window.ProfileManager.CurrentProfile);
        }

        /// <summary>
        /// 入力処理管理システムの初期化
        /// </summary>
        private void InitializeInput(MainWindow window)
        {
            window.Input.UpdateAllTextForegroundAction = window.UpdateAllTextForeground;
            window.Input.Start();
        }

        /// <summary>
        /// メニューとイベントハンドラーの初期化
        /// </summary>
        private void InitializeMenu(MainWindow window)
        {
            window.InitializeMenuActions(window.Menu);
            window.Menu.SetupContextMenu();
        }

        /// <summary>
        /// 最終設定の適用
        /// </summary>
        private void ApplyFinalSetup(MainWindow window)
        {
            // アプリケーション終了時に設定を保存
            Application.Current.Exit += (s, e) => {
                window.Settings?.SaveSettings();
                Logger.Info("アプリケーション終了時の設定保存完了");
            };
            
            // UI管理クラスを初期化
            window.InitializeUIManagers();
            
            // 保存されたプロファイル設定を復元
            RestoreProfileSettings(window);
            
            // 設定オーバーライドを適用（表示スケール、マウス位置、テキスト色も含む）
            window.Settings?.ApplySettingsOverride();
        }

        /// <summary>
        /// 保存されたプロファイル設定を復元
        /// </summary>
        private void RestoreProfileSettings(MainWindow window)
        {
            try
            {
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