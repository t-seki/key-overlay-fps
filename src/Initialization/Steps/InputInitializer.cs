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
            // 入力処理アクションを初期化
            window.Input.UpdateAllTextForegroundAction = window.UpdateAllTextForeground;
            
            // 入力処理開始
            window.Input.Start();
        }
    }
}