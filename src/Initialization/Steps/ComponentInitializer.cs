using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// WPFコンポーネントの初期化
    /// </summary>
    public class ComponentInitializer : IInitializationStep
    {
        public string Name => "WPFコンポーネント初期化";

        public void Execute(MainWindow window, InitializationContext context)
        {
            window.InitializeComponent();
            Logger.Info("InitializeComponent完了");
        }
    }
}