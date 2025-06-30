using System;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// アプリケーション設定クラス（簡素化版）
    /// </summary>
    public class AppSettings
    {
        // ウィンドウ設定 - 位置のみフォールバック値として定数使用
        public double WindowLeft { get; set; } = ApplicationConstants.UILayout.CanvasMargin * 12.5; // 100
        public double WindowTop { get; set; } = ApplicationConstants.UILayout.CanvasMargin * 12.5;  // 100
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
        
        // マウス設定
        public bool IsMouseTrackingEnabled { get; set; } = true;
    }
}