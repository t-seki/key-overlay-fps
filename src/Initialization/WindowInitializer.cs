using System;
using KeyOverlayFPS.Initialization.Steps;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.Initialization
{
    /// <summary>
    /// MainWindowの初期化を管理するクラス
    /// </summary>
    public class WindowInitializer
    {
        private readonly IInitializationStep[] _steps;

        public WindowInitializer()
        {
            _steps = new IInitializationStep[]
            {
                new SettingsInitializer(),
                new LayoutInitializer(),
                new InputInitializer(),
                new MenuInitializer(),
                new FinalSetupInitializer()
            };
        }

        /// <summary>
        /// MainWindowを初期化
        /// </summary>
        /// <param name="window">初期化対象のMainWindow</param>
        public void Initialize(MainWindow window)
        {
            foreach (var step in _steps)
            {
                try
                {
                    Logger.Info($"{step.Name}開始");
                    step.Execute(window);
                    Logger.Info($"{step.Name}完了");
                }
                catch (Exception ex)
                {
                    Logger.Error($"{step.Name}でエラーが発生", ex);
                    throw new InitializationException(step.Name, ex);
                }
            }
            
            Logger.Info("MainWindow初期化完了");
        }
    }
}