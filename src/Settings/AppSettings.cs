using System;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Layout;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// アプリケーション設定クラス（簡素化版）
    /// </summary>
    public class AppSettings
    {
        // ウィンドウ設定
        public double WindowLeft { get; set; } = ApplicationConstants.UILayout.DefaultWindowLeft;
        public double WindowTop { get; set; } = ApplicationConstants.UILayout.DefaultWindowTop;
        public bool IsTopmost { get; set; } = true;
        
        // 表示設定
        public double DisplayScale { get; set; } = 1.0;
        public bool IsMouseVisible { get; set; } = true;
        
        // 色設定（YAML から読み込み、フォールバック値のみ）
        public string BackgroundColor { get; set; } = "Transparent";
        public string ForegroundColor { get; set; } = "White";
        public string HighlightColor { get; set; } = "Green";
        
        // プロファイル設定  
        public string CurrentProfile { get; set; } = "FullKeyboard65";

        /// <summary>
        /// LayoutConfigからAppSettingsを作成する
        /// YAMLレイアウトファイルから恒常的な設定値のみを取得し、
        /// 実行時の動的設定（ウィンドウ位置、表示制御など）はデフォルト値を使用する
        /// </summary>
        /// <param name="layout">レイアウト設定</param>
        /// <returns>新しいAppSettingsインスタンス</returns>
        public static AppSettings CreateFromLayout(LayoutConfig layout)
        {
            return new AppSettings
            {
                // YAMLから取得する恒常的な設定
                BackgroundColor = layout?.Global?.BackgroundColor ?? "Transparent",
                ForegroundColor = layout?.Global?.ForegroundColor ?? "White",
                HighlightColor = layout?.Global?.HighlightColor ?? "Green",
                CurrentProfile = layout?.Profile?.Name ?? "FullKeyboard65",
                
                // 実行時の動的設定はAppSettingsのデフォルト値を使用
                // - ウィンドウ位置（WindowLeft、WindowTop）
                // - 最前面表示（IsTopmost）
                // - 表示スケール（DisplayScale）
                // - マウス可視性（IsMouseVisible）
            };
        }
    }
}