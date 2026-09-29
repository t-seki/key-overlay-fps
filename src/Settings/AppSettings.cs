using System;
using KeyOverlayFPS.Constants;

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
        
        // 色設定（初回起動の既定値。保存形式は #AARRGGBB）
        public string BackgroundColor { get; set; } = "Transparent";
        public string ForegroundColor { get; set; } = "White";
        public string HighlightColor { get; set; } = "#B400FF00";
        
        // プロファイル設定  
        public string CurrentProfile { get; set; } = "FullKeyboard65";
    }
}