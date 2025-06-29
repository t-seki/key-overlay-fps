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
            if (context.MouseTracker == null)
                throw new InitializationException(Name, "MouseTrackerが初期化されていません");

            // プロファイルに応じたレイアウトファイルを読み込み
            Logger.Info($"レイアウトを読み込み中: {context.KeyboardHandler.CurrentProfile}");
            window.LayoutManager.LoadLayout(context.KeyboardHandler.CurrentProfile);

            // UIを動的生成
            context.DynamicCanvas = UIGenerator.GenerateCanvas(window.LayoutManager.CurrentLayout!, window);

            // 既存のCanvasと置き換え
            window.Content = context.DynamicCanvas;

            // ウィンドウ背景を設定
            window.Background = BrushFactory.CreateTransparentBackground();

            // 要素名を登録
            UIGenerator.RegisterElementNames(context.DynamicCanvas, window);

            // イベントバインディング
            context.EventBinder = new KeyEventBinder(context.DynamicCanvas, window.LayoutManager.CurrentLayout!, 
                context.KeyboardHandler, context.MouseTracker);
            context.EventBinder.BindAllEvents();

            // MainWindowのプロパティに設定
            window.EventBinder = context.EventBinder;

            Logger.Info("動的レイアウトシステム初期化完了");
        }
    }
}