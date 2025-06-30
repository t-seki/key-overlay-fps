using KeyOverlayFPS.Utils;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// WPFコンポーネントの初期化
    /// </summary>
    public class ComponentInitializer : IInitializationStep
    {
        public string Name => "WPFコンポーネント初期化";

        public void Execute(MainWindow window)
        {
            window.InitializeComponent();
        }
    }
}