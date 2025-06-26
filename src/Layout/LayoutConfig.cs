using System;
using System.Collections.Generic;
using System.Windows;
using YamlDotNet.Serialization;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// レイアウト設定のメインクラス
    /// </summary>
    public class LayoutConfig
    {
        /// <summary>
        /// グローバル設定（デフォルト値）
        /// </summary>
        public GlobalSettings Global { get; set; } = new();

        /// <summary>
        /// 個別要素設定（グローバル設定を上書き可能）
        /// </summary>
        public Dictionary<string, ElementConfig> Elements { get; set; } = new();
    }

    /// <summary>
    /// グローバル設定
    /// </summary>
    public class GlobalSettings
    {
        /// <summary>
        /// デフォルトキーサイズ
        /// </summary>
        public SizeConfig KeySize { get; set; } = new() { Width = 26, Height = 26 };

        /// <summary>
        /// デフォルトフォントサイズ
        /// </summary>
        public int FontSize { get; set; } = 10;

        /// <summary>
        /// フォントファミリー
        /// </summary>
        public string FontFamily { get; set; } = "Arial";

        /// <summary>
        /// 背景色
        /// </summary>
        public string BackgroundColor { get; set; } = "#2A2A2A";

        /// <summary>
        /// ハイライト色
        /// </summary>
        public string HighlightColor { get; set; } = "#00FF00";

        /// <summary>
        /// 前景色（文字色）
        /// </summary>
        public string ForegroundColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// マウス円のサイズ
        /// </summary>
        public double MouseCircleSize { get; set; } = 20;

        /// <summary>
        /// マウス円の色
        /// </summary>
        public string MouseCircleColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// マウス移動ハイライト色
        /// </summary>
        public string MouseMoveHighlightColor { get; set; } = "#FF0000";

        /// <summary>
        /// マウス移動ハイライト継続時間（秒）
        /// </summary>
        public double MouseMoveHighlightDuration { get; set; } = 0.1;

        /// <summary>
        /// マウス移動感度（ピクセル）
        /// </summary>
        public double MouseMoveThreshold { get; set; } = 5.0;

        /// <summary>
        /// ウィンドウ幅
        /// </summary>
        public double WindowWidth { get; set; } = 580;

        /// <summary>
        /// ウィンドウ高さ
        /// </summary>
        public double WindowHeight { get; set; } = 160;
    }

    /// <summary>
    /// 個別要素設定
    /// </summary>
    public class ElementConfig
    {
        /// <summary>
        /// X座標
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 表示テキスト
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// サイズ（nullの場合はグローバル設定使用）
        /// </summary>
        public SizeConfig? Size { get; set; }

        /// <summary>
        /// フォントサイズ（nullの場合はグローバル設定使用）
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        /// 表示可否
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// サイズ設定
    /// </summary>
    public class SizeConfig
    {
        /// <summary>
        /// 幅
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 高さ
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// System.Windows.Sizeに変換（YAMLシリアライゼーションから除外）
        /// </summary>
        [YamlIgnore]
        public Size Size => new(Width, Height);

        /// <summary>
        /// System.Windows.Sizeに変換
        /// </summary>
        public Size ToSize() => new(Width, Height);
    }
}