using KeyOverlayFPS.UI;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// 動的レイアウトシステムの初期化
    /// </summary>
    public class LayoutInitializer : IInitializationStep
    {
        private readonly CanvasRebuilder _canvasRebuilder;

        public string Name => "動的レイアウトシステム初期化";

        public LayoutInitializer()
        {
            _canvasRebuilder = new CanvasRebuilder();
        }

        public void Execute(MainWindow window)
        {
            // CanvasRebuilderを使用してキャンバスを再構築
            _canvasRebuilder.RebuildCanvas(window, window.ProfileManager.CurrentProfile);
        }
    }
}