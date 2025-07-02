using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// キーボード要素の生成を担当するクラス
    /// </summary>
    public static class KeyboardElementGenerator
    {
        /// <summary>
        /// キーボードキー要素を生成
        /// </summary>
        public static void GenerateKeyElements(Canvas canvas, LayoutConfig layout)
        {
            if (layout.Keys == null) return;

            foreach (var (keyName, keyDef) in layout.Keys)
            {
                if (!keyDef.IsVisible) continue;

                var keyBorder = CreateKeyBorder(keyName, keyDef, layout.Global);
                var textBlock = CreateKeyTextBlock(keyName, keyDef, layout.Global);

                keyBorder.Child = textBlock;
                Canvas.SetLeft(keyBorder, keyDef.Position.X);
                Canvas.SetTop(keyBorder, keyDef.Position.Y);

                canvas.Children.Add(keyBorder);
            }
        }

        /// <summary>
        /// キーボードキーのBorder要素を作成
        /// </summary>
        private static Border CreateKeyBorder(string keyName, KeyDefinition keyDef, GlobalSettings global)
        {
            var border = new Border
            {
                Name = keyName,
                Width = keyDef.Size.Width,
                Height = keyDef.Size.Height,
                BorderBrush = BrushFactory.CreateBrushFromString(global.ForegroundColor, Brushes.White),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Background = BrushFactory.CreateKeyboardKeyBackground()
            };

            return border;
        }

        /// <summary>
        /// キーボードキーのTextBlock要素を作成
        /// </summary>
        private static TextBlock CreateKeyTextBlock(string keyName, KeyDefinition keyDef, GlobalSettings global)
        {
            var fontSize = keyDef.FontSize ?? global.FontSize;
            
            var textBlock = new TextBlock
            {
                Name = keyName + "Text",
                Text = keyDef.Text,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = BrushFactory.CreateBrushFromString(global.ForegroundColor, Brushes.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(global.FontFamily))
            {
                textBlock.FontFamily = new FontFamily(global.FontFamily);
            }

            return textBlock;
        }
    }
}