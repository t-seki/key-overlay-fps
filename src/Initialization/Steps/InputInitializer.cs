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

        public void Execute(MainWindow window)
        {
            if (window.Settings == null)
                throw new InitializationException(Name, "Settingsが初期化されていません");
            if (window.KeyboardInputHandler == null)
                throw new InitializationException(Name, "KeyboardInputHandlerが初期化されていません");
            if (window.ElementLocator == null)
                throw new InitializationException(Name, "ElementLocatorが初期化されていません");
            if (window.MouseTracker == null)
                throw new InitializationException(Name, "MouseTrackerが初期化されていません");
            
            // 入力処理アクションを初期化
            window.Input.UpdateAllTextForegroundAction = window.UpdateAllTextForeground;
            
            // 入力処理開始
            window.Input.Start();
        }
    }
}