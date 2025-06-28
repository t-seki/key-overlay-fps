using System.Collections.Generic;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.Input;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Layout;
using System.Windows.Controls;

namespace KeyOverlayFPS.Initialization
{
    /// <summary>
    /// 初期化プロセス中に共有されるコンテキスト
    /// </summary>
    public class InitializationContext
    {
        /// <summary>
        /// 初期化済みコンポーネントを格納する辞書
        /// </summary>
        public Dictionary<string, object> Components { get; } = new();
        
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
        /// 現在のレイアウト設定
        /// </summary>
        public LayoutConfig? CurrentLayout { get; set; }
        
        /// <summary>
        /// イベントバインダー
        /// </summary>
        public KeyEventBinder? EventBinder { get; set; }
        
        /// <summary>
        /// 動的生成されたCanvas
        /// </summary>
        public Canvas? DynamicCanvas { get; set; }
        
        /// <summary>
        /// 入力処理管理
        /// </summary>
        public MainWindowInput? Input { get; set; }
        
        /// <summary>
        /// コンポーネントを取得
        /// </summary>
        public T? GetComponent<T>(string key) where T : class
        {
            return Components.TryGetValue(key, out var component) ? component as T : null;
        }
        
        /// <summary>
        /// コンポーネントを設定
        /// </summary>
        public void SetComponent<T>(string key, T component) where T : class
        {
            Components[key] = component;
        }
    }
}