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
        /// プロファイル情報
        /// </summary>
        public ProfileInfo Profile { get; set; } = new();

        /// <summary>
        /// グローバル設定（デフォルト値）
        /// </summary>
        public GlobalSettings Global { get; set; } = new();

        /// <summary>
        /// キー定義設定
        /// </summary>
        public Dictionary<string, KeyDefinition> Keys { get; set; } = new();

        /// <summary>
        /// マウス設定
        /// </summary>
        public MouseSettings Mouse { get; set; } = new();

        /// <summary>
        /// ウィンドウ設定
        /// </summary>
        public WindowSettings Window { get; set; } = new();

    }

    /// <summary>
    /// プロファイル情報
    /// </summary>
    public class ProfileInfo
    {
        /// <summary>
        /// プロファイル名
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// プロファイル説明
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// バージョン
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 作成者
        /// </summary>
        public string Author { get; set; } = "";

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// ウィンドウ設定
    /// </summary>
    public class WindowSettings
    {
        /// <summary>
        /// ウィンドウ幅（マウス表示時）
        /// </summary>
        public double Width { get; set; } = 580;

        /// <summary>
        /// ウィンドウ高さ
        /// </summary>
        public double Height { get; set; } = 160;

        /// <summary>
        /// ウィンドウ幅（マウス非表示時）
        /// </summary>
        public double WidthWithoutMouse { get; set; } = 510;

        /// <summary>
        /// 背景色
        /// </summary>
        public string BackgroundColor { get; set; } = "Transparent";
    }

    /// <summary>
    /// マウス設定
    /// </summary>
    public class MouseSettings
    {

        /// <summary>
        /// マウス位置
        /// </summary>
        public PositionConfig Position { get; set; } = new() { X = 475, Y = 20 };

        /// <summary>
        /// ボタン配置設定
        /// </summary>
        public Dictionary<string, ButtonConfig> Buttons { get; set; } = new();

        /// <summary>
        /// マウス本体設定
        /// </summary>
        public MouseElementConfig Body { get; set; } = new()
        {
            Size = new SizeConfig { Width = DefaultBodyWidth, Height = DefaultBodyHeight }
        };

        /// <summary>
        /// マウス本体の既定の幅（YAML に body が無いときに使う）
        /// </summary>
        public const double DefaultBodyWidth = 60;

        /// <summary>
        /// マウス本体の既定の高さ（YAML に body が無いときに使う）
        /// </summary>
        public const double DefaultBodyHeight = 100;

        /// <summary>
        /// 仮想キーコードを持つボタンの名前とキーコードを列挙する
        /// virtualKey を持たないボタン（ScrollUp / ScrollDown など）は含めない
        /// </summary>
        /// <returns>ボタン名と仮想キーコードの組</returns>
        public IEnumerable<(string Name, int VirtualKey)> GetButtonVirtualKeys()
        {
            if (Buttons == null) yield break;

            foreach (var (name, button) in Buttons)
            {
                if (button != null && button.VirtualKey > 0)
                {
                    yield return (name, button.VirtualKey);
                }
            }
        }

        /// <summary>
        /// 方向表示キャンバス設定
        /// </summary>
        public MouseDirectionCanvasConfig DirectionCanvas { get; set; } = new();
    }

    /// <summary>
    /// マウス要素設定
    /// </summary>
    public class MouseElementConfig
    {
        /// <summary>
        /// 相対位置（マウス本体からのオフセット）
        /// </summary>
        public PositionConfig Offset { get; set; } = new();

        /// <summary>
        /// サイズ
        /// </summary>
        public SizeConfig Size { get; set; } = new();

        /// <summary>
        /// 表示可否
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// マウス方向表示キャンバス設定
    /// サイズと表示内容は定数（ApplicationConstants.MouseVisualization）で決まり、YAML では位置だけを指定する
    /// </summary>
    public class MouseDirectionCanvasConfig
    {
        /// <summary>
        /// 相対位置（マウス位置からのオフセット）
        /// </summary>
        public PositionConfig Offset { get; set; } = new();
    }

    /// <summary>
    /// ボタン設定
    /// </summary>
    public class ButtonConfig
    {
        /// <summary>
        /// 相対位置（マウス本体からのオフセット）
        /// </summary>
        public PositionConfig Offset { get; set; } = new();

        /// <summary>
        /// サイズ
        /// </summary>
        public SizeConfig Size { get; set; } = new();

        /// <summary>
        /// 仮想キーコード
        /// </summary>
        public int VirtualKey { get; set; }

        /// <summary>
        /// 表示可否
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// キー定義
    /// </summary>
    public class KeyDefinition
    {
        /// <summary>
        /// 位置
        /// </summary>
        public PositionConfig Position { get; set; } = new();

        /// <summary>
        /// サイズ
        /// </summary>
        public SizeConfig Size { get; set; } = new();

        /// <summary>
        /// 表示テキスト
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Shift時テキスト
        /// </summary>
        public string? ShiftText { get; set; }

        /// <summary>
        /// 仮想キーコード
        /// </summary>
        public int VirtualKey { get; set; }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        /// 表示可否
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// 位置設定
    /// </summary>
    public class PositionConfig
    {
        /// <summary>
        /// X座標
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y { get; set; }
    }

    /// <summary>
    /// グローバル設定
    /// </summary>
    public class GlobalSettings
    {
        /// <summary>
        /// デフォルトフォントサイズ
        /// </summary>
        public int FontSize { get; set; } = 10;

        /// <summary>
        /// フォントファミリー
        /// </summary>
        public string FontFamily { get; set; } = "Arial";

        /// <summary>
        /// 前景色（キーの枠線と文字の初期色。文字色は表示設定の前景色で上書きされる）
        /// </summary>
        public string ForegroundColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// Shift表示変更機能の有効/無効
        /// </summary>
        public bool ShiftDisplayEnabled { get; set; } = true;
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