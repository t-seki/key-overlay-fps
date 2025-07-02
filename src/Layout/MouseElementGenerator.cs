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
            
            return UIElementFactory.CreateMouseBodyBorder();
        }

        /// <summary>
        /// マウスボタン要素を作成
        /// </summary>
        private static Border CreateMouseButton(string buttonName, ButtonConfig buttonConfig)
        {
            var border = UIElementFactory.CreateMouseButtonBorder(
                buttonName,
                buttonConfig.Size.Width,
                buttonConfig.Size.Height
            );

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
        /// スクロールインジケーターを作成
        /// </summary>
        private static StackPanel CreateScrollIndicators()
        {
            var stackPanel = new StackPanel();
            
            var scrollUpIndicator = UIElementFactory.CreateScrollIndicatorTextBlock("ScrollUpIndicator", "▲");
            var scrollDownIndicator = UIElementFactory.CreateScrollIndicatorTextBlock("ScrollDownIndicator", "▼");

            stackPanel.Children.Add(scrollUpIndicator);
            stackPanel.Children.Add(scrollDownIndicator);
            return stackPanel;
        }

        /// <summary>
        /// 空のボタンコンテンツを作成
        /// </summary>
        private static TextBlock CreateEmptyButtonContent()
        {
            return UIElementFactory.CreateMouseButtonTextBlock();
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