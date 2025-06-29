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
            /// 方向分割数（32方向）
            /// </summary>
            public const int DirectionSegments = 32;
            
            /// <summary>
            /// マウス移動検出の最小ピクセル数
            /// </summary>
            public const double MovementThreshold = 5.0;
            
            /// <summary>
            /// 方向表示キャンバスのサイズ
            /// </summary>
            public static double CanvasSize => CircleRadius * 2;
        }
    }
}