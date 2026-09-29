using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Media;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.Utils;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// ブラシ生成の統一ファクトリクラス
    /// 重複するブラシ作成コードを集約し、一貫性を保つ
    /// </summary>
    public static class BrushFactory
    {
        /// <summary>
        /// 変換に失敗したことをログに出した色文字列（毎フレーム呼ばれるため、同じ文字列は一度だけログに出す）
        /// </summary>
        private static readonly ConcurrentDictionary<string, byte> _loggedInvalidColors = new();

        /// <summary>
        /// 標準背景グラデーションブラシを作成（キーボードキー・マウス本体共用）
        /// </summary>
        /// <returns>標準背景用LinearGradientBrush</returns>
        public static LinearGradientBrush CreateStandardBackground()
        {
            return new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(ApplicationConstants.Colors.KeyBackground1, 0),
                    new GradientStop(ApplicationConstants.Colors.KeyBackground2, 1)
                },
                new Point(0, 0),
                new Point(1, 1)
            );
        }
        
        /// <summary>
        /// キーボードキー用の背景グラデーションブラシを作成
        /// </summary>
        /// <returns>キーボードキー用LinearGradientBrush</returns>
        public static LinearGradientBrush CreateKeyboardKeyBackground()
        {
            return CreateStandardBackground();
        }
        
        /// <summary>
        /// マウス本体用の背景グラデーションブラシを作成
        /// </summary>
        /// <returns>マウス本体用LinearGradientBrush</returns>
        public static LinearGradientBrush CreateMouseBodyBackground()
        {
            return CreateStandardBackground();
        }
        
        /// <summary>
        /// マウスボタン用の背景グラデーションブラシを作成
        /// </summary>
        /// <returns>マウスボタン用LinearGradientBrush</returns>
        public static LinearGradientBrush CreateMouseButtonBackground()
        {
            return new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(ApplicationConstants.Colors.MouseButtonBackground1, 0),
                    new GradientStop(ApplicationConstants.Colors.MouseButtonBackground2, 1)
                },
                new Point(0, 0),
                new Point(0, 1)
            );
        }
        
        /// <summary>
        /// デフォルトハイライト色ブラシを作成
        /// </summary>
        /// <returns>ハイライト用SolidColorBrush</returns>
        public static SolidColorBrush CreateDefaultHighlightBrush()
        {
            return new SolidColorBrush(ApplicationConstants.Colors.DefaultHighlight);
        }
        
        /// <summary>
        /// 透明背景ブラシを作成（ウィンドウ用）
        /// </summary>
        /// <returns>透明背景用SolidColorBrush</returns>
        public static SolidColorBrush CreateTransparentBackground()
        {
            return new SolidColorBrush(ApplicationConstants.Colors.TransparentBackground);
        }
        
        /// <summary>
        /// マウス方向表示中心点ブラシを作成
        /// </summary>
        /// <returns>中心点用SolidColorBrush</returns>
        public static SolidColorBrush CreateMouseDirectionCenterBrush()
        {
            return new SolidColorBrush(ApplicationConstants.Colors.MouseDirectionCenter);
        }
        
        
        
        /// <summary>
        /// 色文字列からブラシを作成（エラーハンドリング付き）
        /// </summary>
        /// <param name="colorString">色を表す文字列</param>
        /// <param name="fallbackBrush">変換失敗時のフォールバック</param>
        /// <returns>SolidColorBrush</returns>
        /// <remarks>
        /// 変換に失敗したときは警告をログに出してフォールバックを返す。同じ色文字列の警告は一度だけ出す
        /// </remarks>
        public static Brush CreateBrushFromString(string colorString, Brush? fallbackBrush = null)
        {
            try
            {
                if (colorString.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                {
                    return CreateTransparentBackground();
                }
                
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                return new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                if (_loggedInvalidColors.TryAdd(colorString ?? string.Empty, 0))
                {
                    Logger.Warning($"色文字列をブラシに変換できないため、フォールバックの色を使います: \"{colorString}\"", ex);
                }
                return fallbackBrush ?? Brushes.White;
            }
        }
    }
}