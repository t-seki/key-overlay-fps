using KeyOverlayFPS.UI;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// メニューとイベントハンドラーの初期化
    /// </summary>
    public class MenuInitializer : IInitializationStep
    {
        public string Name => "メニューとイベント初期化";

        public void Execute(MainWindow window)
        {
            // メニュー管理システムを初期化
            window.InitializeMenuActions(window.Menu);
            
            // イベントハンドラーはXAMLで設定済みのため、
            // プログラム的な登録は不要
            
            // コンテキストメニュー設定
            window.Menu.SetupContextMenu();
        }
    }
}