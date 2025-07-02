using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.MouseVisualization;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// マウス要素の生成を担当するクラス
    /// </summary>
    public static class MouseElementGenerator
    {
        /// <summary>
        /// マウス要素を生成
        /// </summary>
        public static void GenerateMouseElements(Canvas canvas, LayoutConfig layout)
        {
            if (layout.Mouse == null) return;

            // マウス本体を生成
            var mouseBody = CreateMouseBody(layout.Mouse);
            Canvas.SetLeft(mouseBody, layout.Mouse.Position.X);
            Canvas.SetTop(mouseBody, layout.Mouse.Position.Y);
            canvas.Children.Add(mouseBody);

            // マウスボタンを生成
            foreach (var (buttonName, buttonConfig) in layout.Mouse.Buttons)
            {
                if (!buttonConfig.IsVisible) continue;

                var button = CreateMouseButton(buttonName, buttonConfig);
                Canvas.SetLeft(button, layout.Mouse.Position.X + buttonConfig.Offset.X);
                Canvas.SetTop(button, layout.Mouse.Position.Y + buttonConfig.Offset.Y);
                canvas.Children.Add(button);
            }

            // マウス移動可視化キャンバスを生成
            var directionCanvas = CreateMouseDirectionCanvas(layout.Mouse);
            Canvas.SetLeft(directionCanvas, layout.Mouse.Position.X + ApplicationConstants.MouseVisualization.DirectionCanvasOffsetX);
            Canvas.SetTop(directionCanvas, layout.Mouse.Position.Y + ApplicationConstants.MouseVisualization.DirectionCanvasOffsetY);
            canvas.Children.Add(directionCanvas);
        }

        /// <summary>
        /// マウス本体要素を作成
        /// </summary>
        private static Border CreateMouseBody(MouseSettings mouseSettings)
        {
            if (mouseSettings == null)
            {
                throw new ArgumentNullException(nameof(mouseSettings));
            }
            
            var border = new Border
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
                Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = ApplicationConstants.UILayout.MouseBodyShadowBlur,
                    ShadowDepth = ApplicationConstants.UILayout.MouseBodyShadowDepth,
                    Opacity = ApplicationConstants.UILayout.MouseBodyShadowOpacity
                }
            };

            return border;
        }

        /// <summary>
        /// マウスボタン要素を作成
        /// </summary>
        private static Border CreateMouseButton(string buttonName, ButtonConfig buttonConfig)
        {
            var border = new Border
            {
                Name = buttonName,
                Width = buttonConfig.Size.Width,
                Height = buttonConfig.Size.Height,
                BorderBrush = new SolidColorBrush(ApplicationConstants.Colors.MouseButtonBorderColor),
                BorderThickness = new Thickness(ApplicationConstants.UILayout.MouseButtonBorderThickness),
                Background = BrushFactory.CreateMouseButtonBackground(),
                Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = ApplicationConstants.UILayout.MouseButtonShadowBlur,
                    ShadowDepth = ApplicationConstants.UILayout.MouseButtonShadowDepth,
                    Opacity = ApplicationConstants.UILayout.MouseButtonShadowOpacity
                }
            };

            // ボタン種類に応じてCornerRadiusを設定
            border.CornerRadius = GetCornerRadiusForButton(buttonName);

            // ホイールボタンの場合はスクロールインジケーターを追加
            if (buttonName == "MouseWheelButton")
            {
                border.Child = CreateScrollIndicators();
            }
            else
            {
                border.Child = CreateEmptyButtonContent();
            }

            return border;
        }

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
        /// スクロールインジケーターを作成
        /// </summary>
        private static StackPanel CreateScrollIndicators()
        {
            var stackPanel = new StackPanel();
            
            var scrollUpIndicator = new TextBlock
            {
                Name = "ScrollUpIndicator",
                Text = "▲",
                FontSize = ApplicationConstants.UILayout.ScrollIndicatorFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Transparent,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var scrollDownIndicator = new TextBlock
            {
                Name = "ScrollDownIndicator",
                Text = "▼",
                FontSize = ApplicationConstants.UILayout.ScrollIndicatorFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Transparent,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(scrollUpIndicator);
            stackPanel.Children.Add(scrollDownIndicator);
            return stackPanel;
        }

        /// <summary>
        /// 空のボタンコンテンツを作成
        /// </summary>
        private static TextBlock CreateEmptyButtonContent()
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
        /// マウス移動可視化キャンバスを作成
        /// </summary>
        private static Canvas CreateMouseDirectionCanvas(MouseSettings mouseSettings)
        {
            var canvas = new Canvas
            {
                Name = "MouseDirectionCanvas",
                Width = ApplicationConstants.MouseVisualization.CanvasSize,
                Height = ApplicationConstants.MouseVisualization.CanvasSize,
                IsHitTestVisible = false
            };

            // 基準円と中心点を作成
            DirectionArcGenerator.CreateBaseCircleAndCenter(canvas);

            // 16方向の円弧を一括生成
            DirectionArcGenerator.CreateAllDirectionArcs(canvas);

            return canvas;
        }
    }
}