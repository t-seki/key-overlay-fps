using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 統一設定クラス - 全ての設定項目を階層化して管理
    /// </summary>
    public class UnifiedSettings
    {
        public WindowSettings Window { get; set; } = new WindowSettings();
        public DisplaySettings Display { get; set; } = new DisplaySettings();
        public ColorSettings Colors { get; set; } = new ColorSettings();
        public ProfileSettings Profile { get; set; } = new ProfileSettings();
        public MouseSettings Mouse { get; set; } = new MouseSettings();
        public LayoutSettings Layout { get; set; } = new LayoutSettings();
    }

    /// <summary>
    /// ウィンドウ設定
    /// </summary>
    public class WindowSettings
    {
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public bool IsTopmost { get; set; } = true;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 600;
    }

    /// <summary>
    /// 表示設定
    /// </summary>
    public class DisplaySettings
    {
        public double Scale { get; set; } = 1.0;
        public bool IsMouseVisible { get; set; } = true;
        public List<double> AvailableScales { get; set; } = new List<double> { 0.8, 1.0, 1.2, 1.5 };
    }

    /// <summary>
    /// 色設定
    /// </summary>
    public class ColorSettings
    {
        public string Background { get; set; } = "Transparent";
        public string Foreground { get; set; } = "White";
        public string Highlight { get; set; } = "Green";
        
        public ColorDefinitions Definitions { get; set; } = new ColorDefinitions();
    }

    /// <summary>
    /// 色定義 - 利用可能な色のマスターデータ
    /// </summary>
    public class ColorDefinitions
    {
        public Dictionary<string, string> ForegroundColors { get; set; } = new Dictionary<string, string>
        {
            { "White", "#FFFFFF" },
            { "Black", "#000000" },
            { "Gray", "#808080" },
            { "Blue", "#0000FF" },
            { "Green", "#008000" },
            { "Red", "#FF0000" },
            { "Yellow", "#FFFF00" }
        };

        public Dictionary<string, string> HighlightColors { get; set; } = new Dictionary<string, string>
        {
            { "White", "#B4FFFFFF" },
            { "Black", "#B4000000" },
            { "Gray", "#B4808080" },
            { "Blue", "#B40000FF" },
            { "Green", "#B4008000" },
            { "Red", "#B4FF0000" },
            { "Yellow", "#B4FFFF00" }
        };

        public Dictionary<string, string> BackgroundColors { get; set; } = new Dictionary<string, string>
        {
            { "Transparent", "Transparent" },
            { "Lime", "#00FF00" },
            { "Blue", "#0000FF" },
            { "Black", "#000000" }
        };

        public List<ColorMenuOption> BackgroundMenuOptions { get; set; } = new List<ColorMenuOption>
        {
            new ColorMenuOption { Key = "Transparent", DisplayName = "透明" },
            new ColorMenuOption { Key = "Lime", DisplayName = "ライム" },
            new ColorMenuOption { Key = "Blue", DisplayName = "青" },
            new ColorMenuOption { Key = "Black", DisplayName = "黒" }
        };

        public List<ColorMenuOption> ForegroundMenuOptions { get; set; } = new List<ColorMenuOption>
        {
            new ColorMenuOption { Key = "White", DisplayName = "白" },
            new ColorMenuOption { Key = "Black", DisplayName = "黒" },
            new ColorMenuOption { Key = "Gray", DisplayName = "グレー" },
            new ColorMenuOption { Key = "Blue", DisplayName = "青" },
            new ColorMenuOption { Key = "Green", DisplayName = "緑" },
            new ColorMenuOption { Key = "Red", DisplayName = "赤" },
            new ColorMenuOption { Key = "Yellow", DisplayName = "黄" }
        };

        public List<ColorMenuOption> HighlightMenuOptions { get; set; } = new List<ColorMenuOption>
        {
            new ColorMenuOption { Key = "White", DisplayName = "白" },
            new ColorMenuOption { Key = "Black", DisplayName = "黒" },
            new ColorMenuOption { Key = "Gray", DisplayName = "グレー" },
            new ColorMenuOption { Key = "Blue", DisplayName = "青" },
            new ColorMenuOption { Key = "Green", DisplayName = "緑" },
            new ColorMenuOption { Key = "Red", DisplayName = "赤" },
            new ColorMenuOption { Key = "Yellow", DisplayName = "黄" }
        };
    }

    /// <summary>
    /// 色メニューオプション
    /// </summary>
    public class ColorMenuOption
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// プロファイル設定
    /// </summary>
    public class ProfileSettings
    {
        public string Current { get; set; } = "FullKeyboard65";
        public List<string> Available { get; set; } = new List<string>
        {
            "FullKeyboard65",
            "FPS"
        };
    }

    /// <summary>
    /// マウス設定
    /// </summary>
    public class MouseSettings
    {
        public bool IsVisible { get; set; } = true;
        public bool IsTrackingEnabled { get; set; } = true;
        public int DirectionCount { get; set; } = 32;
        public double Sensitivity { get; set; } = 1.0;
    }

    /// <summary>
    /// レイアウト設定
    /// </summary>
    public class LayoutSettings
    {
        public string CurrentLayoutPath { get; set; } = "";
        public bool IsEditorVisible { get; set; } = false;
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// アプリケーション定数
    /// </summary>
    public static class AppConstants
    {
        public const int TimerInterval = 50;
        public const int ScrollDisplayFrames = 20;
        
        public static class WindowSizes
        {
            public const double FullKeyboardWidth = 800;
            public const double FullKeyboardHeight = 300;
            public const double FpsWidth = 400;
            public const double FpsHeight = 200;
        }
        
        public static readonly List<double> DisplayScales = new List<double> { 0.8, 1.0, 1.2, 1.5 };
    }
}