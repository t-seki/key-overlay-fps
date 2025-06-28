using System.IO;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Utils;
using KeyOverlayFPS.Input;

namespace KeyOverlayFPS.Initialization.Steps
{
    /// <summary>
    /// 動的レイアウトシステムの初期化
    /// </summary>
    public class LayoutInitializer : IInitializationStep
    {
        public string Name => "動的レイアウトシステム初期化";

        public void Execute(MainWindow window, InitializationContext context)
        {
            if (context.KeyboardHandler == null)
                throw new InitializationException(Name, "KeyboardHandlerが初期化されていません");

            // プロファイルに応じたレイアウトファイルを読み込み
            string layoutPath = GetLayoutPath(context.KeyboardHandler.CurrentProfile);

            if (!File.Exists(layoutPath))
            {
                throw new FileNotFoundException($"必須レイアウトファイルが見つかりません: {layoutPath}");
            }

            Logger.Info($"レイアウトファイルを読み込み中: {layoutPath}");
            context.CurrentLayout = LayoutManager.ImportLayout(layoutPath);

            // UIを動的生成
            context.DynamicCanvas = UIGenerator.GenerateCanvas(context.CurrentLayout, window);

            // 既存のCanvasと置き換え
            window.Content = context.DynamicCanvas;

            // ウィンドウ背景を設定
            window.Background = BrushFactory.CreateTransparentBackground();

            // 要素名を登録
            UIGenerator.RegisterElementNames(context.DynamicCanvas, window);

            // KeyboardInputHandlerにLayoutConfigを設定
            context.KeyboardHandler.SetLayoutConfig(context.CurrentLayout);

            // イベントバインディング
            context.EventBinder = new KeyEventBinder(context.DynamicCanvas, context.CurrentLayout, 
                context.KeyboardHandler, new MouseTracker());
            context.EventBinder.BindAllEvents();

            // MainWindowのプロパティに設定
            window.CurrentLayout = context.CurrentLayout;
            window.EventBinder = context.EventBinder;
            window.DynamicCanvas = context.DynamicCanvas;

            Logger.Info("動的レイアウトシステム初期化完了");
        }
        
        /// <summary>
        /// プロファイルに応じたレイアウトファイルパスを取得
        /// </summary>
        private string GetLayoutPath(KeyboardProfile profile)
        {
            return profile switch
            {
                KeyboardProfile.FPSKeyboard => ApplicationConstants.Paths.FpsLayout,
                _ => ApplicationConstants.Paths.Keyboard65Layout
            };
        }
    }
}