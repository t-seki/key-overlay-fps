using System;
using System.Windows.Media;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 設定管理サービスのインターフェース
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 現在の設定
        /// </summary>
        AppSettings Current { get; }

        /// <summary>
        /// 設定変更時のイベント
        /// </summary>
        event EventHandler? SettingsChanged;

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        void Load();

        /// <summary>
        /// 設定を保存する
        /// </summary>
        void Save();

        /// <summary>
        /// ウィンドウ位置を更新
        /// </summary>
        void UpdateWindowPosition(double left, double top);

        /// <summary>
        /// 最前面表示を切り替え
        /// </summary>
        void ToggleTopmost();

        /// <summary>
        /// 背景色を設定
        /// </summary>
        void SetBackgroundColor(Color color, bool transparent);

        /// <summary>
        /// 前景色を設定
        /// </summary>
        void SetForegroundColor(Color color);

        /// <summary>
        /// ハイライト色を設定
        /// </summary>
        void SetHighlightColor(Color color);

        /// <summary>
        /// 表示スケールを設定
        /// </summary>
        void SetDisplayScale(double scale);

        /// <summary>
        /// マウス可視性を切り替え
        /// </summary>
        void ToggleMouseVisibility();

        /// <summary>
        /// プロファイルを設定
        /// </summary>
        void SetCurrentProfile(string profile);

        /// <summary>
        /// 色名からBrushを取得
        /// </summary>
        Brush GetBrushFromColorName(string colorName);

        /// <summary>
        /// Brushから色名を取得
        /// </summary>
        string GetColorNameFromBrush(Brush brush);
    }
}