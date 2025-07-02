using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// UI要素作成の統一ファクトリクラス
    /// Border、TextBlock等の重複する作成コードを集約
    /// </summary>
    public static class UIElementFactory
    {
        #region Border作成メソッド

        /// <summary>
        /// キーボードキー用のBorderを作成
        /// </summary>
        public static Border CreateKeyboardKeyBorder(string name, double width, double height, string foregroundColor)
        {
            return new Border
            {
                Name = name,
                Width = width,
                Height = height,
                BorderBrush = BrushFactory.CreateBrushFromString(foregroundColor, Brushes.White),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Background = BrushFactory.CreateKeyboardKeyBackground()
            };
        }

        /// <summary>
        /// マウスボタン用のBorderを作成
        /// </summary>
        public static Border CreateMouseButtonBorder(string buttonName, double width, double height)
        {
            var border = new Border
            {
                Name = buttonName,
                Width = width,
                Height = height,
                BorderBrush = new SolidColorBrush(ApplicationConstants.Colors.MouseButtonBorderColor),
                BorderThickness = new Thickness(ApplicationConstants.UILayout.MouseButtonBorderThickness),
                Background = BrushFactory.CreateMouseButtonBackground(),
                Effect = CreateMouseButtonShadowEffect()
            };

            // ボタン種類に応じてCornerRadiusを設定
            border.CornerRadius = GetCornerRadiusForButton(buttonName);

            return border;
        }

        /// <summary>
        /// マウス本体用のBorderを作成
        /// </summary>
        public static Border CreateMouseBodyBorder()
        {
            return new Border
            {
                Name = "MouseBody",
                Width = ApplicationConstants.UILayout.MouseBodyWidth,
                Height = ApplicationConstants.UILayout.MouseBodyHeight,
                BorderBrush = new SolidColorBrush(ApplicationConstants.Colors.MouseBodyBorderColor),
                BorderThickness = new Thickness(ApplicationConstants.UILayout.MouseBodyBorderThickness),
                CornerRadius = new CornerRadius(
                    ApplicationConstants.UILayout.KeyCornerRadiusX,
                    ApplicationConstants.UILayout.KeyCornerRadiusX,
                    ApplicationConstants.UILayout.KeyCornerRadiusY,
                    ApplicationConstants.UILayout.KeyCornerRadiusY),
                Background = BrushFactory.CreateMouseBodyBackground(),
                Effect = CreateMouseBodyShadowEffect()
            };
        }

        #endregion

        #region TextBlock作成メソッド

        /// <summary>
        /// キーボードキー用のTextBlockを作成
        /// </summary>
        public static TextBlock CreateKeyboardKeyTextBlock(string name, string text, double fontSize, string foregroundColor, string? fontFamily = null)
        {
            var textBlock = new TextBlock
            {
                Name = name,
                Text = text,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = BrushFactory.CreateBrushFromString(foregroundColor, Brushes.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(fontFamily))
            {
                textBlock.FontFamily = new FontFamily(fontFamily);
            }

            return textBlock;
        }

        /// <summary>
        /// マウスボタン用の空TextBlockを作成
        /// </summary>
        public static TextBlock CreateMouseButtonTextBlock()
        {
            return new TextBlock
            {
                Text = "",
                FontSize = ApplicationConstants.UILayout.MouseButtonTextFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(
                    ApplicationConstants.Colors.MouseButtonTextColor.R,
                    ApplicationConstants.Colors.MouseButtonTextColor.G,
                    ApplicationConstants.Colors.MouseButtonTextColor.B)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        /// <summary>
        /// スクロールインジケーター用のTextBlockを作成
        /// </summary>
        public static TextBlock CreateScrollIndicatorTextBlock(string name, string text)
        {
            return new TextBlock
            {
                Name = name,
                Text = text,
                FontSize = ApplicationConstants.UILayout.ScrollIndicatorFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Transparent,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        #endregion

        #region ヘルパーメソッド

        /// <summary>
        /// ボタン種類に応じたCornerRadiusを取得
        /// </summary>
        private static CornerRadius GetCornerRadiusForButton(string buttonName)
        {
            return buttonName switch
            {
                "MouseLeft" => new CornerRadius(
                    ApplicationConstants.UILayout.MouseLeftButtonRadius,
                    ApplicationConstants.UILayout.MouseButtonRadius,
                    ApplicationConstants.UILayout.MouseButtonRadius,
                    ApplicationConstants.UILayout.MouseButtonRadius),
                "MouseRight" => new CornerRadius(
                    ApplicationConstants.UILayout.MouseButtonRadius,
                    ApplicationConstants.UILayout.MouseRightButtonRadius,
                    ApplicationConstants.UILayout.MouseButtonRadius,
                    ApplicationConstants.UILayout.MouseButtonRadius),
                "MouseWheelButton" => new CornerRadius(ApplicationConstants.UILayout.MouseWheelButtonRadius),
                "MouseButton4" or "MouseButton5" => new CornerRadius(
                    ApplicationConstants.UILayout.MouseButtonRadius,
                    ApplicationConstants.UILayout.MouseSideButtonRadius,
                    ApplicationConstants.UILayout.MouseSideButtonRadius,
                    ApplicationConstants.UILayout.MouseButtonRadius),
                _ => new CornerRadius(ApplicationConstants.UILayout.MouseButtonRadius)
            };
        }

        /// <summary>
        /// マウスボタン用のDropShadowEffectを作成
        /// </summary>
        private static DropShadowEffect CreateMouseButtonShadowEffect()
        {
            return new DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = ApplicationConstants.UILayout.MouseButtonShadowBlur,
                ShadowDepth = ApplicationConstants.UILayout.MouseButtonShadowDepth,
                Opacity = ApplicationConstants.UILayout.MouseButtonShadowOpacity
            };
        }

        /// <summary>
        /// マウス本体用のDropShadowEffectを作成
        /// </summary>
        private static DropShadowEffect CreateMouseBodyShadowEffect()
        {
            return new DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = ApplicationConstants.UILayout.MouseBodyShadowBlur,
                ShadowDepth = ApplicationConstants.UILayout.MouseBodyShadowDepth,
                Opacity = ApplicationConstants.UILayout.MouseBodyShadowOpacity
            };
        }

        #endregion
    }
}