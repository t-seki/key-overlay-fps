using KeyOverlayFPS.UI;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// 入力処理管理システムの初期化
    /// </summary>
    public class InputInitializer : IInitializationStep
    {
        public string Name => "入力処理システム初期化";

        public void Execute(MainWindow window, InitializationContext context)
        {
            if (context.Settings == null)
                throw new InitializationException(Name, "Settingsが初期化されていません");
            if (context.KeyboardHandler == null)
                throw new InitializationException(Name, "KeyboardHandlerが初期化されていません");
            if (context.EventBinder == null)
                throw new InitializationException(Name, "EventBinderが初期化されていません");

            // ブラシを統一ファクトリーから初期化
            var keyboardKeyBackgroundBrush = BrushFactory.CreateKeyboardKeyBackground();
            
            // 入力処理管理システムを初期化
            context.Input = new MainWindowInput(window, context.Settings, context.KeyboardHandler, 
                new MouseTracker(), context.EventBinder, keyboardKeyBackgroundBrush);
            
            // 入力処理アクションを初期化
            context.Input.UpdateAllTextForegroundAction = window.UpdateAllTextForeground;
            
            // MainWindowのプロパティに設定
            window.Input = context.Input;
            
            // 入力処理開始
            context.Input.Start();
            
            Logger.Info("入力処理システム初期化完了");
        }
    }
}