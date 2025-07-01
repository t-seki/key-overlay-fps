using System.Windows;
using System.Windows.Controls;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// キャンバス再構築処理を担当するクラス
    /// 初期化時とプロファイル切り替え時の共通処理を提供
    /// </summary>
    public class CanvasRebuilder
    {
        /// <summary>
        /// キャンバスを完全に再構築する
        /// </summary>
        /// <param name="window">対象のMainWindow</param>
        /// <param name="profile">適用するプロファイル</param>
        public void RebuildCanvas(MainWindow window, KeyboardProfile profile)
        {
            Logger.Info($"キャンバス再構築開始: {profile}");

            // 既存のCanvas要素の名前を解除
            if (window.Content is Canvas oldCanvas)
            {
                UIGenerator.UnregisterElementNames(oldCanvas, window);
            }

            // プロファイルに応じたレイアウトファイルを読み込み
            Logger.Info($"レイアウトを読み込み中: {profile}");
            window.LayoutManager.LoadLayout(profile);

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

            // 表示スケールを適用
            window.ApplyDisplayScale();

            Logger.Info($"キャンバス再構築完了: {profile}");
        }
    }
}