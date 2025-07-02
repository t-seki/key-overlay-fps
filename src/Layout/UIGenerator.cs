using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// YAML設定からCanvasUIを動的生成するクラス
    /// </summary>
    public static class UIGenerator
    {
        /// <summary>
        /// レイアウト設定からCanvasを生成
        /// </summary>
        /// <param name="layout">レイアウト設定</param>
        /// <param name="parentWindow">親ウィンドウ</param>
        /// <returns>生成されたCanvas</returns>
        public static Canvas GenerateCanvas(LayoutConfig layout, Window parentWindow)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            var canvas = new Canvas
            {
                Margin = new Thickness(ApplicationConstants.UILayout.CanvasMargin),
                Background = Brushes.Transparent
            };

            // ウィンドウサイズを設定
            if (layout.Window != null)
            {
                parentWindow.Width = layout.Window.Width;
                parentWindow.Height = layout.Window.Height;
                
                // 背景色を設定
                if (!string.IsNullOrEmpty(layout.Window.BackgroundColor))
                {
                    SetCanvasBackground(canvas, layout.Window.BackgroundColor);
                }
            }

            // キー要素を生成
            KeyboardElementGenerator.GenerateKeyElements(canvas, layout);

            // マウス要素を生成
            MouseElementGenerator.GenerateMouseElements(canvas, layout);

            return canvas;
        }

        /// <summary>
        /// 動的UI要素の名前登録を解除
        /// </summary>
        /// <param name="canvas">対象のCanvas</param>
        /// <param name="parentWindow">親ウィンドウ</param>
        public static void UnregisterElementNames(Canvas canvas, Window parentWindow)
        {
            ElementNameManager.UnregisterElementNames(canvas, parentWindow);
        }

        /// <summary>
        /// 動的UI要素に名前を登録（後のイベントバインド用）
        /// </summary>
        public static void RegisterElementNames(Canvas canvas, Window parentWindow)
        {
            ElementNameManager.RegisterElementNames(canvas, parentWindow);
        }

        /// <summary>
        /// Canvas背景色を設定
        /// </summary>
        private static void SetCanvasBackground(Canvas canvas, string colorString)
        {
            try
            {
                if (colorString.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                {
                    canvas.Background = BrushFactory.CreateTransparentBackground();
                }
                else
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    canvas.Background = new SolidColorBrush(color);
                }
            }
            catch
            {
                // 色変換エラー時は透明背景を使用
                canvas.Background = BrushFactory.CreateTransparentBackground();
            }
        }
    }
}