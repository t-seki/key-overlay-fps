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
            if (context.ProfileManager == null)
                throw new InitializationException(Name, "ProfileManagerが初期化されていません");
            if (context.MouseTracker == null)
                throw new InitializationException(Name, "MouseTrackerが初期化されていません");

            // プロファイルに応じたレイアウトファイルを読み込み
            Logger.Info($"レイアウトを読み込み中: {context.ProfileManager.CurrentProfile}");
            window.LayoutManager.LoadLayout(context.ProfileManager.CurrentProfile);

            // UIを動的生成
            context.DynamicCanvas = UIGenerator.GenerateCanvas(window.LayoutManager.CurrentLayout!, window);

            // 既存のCanvasと置き換え
            window.Content = context.DynamicCanvas;

            // ウィンドウ背景を設定
            window.Background = BrushFactory.CreateTransparentBackground();

            // 要素名を登録
            UIGenerator.RegisterElementNames(context.DynamicCanvas, window);

            // UI要素検索管理を初期化
            context.ElementLocator = new UIElementLocator();
            context.ElementLocator.BuildCache(context.DynamicCanvas);

            // マウス方向可視化を初期化
            context.MouseVisualizer = new MouseDirectionVisualizer(context.ElementLocator);
            context.MouseVisualizer.Initialize(context.MouseTracker);

            // MainWindowのプロパティに設定
            window.ElementLocator = context.ElementLocator;
            window.MouseVisualizer = context.MouseVisualizer;

            Logger.Info("動的レイアウトシステム初期化完了");
        }
    }
}