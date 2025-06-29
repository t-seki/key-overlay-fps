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

        public void Execute(MainWindow window, InitializationContext context)
        {
            if (context.Settings == null)
                throw new InitializationException(Name, "Settingsが初期化されていません");
            if (context.KeyboardHandler == null)
                throw new InitializationException(Name, "KeyboardHandlerが初期化されていません");

            // メニュー管理システムを初期化
            context.Menu = new MainWindowMenu(window, context.Settings, context.KeyboardHandler);
            window.InitializeMenuActions(context.Menu);
            
            // MainWindowのプロパティに設定
            window.Menu = context.Menu;
            
            // イベントハンドラーはXAMLで設定済みのため、
            // プログラム的な登録は不要
            
            // コンテキストメニュー設定
            context.Menu.SetupContextMenu();
            
            Logger.Info("メニューとイベント初期化完了");
        }
    }
}