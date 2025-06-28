using KeyOverlayFPS.UI;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// 設定管理システムの初期化
    /// </summary>
    public class SettingsInitializer : IInitializationStep
    {
        public string Name => "設定管理システム初期化";

        public void Execute(MainWindow window, InitializationContext context)
        {
            // キーボード入力ハンドラーを初期化
            context.KeyboardHandler = new KeyboardInputHandler();
            
            // マウストラッカーを初期化
            context.MouseTracker = new MouseTracker();
            
            // MainWindow設定管理を初期化
            context.Settings = new MainWindowSettings(window, context.SettingsManager);
            context.Settings.SettingsChanged += window.OnSettingsChanged;
            
            // MainWindowのプロパティに設定
            window.Settings = context.Settings;
            
            // 設定システム初期化
            context.Settings.Initialize();
            
            Logger.Info("設定管理システム初期化完了");
        }
    }
}