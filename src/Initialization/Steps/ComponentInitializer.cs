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

        public void Execute(MainWindow window, InitializationContext context)
        {
            window.InitializeComponent();
            
            // ProfileManagerを初期化（SettingsManagerを渡す）
            context.ProfileManager = new ProfileManager(context.SettingsManager);
        }
    }
}