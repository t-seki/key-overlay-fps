using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.MouseVisualization;
using System.Windows.Controls;

namespace KeyOverlayFPS.Initialization
{
    /// <summary>
    /// 初期化プロセス中に共有されるコンテキスト
    /// </summary>
    public class InitializationContext
    {
        
        /// <summary>
        /// 設定管理システム
        /// </summary>
        public SettingsManager SettingsManager { get; set; } = SettingsManager.Instance;
        
        /// <summary>
        /// MainWindow設定管理
        /// </summary>
        public MainWindowSettings? Settings { get; set; }
        
        /// <summary>
        /// キーボード入力ハンドラー
        /// </summary>
        public KeyboardInputHandler? KeyboardHandler { get; set; }
        
        /// <summary>
        /// メニュー管理
        /// </summary>
        public MainWindowMenu? Menu { get; set; }
        
        
        /// <summary>
        /// UI要素検索管理
        /// </summary>
        public UIElementLocator? ElementLocator { get; set; }
        
        /// <summary>
        /// マウス方向可視化管理
        /// </summary>
        public MouseDirectionVisualizer? MouseVisualizer { get; set; }
        
        /// <summary>
        /// 動的生成されたCanvas
        /// </summary>
        public Canvas? DynamicCanvas { get; set; }
        
        /// <summary>
        /// 入力処理管理
        /// </summary>
        public MainWindowInput? Input { get; set; }
        
        /// <summary>
        /// マウス移動可視化トラッカー
        /// </summary>
        public MouseTracker? MouseTracker { get; set; }
        
    }
}