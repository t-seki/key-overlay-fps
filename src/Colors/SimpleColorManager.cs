using System;
using System.Collections.Concurrent;
using System.Windows.Media;
using KeyOverlayFPS.Utils;

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

        /// <summary>
        /// 背景色の設定値に一致する選択肢の位置を返す。一致しなければ -1。
        /// 透明の選択肢は設定値が "Transparent" のときに一致する。
        /// </summary>
        /// <param name="setting">設定の文字列（色名または #AARRGGBB）</param>
        /// <returns>一致した選択肢の位置。一致しない、または読めない文字列なら -1</returns>
        public static int FindBackgroundIndex(string? setting)
        {
            for (int i = 0; i < BackgroundMenuOptions.Length; i++)
            {
                var option = BackgroundMenuOptions[i];
                if (option.Transparent)
                {
                    if (string.Equals(setting?.Trim(), "Transparent", StringComparison.OrdinalIgnoreCase)) return i;
                }
                else if (TryParseColor(setting, out var color) && color == option.Color)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 前景色の設定値に一致する選択肢の位置を返す。一致しなければ -1。
        /// </summary>
        /// <param name="setting">設定の文字列（色名または #AARRGGBB）</param>
        /// <returns>一致した選択肢の位置。一致しない、または読めない文字列なら -1</returns>
        public static int FindForegroundIndex(string? setting)
        {
            if (!TryParseColor(setting, out var color)) return -1;
            for (int i = 0; i < ForegroundMenuOptions.Length; i++)
            {
                if (ForegroundMenuOptions[i].Color == color) return i;
            }
            return -1;
        }

        /// <summary>
        /// ハイライト色の設定値に一致する選択肢の位置を返す。一致しなければ -1。
        /// </summary>
        /// <param name="setting">設定の文字列（色名または #AARRGGBB）</param>
        /// <returns>一致した選択肢の位置。一致しない、または読めない文字列なら -1</returns>
        public static int FindHighlightIndex(string? setting)
        {
            if (!TryParseColor(setting, out var color)) return -1;
            for (int i = 0; i < HighlightMenuOptions.Length; i++)
            {
                if (HighlightMenuOptions[i].Color == color) return i;
            }
            return -1;
        }

        /// <summary>
        /// 読めない色文字列をログに出した記録（メニュー更新のたびに呼ばれるため、同じ文字列は一度だけ出す）
        /// </summary>
        private static readonly ConcurrentDictionary<string, byte> _loggedInvalidColors = new();

        private static bool TryParseColor(string? setting, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(setting)) return false;
            try
            {
                if (ColorConverter.ConvertFromString(setting) is Color parsed)
                {
                    color = parsed;
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (_loggedInvalidColors.TryAdd(setting, 0))
                {
                    Logger.Warning($"色の設定を読めないため、メニューのチェックを付けません: \"{setting}\"", ex);
                }
            }
            return false;
        }
    }
}