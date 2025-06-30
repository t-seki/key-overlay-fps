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
            if (context.ElementLocator == null)
                throw new InitializationException(Name, "ElementLocatorが初期化されていません");
            if (context.MouseTracker == null)
                throw new InitializationException(Name, "MouseTrackerが初期化されていません");

            // ブラシを統一ファクトリーから初期化
            var keyboardKeyBackgroundBrush = BrushFactory.CreateKeyboardKeyBackground();
            
            // 入力処理管理システムを初期化
            context.Input = new MainWindowInput(window, context.Settings, context.KeyboardHandler, 
                context.MouseTracker, context.ElementLocator, window.LayoutManager, keyboardKeyBackgroundBrush);
            
            // 入力処理アクションを初期化
            context.Input.UpdateAllTextForegroundAction = window.UpdateAllTextForeground;
            
            // MainWindowのプロパティに設定
            window.Input = context.Input;
            
            // 入力処理開始
            context.Input.Start();
        }
    }
}