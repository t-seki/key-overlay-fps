using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KeyOverlayFPS.Settings;

namespace KeyOverlayFPS.Colors
{
    /// <summary>
    /// 色管理クラス - UnifiedSettingsと統合された色管理システム
    /// </summary>
    public static class ColorManager
    {
        /// <summary>
        /// 前景色ブラシを取得（UnifiedSettingsから取得）
        /// </summary>
        public static Brush GetForegroundBrush(string colorName)
        {
            var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
            if (colorDefinitions.ForegroundColors.TryGetValue(colorName, out var colorValue))
            {
                return ParseColorValue(colorValue);
            }
            return Brushes.White; // デフォルト
        }
        
        /// <summary>
        /// ハイライト色ブラシを取得（UnifiedSettingsから取得）
        /// </summary>
        public static Brush GetHighlightBrush(string colorName)
        {
            var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
            if (colorDefinitions.HighlightColors.TryGetValue(colorName, out var colorValue))
            {
                return ParseColorValue(colorValue);
            }
            return new SolidColorBrush(Color.FromArgb(180, 0, 255, 0)); // デフォルト緑
        }
        
        /// <summary>
        /// 背景色を取得（UnifiedSettingsから取得）
        /// </summary>
        public static Color GetBackgroundColor(string colorName)
        {
            var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
            if (colorDefinitions.BackgroundColors.TryGetValue(colorName, out var colorValue))
            {
                if (colorValue.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                    return System.Windows.Media.Colors.Transparent;
                
                var brush = ParseColorValue(colorValue);
                if (brush is SolidColorBrush solidBrush)
                    return solidBrush.Color;
            }
            return System.Windows.Media.Colors.Transparent; // デフォルト
        }
        
        /// <summary>
        /// ブラシから色名を逆引き
        /// </summary>
        public static string GetColorName(Brush brush)
        {
            if (brush is not SolidColorBrush solidBrush) return "White";
            
            var color = solidBrush.Color;
            var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
            
            // 前景色チェック
            foreach (var kvp in colorDefinitions.ForegroundColors)
            {
                var testBrush = ParseColorValue(kvp.Value);
                if (testBrush is SolidColorBrush testSolid && ColorsEqual(color, testSolid.Color))
                    return kvp.Key;
            }
            
            // ハイライト色チェック
            foreach (var kvp in colorDefinitions.HighlightColors)
            {
                var testBrush = ParseColorValue(kvp.Value);
                if (testBrush is SolidColorBrush testSolid && ColorsEqual(color, testSolid.Color))
                    return kvp.Key;
            }
            
            return "White";
        }
        
        /// <summary>
        /// 色から背景色名を逆引き
        /// </summary>
        public static string GetBackgroundColorName(Color color)
        {
            var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
            
            foreach (var kvp in colorDefinitions.BackgroundColors)
            {
                if (kvp.Value.Equals("Transparent", StringComparison.OrdinalIgnoreCase) && color == System.Windows.Media.Colors.Transparent)
                    return kvp.Key;
                
                var testBrush = ParseColorValue(kvp.Value);
                if (testBrush is SolidColorBrush testSolid && ColorsEqual(color, testSolid.Color))
                    return kvp.Key;
            }
            
            return "Transparent";
        }
        
        /// <summary>
        /// メニュー用背景色オプションを取得
        /// </summary>
        public static (string Name, Color Color, bool Transparent)[] GetBackgroundMenuOptions()
        {
            var options = UnifiedSettingsManager.Instance.Settings.Colors.Definitions.BackgroundMenuOptions;
            var result = new List<(string, Color, bool)>();
            
            foreach (var option in options)
            {
                var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
                if (colorDefinitions.BackgroundColors.TryGetValue(option.Key, out var colorValue))
                {
                    if (colorValue.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add((option.DisplayName, System.Windows.Media.Colors.Transparent, true));
                    }
                    else
                    {
                        var brush = ParseColorValue(colorValue);
                        if (brush is SolidColorBrush solidBrush)
                        {
                            result.Add((option.DisplayName, solidBrush.Color, false));
                        }
                    }
                }
            }
            
            return result.ToArray();
        }
        
        /// <summary>
        /// メニュー用前景色オプションを取得
        /// </summary>
        public static (string Name, Color Color)[] GetForegroundMenuOptions()
        {
            var options = UnifiedSettingsManager.Instance.Settings.Colors.Definitions.ForegroundMenuOptions;
            var result = new List<(string, Color)>();
            
            foreach (var option in options)
            {
                var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
                if (colorDefinitions.ForegroundColors.TryGetValue(option.Key, out var colorValue))
                {
                    var brush = ParseColorValue(colorValue);
                    if (brush is SolidColorBrush solidBrush)
                    {
                        result.Add((option.DisplayName, solidBrush.Color));
                    }
                }
            }
            
            return result.ToArray();
        }
        
        /// <summary>
        /// メニュー用ハイライト色オプションを取得
        /// </summary>
        public static (string Name, Color Color)[] GetHighlightMenuOptions()
        {
            var options = UnifiedSettingsManager.Instance.Settings.Colors.Definitions.HighlightMenuOptions;
            var result = new List<(string, Color)>();
            
            foreach (var option in options)
            {
                var colorDefinitions = UnifiedSettingsManager.Instance.Settings.Colors.Definitions;
                if (colorDefinitions.HighlightColors.TryGetValue(option.Key, out var colorValue))
                {
                    var brush = ParseColorValue(colorValue);
                    if (brush is SolidColorBrush solidBrush)
                    {
                        result.Add((option.DisplayName, solidBrush.Color));
                    }
                }
            }
            
            return result.ToArray();
        }
        
        /// <summary>
        /// 色値文字列（#RRGGBB）をBrushに変換
        /// </summary>
        private static Brush ParseColorValue(string colorValue)
        {
            try
            {
                if (colorValue.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                    return Brushes.Transparent;
                
                var color = (Color)ColorConverter.ConvertFromString(colorValue);
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.White; // パース失敗時のデフォルト
            }
        }
        
        /// <summary>
        /// 色の比較（アルファ値も含む）
        /// </summary>
        private static bool ColorsEqual(Color color1, Color color2)
        {
            return color1.A == color2.A && color1.R == color2.R && 
                   color1.G == color2.G && color1.B == color2.B;
        }
        
        // 後方互換性のための静的プロパティ（移行期間用）
        public static (string Name, Color Color, bool Transparent)[] BackgroundMenuOptions => GetBackgroundMenuOptions();
        public static (string Name, Color Color)[] ForegroundMenuOptions => GetForegroundMenuOptions();
        public static (string Name, Color Color)[] HighlightMenuOptions => GetHighlightMenuOptions();
    }
}