using System.Windows.Media;

namespace KeyOverlayFPS.Colors
{
    /// <summary>
    /// 簡素化された色管理クラス
    /// </summary>
    public static class SimpleColorManager
    {
        /// <summary>
        /// 背景色オプション（名前、色、透明フラグ）
        /// </summary>
        public static (string Name, Color Color, bool Transparent)[] BackgroundMenuOptions = new[]
        {
            ("透明", System.Windows.Media.Colors.Transparent, true),
            ("ライム", Color.FromRgb(0, 255, 0), false),
            ("青", System.Windows.Media.Colors.Blue, false),
            ("黒", System.Windows.Media.Colors.Black, false)
        };

        /// <summary>
        /// 前景色オプション（名前、色）
        /// </summary>
        public static (string Name, Color Color)[] ForegroundMenuOptions = new[]
        {
            ("白", System.Windows.Media.Colors.White),
            ("黒", System.Windows.Media.Colors.Black),
            ("グレー", System.Windows.Media.Colors.Gray),
            ("青", System.Windows.Media.Colors.Blue),
            ("緑", System.Windows.Media.Colors.Green),
            ("赤", System.Windows.Media.Colors.Red),
            ("黄", System.Windows.Media.Colors.Yellow)
        };

        /// <summary>
        /// ハイライト色オプション（名前、色）
        /// </summary>
        public static (string Name, Color Color)[] HighlightMenuOptions = new[]
        {
            ("緑", Color.FromArgb(180, 0, 255, 0)),
            ("白", Color.FromArgb(180, 255, 255, 255)),
            ("黒", Color.FromArgb(180, 0, 0, 0)),
            ("グレー", Color.FromArgb(180, 128, 128, 128)),
            ("青", Color.FromArgb(180, 0, 0, 255)),
            ("赤", Color.FromArgb(180, 255, 0, 0)),
            ("黄", Color.FromArgb(180, 255, 255, 0))
        };
    }
}