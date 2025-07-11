using System;
using System.IO;
using System.Windows.Media;

namespace KeyOverlayFPS.Constants
{
    /// <summary>
    /// アプリケーション全体で使用する定数定義
    /// </summary>
    public static class ApplicationConstants
    {
        /// <summary>
        /// ファイルパス関連の定数
        /// </summary>
        public static class Paths
        {
            /// <summary>
            /// レイアウトファイル格納ディレクトリ
            /// </summary>
            public const string LayoutsDirectory = "layouts";
            
            /// <summary>
            /// 65%キーボードレイアウトファイルパス
            /// </summary>
            public static string Keyboard65Layout => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LayoutsDirectory, "65_keyboard.yaml");
            
            /// <summary>
            /// FPSキーボードレイアウトファイルパス
            /// </summary>
            public static string FpsLayout => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LayoutsDirectory, "fps_keyboard.yaml");
            
            /// <summary>
            /// 設定ファイル格納ディレクトリ
            /// </summary>
            public static string SettingsDirectory => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "KeyOverlayFPS"
            );
            
        }
        
        /// <summary>
        /// タイマー・タイミング関連の定数
        /// </summary>
        public static class Timing
        {
            /// <summary>
            /// メインタイマーの更新間隔（ミリ秒） - 約60FPS
            /// </summary>
            public const double MainTimerInterval = 16.67;
            
            /// <summary>
            /// マウス方向表示の自動非表示時間（ミリ秒）
            /// </summary>
            public const double DirectionHideDelay = 200;
            
            /// <summary>
            /// スクロール表示の持続フレーム数
            /// </summary>
            public const int ScrollDisplayFrames = 10;
        }
        
        /// <summary>
        /// 色関連の定数
        /// </summary>
        public static class Colors
        {
            /// <summary>
            /// デフォルトハイライト色（アクティブ状態）
            /// </summary>
            public static readonly Color DefaultHighlight = Color.FromArgb(180, 0, 255, 0);
            
            /// <summary>
            /// キーボードキー背景グラデーション開始色
            /// </summary>
            public static readonly Color KeyBackground1 = Color.FromRgb(0x2A, 0x2A, 0x2A);
            
            /// <summary>
            /// キーボードキー背景グラデーション終了色
            /// </summary>
            public static readonly Color KeyBackground2 = Color.FromRgb(0x1A, 0x1A, 0x1A);
            
            /// <summary>
            /// マウスボタン背景グラデーション開始色
            /// </summary>
            public static readonly Color MouseButtonBackground1 = Color.FromRgb(0x40, 0x40, 0x40);
            
            /// <summary>
            /// マウスボタン背景グラデーション終了色
            /// </summary>
            public static readonly Color MouseButtonBackground2 = Color.FromRgb(0x2A, 0x2A, 0x2A);
            
            /// <summary>
            /// 透明背景（ウィンドウ用）
            /// </summary>
            public static readonly Color TransparentBackground = Color.FromArgb(1, 0, 0, 0);
            
            /// <summary>
            /// マウス方向表示の中心点色
            /// </summary>
            public static readonly Color MouseDirectionCenter = Color.FromRgb(255, 68, 68);
            
            /// <summary>
            /// UIGenerator で使用される直接指定色
            /// </summary>
            public static readonly Color MouseBodyBorderColor = Color.FromRgb(0xCC, 0xCC, 0xCC);
            public static readonly Color MouseButtonBorderColor = Color.FromRgb(0x88, 0x88, 0x88);
            public static readonly Color MouseButtonTextColor = Color.FromRgb(0xEE, 0xEE, 0xEE);
        }
        
        
        /// <summary>
        /// 表示スケールオプション
        /// </summary>
        public static class ScaleOptions
        {
            /// <summary>
            /// 利用可能なスケール値
            /// </summary>
            public static readonly double[] Values = { 0.8, 1.0, 1.2, 1.5 };
            
            /// <summary>
            /// スケール値の表示ラベル
            /// </summary>
            public static readonly string[] Labels = { "80%", "100%", "120%", "150%" };
        }
        
        /// <summary>
        /// 数学計算関連の定数
        /// </summary>
        public static class Mathematics
        {
            /// <summary>
            /// 完全円の角度（度）
            /// </summary>
            public const double DegreesFullCircle = 360.0;
            
            /// <summary>
            /// ラジアンから度への変換係数
            /// </summary>
            public const double RadiansToDegrees = 180.0;
            
        }
        
        /// <summary>
        /// UI レイアウト関連の定数
        /// </summary>
        public static class UILayout
        {
            /// <summary>
            /// マウス本体の幅
            /// </summary>
            public const double MouseBodyWidth = 60.0;
            
            /// <summary>
            /// マウス本体の高さ
            /// </summary>
            public const double MouseBodyHeight = 100.0;
            
            /// <summary>
            /// キーの角丸半径（X方向）
            /// </summary>
            public const double KeyCornerRadiusX = 28.0;
            
            /// <summary>
            /// キーの角丸半径（Y方向）
            /// </summary>
            public const double KeyCornerRadiusY = 25.0;
            
            /// <summary>
            /// Canvas のマージン
            /// </summary>
            public const double CanvasMargin = 8.0;
            
            /// <summary>
            /// ウィンドウのデフォルト X 座標
            /// </summary>
            public const double DefaultWindowLeft = 100;
            
            /// <summary>
            /// ウィンドウのデフォルト Y 座標
            /// </summary>
            public const double DefaultWindowTop = 100;
            
            /// <summary>
            /// マウス本体のボーダー太さ
            /// </summary>
            public const double MouseBodyBorderThickness = 2.0;
            
            /// <summary>
            /// マウスボタンのボーダー太さ
            /// </summary>
            public const double MouseButtonBorderThickness = 1.0;
            
            /// <summary>
            /// マウス本体の影のぼかし半径
            /// </summary>
            public const double MouseBodyShadowBlur = 3.0;
            
            /// <summary>
            /// マウス本体の影の深さ
            /// </summary>
            public const double MouseBodyShadowDepth = 2.0;
            
            /// <summary>
            /// マウス本体の影の不透明度
            /// </summary>
            public const double MouseBodyShadowOpacity = 0.5;
            
            /// <summary>
            /// マウスボタンの影のぼかし半径
            /// </summary>
            public const double MouseButtonShadowBlur = 2.0;
            
            /// <summary>
            /// マウスボタンの影の深さ
            /// </summary>
            public const double MouseButtonShadowDepth = 1.0;
            
            /// <summary>
            /// マウスボタンの影の不透明度
            /// </summary>
            public const double MouseButtonShadowOpacity = 0.3;
            
            /// <summary>
            /// マウス左ボタンの角丸半径
            /// </summary>
            public const double MouseLeftButtonRadius = 12.0;
            
            /// <summary>
            /// マウス右ボタンの角丸半径
            /// </summary>
            public const double MouseRightButtonRadius = 12.0;
            
            /// <summary>
            /// マウス一般ボタンの角丸半径
            /// </summary>
            public const double MouseButtonRadius = 5.0;
            
            /// <summary>
            /// マウスホイールボタンの角丸半径
            /// </summary>
            public const double MouseWheelButtonRadius = 5.0;
            
            /// <summary>
            /// マウスサイドボタンの角丸半径
            /// </summary>
            public const double MouseSideButtonRadius = 2.0;
            
            /// <summary>
            /// スクロールインジケーターのフォントサイズ
            /// </summary>
            public const double ScrollIndicatorFontSize = 8.0;
            
            /// <summary>
            /// マウスボタンテキストのフォントサイズ
            /// </summary>
            public const double MouseButtonTextFontSize = 11.0;
        }
        
        /// <summary>
        /// マウス方向可視化関連の定数
        /// </summary>
        public static class MouseVisualization
        {
            /// <summary>
            /// 方向円の半径
            /// </summary>
            public const double CircleRadius = 15.0;
            
            /// <summary>
            /// 方向表示の線幅
            /// </summary>
            public const double StrokeThickness = 4.0;
            
            /// <summary>
            /// 方向分割数（16方向）
            /// </summary>
            public const int DirectionSegments = 16;
            
            /// <summary>
            /// マウス移動検出の最小ピクセル数
            /// </summary>
            public const double MovementThreshold = 5.0;
            
            /// <summary>
            /// 方向表示中心点のサイズ
            /// </summary>
            public const double CenterPointSize = 2.0;
            
            /// <summary>
            /// 方向表示キャンバスのサイズ
            /// </summary>
            public static double CanvasSize => CircleRadius * 2;
            
            /// <summary>
            /// 方向表示キャンバスのX軸オフセット
            /// </summary>
            public const double DirectionCanvasOffsetX = 15.0;
            
            /// <summary>
            /// 方向表示キャンバスのY軸オフセット
            /// </summary>
            public const double DirectionCanvasOffsetY = 50.0;
        }
    }
}