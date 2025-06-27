using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyOverlayFPS.Settings
{
    /// <summary>
    /// 設定検証クラス - 設定値の妥当性をチェック
    /// </summary>
    public static class SettingsValidator
    {
        /// <summary>
        /// 統一設定全体を検証
        /// </summary>
        public static ValidationResult ValidateUnifiedSettings(UnifiedSettings settings)
        {
            var result = new ValidationResult();
            
            if (settings == null)
            {
                result.AddError("設定オブジェクトがnullです");
                return result;
            }

            // 各カテゴリの検証
            ValidateWindowSettings(settings.Window, result);
            ValidateDisplaySettings(settings.Display, result);
            ValidateColorSettings(settings.Colors, result);
            ValidateProfileSettings(settings.Profile, result);
            ValidateMouseSettings(settings.Mouse, result);
            ValidateLayoutSettings(settings.Layout, result);

            return result;
        }

        /// <summary>
        /// ウィンドウ設定を検証
        /// </summary>
        public static void ValidateWindowSettings(WindowSettings window, ValidationResult result)
        {
            if (window == null)
            {
                result.AddError("ウィンドウ設定がnullです");
                return;
            }

            // 位置の検証
            if (window.Left < -1000 || window.Left > 10000)
            {
                result.AddWarning($"ウィンドウの左端位置が範囲外です: {window.Left}");
            }

            if (window.Top < -1000 || window.Top > 10000)
            {
                result.AddWarning($"ウィンドウの上端位置が範囲外です: {window.Top}");
            }

            // サイズの検証
            if (window.Width < 100 || window.Width > 5000)
            {
                result.AddError($"ウィンドウ幅が無効です: {window.Width}");
            }

            if (window.Height < 50 || window.Height > 3000)
            {
                result.AddError($"ウィンドウ高さが無効です: {window.Height}");
            }
        }

        /// <summary>
        /// 表示設定を検証
        /// </summary>
        public static void ValidateDisplaySettings(DisplaySettings display, ValidationResult result)
        {
            if (display == null)
            {
                result.AddError("表示設定がnullです");
                return;
            }

            // スケールの検証
            if (!AppConstants.DisplayScales.Contains(display.Scale))
            {
                result.AddError($"表示スケールが無効です: {display.Scale}");
            }

            // 利用可能スケールリストの検証
            if (display.AvailableScales == null || display.AvailableScales.Count == 0)
            {
                result.AddError("利用可能なスケールリストが空です");
            }
            else
            {
                foreach (var scale in display.AvailableScales)
                {
                    if (scale <= 0 || scale > 3.0)
                    {
                        result.AddWarning($"スケール値が範囲外です: {scale}");
                    }
                }
            }
        }

        /// <summary>
        /// 色設定を検証
        /// </summary>
        public static void ValidateColorSettings(ColorSettings colors, ValidationResult result)
        {
            if (colors == null)
            {
                result.AddError("色設定がnullです");
                return;
            }

            if (colors.Definitions == null)
            {
                result.AddError("色定義がnullです");
                return;
            }

            // 前景色の検証
            if (string.IsNullOrEmpty(colors.Foreground))
            {
                result.AddError("前景色が指定されていません");
            }
            else if (!colors.Definitions.ForegroundColors.ContainsKey(colors.Foreground))
            {
                result.AddError($"前景色が定義されていません: {colors.Foreground}");
            }

            // ハイライト色の検証
            if (string.IsNullOrEmpty(colors.Highlight))
            {
                result.AddError("ハイライト色が指定されていません");
            }
            else if (!colors.Definitions.HighlightColors.ContainsKey(colors.Highlight))
            {
                result.AddError($"ハイライト色が定義されていません: {colors.Highlight}");
            }

            // 背景色の検証
            if (string.IsNullOrEmpty(colors.Background))
            {
                result.AddError("背景色が指定されていません");
            }
            else if (!colors.Definitions.BackgroundColors.ContainsKey(colors.Background))
            {
                result.AddError($"背景色が定義されていません: {colors.Background}");
            }

            // 色定義の妥当性チェック
            ValidateColorDefinitions(colors.Definitions, result);
        }

        /// <summary>
        /// 色定義を検証
        /// </summary>
        private static void ValidateColorDefinitions(ColorDefinitions definitions, ValidationResult result)
        {
            // 前景色辞書の検証
            if (definitions.ForegroundColors == null || definitions.ForegroundColors.Count == 0)
            {
                result.AddError("前景色定義が空です");
            }
            else
            {
                foreach (var color in definitions.ForegroundColors)
                {
                    if (!IsValidColorValue(color.Value))
                    {
                        result.AddWarning($"前景色の値が無効です: {color.Key} = {color.Value}");
                    }
                }
            }

            // ハイライト色辞書の検証
            if (definitions.HighlightColors == null || definitions.HighlightColors.Count == 0)
            {
                result.AddError("ハイライト色定義が空です");
            }

            // 背景色辞書の検証
            if (definitions.BackgroundColors == null || definitions.BackgroundColors.Count == 0)
            {
                result.AddError("背景色定義が空です");
            }

            // メニューオプションの検証  
            ValidateColorMenuOptions(definitions.BackgroundMenuOptions, "背景色メニュー", result);
            ValidateColorMenuOptions(definitions.ForegroundMenuOptions, "前景色メニュー", result);
            ValidateColorMenuOptions(definitions.HighlightMenuOptions, "ハイライト色メニュー", result);
        }

        /// <summary>
        /// 色メニューオプションを検証
        /// </summary>
        private static void ValidateColorMenuOptions(List<ColorMenuOption> options, string menuName, ValidationResult result)
        {
            if (options == null || options.Count == 0)
            {
                result.AddWarning($"{menuName}オプションが空です");
                return;
            }

            foreach (var option in options)
            {
                if (string.IsNullOrEmpty(option.Key))
                {
                    result.AddError($"{menuName}のキーが空です");
                }

                if (string.IsNullOrEmpty(option.DisplayName))
                {
                    result.AddWarning($"{menuName}の表示名が空です: {option.Key}");
                }
            }
        }

        /// <summary>
        /// プロファイル設定を検証
        /// </summary>
        public static void ValidateProfileSettings(ProfileSettings profile, ValidationResult result)
        {
            if (profile == null)
            {
                result.AddError("プロファイル設定がnullです");
                return;
            }

            if (string.IsNullOrEmpty(profile.Current))
            {
                result.AddError("現在のプロファイルが指定されていません");
            }

            if (profile.Available == null || profile.Available.Count == 0)
            {
                result.AddError("利用可能なプロファイルリストが空です");
            }
            else if (!string.IsNullOrEmpty(profile.Current) && !profile.Available.Contains(profile.Current))
            {
                result.AddError($"現在のプロファイルが利用可能リストに含まれていません: {profile.Current}");
            }
        }

        /// <summary>
        /// マウス設定を検証
        /// </summary>
        public static void ValidateMouseSettings(MouseSettings mouse, ValidationResult result)
        {
            if (mouse == null)
            {
                result.AddError("マウス設定がnullです");
                return;
            }

            if (mouse.DirectionCount < 4 || mouse.DirectionCount > 64)
            {
                result.AddError($"マウス方向数が範囲外です: {mouse.DirectionCount}");
            }

            if (mouse.Sensitivity <= 0 || mouse.Sensitivity > 10.0)
            {
                result.AddWarning($"マウス感度が範囲外です: {mouse.Sensitivity}");
            }
        }

        /// <summary>
        /// レイアウト設定を検証
        /// </summary>
        public static void ValidateLayoutSettings(LayoutSettings layout, ValidationResult result)
        {
            if (layout == null)
            {
                result.AddError("レイアウト設定がnullです");
                return;
            }

            if (layout.CustomSettings == null)
            {
                result.AddWarning("カスタム設定辞書がnullです");
            }

            // レイアウトパスの検証（存在チェックは行わない）
            if (!string.IsNullOrEmpty(layout.CurrentLayoutPath))
            {
                if (layout.CurrentLayoutPath.Length > 260)
                {
                    result.AddWarning("レイアウトパスが長すぎます");
                }
            }
        }

        /// <summary>
        /// 色値の妥当性をチェック
        /// </summary>
        private static bool IsValidColorValue(string colorValue)
        {
            if (string.IsNullOrEmpty(colorValue))
                return false;

            // "Transparent"は特別な値として許可
            if (colorValue.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                return true;

            // 16進数カラーコードの検証 (#RRGGBB または #AARRGGBB)
            if (colorValue.StartsWith("#"))
            {
                var hex = colorValue.Substring(1);
                return (hex.Length == 6 || hex.Length == 8) && 
                       hex.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
            }

            return false;
        }
    }

    /// <summary>
    /// 検証結果クラス
    /// </summary>
    public class ValidationResult
    {
        public List<string> Errors { get; private set; } = new List<string>();
        public List<string> Warnings { get; private set; } = new List<string>();

        public bool IsValid => Errors.Count == 0;
        public bool HasWarnings => Warnings.Count > 0;

        public void AddError(string message)
        {
            Errors.Add(message);
        }

        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }

        public string GetSummary()
        {
            if (IsValid && !HasWarnings)
                return "検証成功";

            var summary = "";
            if (Errors.Count > 0)
                summary += $"エラー: {Errors.Count}件";
            
            if (Warnings.Count > 0)
            {
                if (!string.IsNullOrEmpty(summary))
                    summary += ", ";
                summary += $"警告: {Warnings.Count}件";
            }

            return summary;
        }

        public override string ToString()
        {
            var result = GetSummary() + Environment.NewLine;
            
            if (Errors.Count > 0)
            {
                result += "エラー:" + Environment.NewLine;
                foreach (var error in Errors)
                {
                    result += $"  - {error}" + Environment.NewLine;
                }
            }
            
            if (Warnings.Count > 0)
            {
                result += "警告:" + Environment.NewLine;
                foreach (var warning in Warnings)
                {
                    result += $"  - {warning}" + Environment.NewLine;
                }
            }
            
            return result;
        }
    }
}