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

        public void Execute(MainWindow window)
        {
            // プロファイルに応じたレイアウトファイルを読み込み
            Logger.Info($"レイアウトを読み込み中: {window.ProfileManager.CurrentProfile}");
            window.LayoutManager.LoadLayout(window.ProfileManager.CurrentProfile);

            // UIを動的生成
            var dynamicCanvas = UIGenerator.GenerateCanvas(window.LayoutManager.CurrentLayout!, window);

            // 既存のCanvasと置き換え
            window.Content = dynamicCanvas;

            // ウィンドウ背景を設定
            window.Background = BrushFactory.CreateTransparentBackground();

            // 要素名を登録
            UIGenerator.RegisterElementNames(dynamicCanvas, window);

            // UI要素検索管理を初期化
            window.ElementLocator?.BuildCache(dynamicCanvas);

            // マウス方向可視化を初期化
            window.MouseVisualizer?.Initialize(window.MouseTracker);
        }
    }
}