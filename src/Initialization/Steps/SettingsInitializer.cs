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

        public void Execute(MainWindow window)
        {
            window.Settings.SettingsChanged += window.OnSettingsChanged;
            
            // 設定システム初期化
            window.Settings.Initialize();
        }
    }
}