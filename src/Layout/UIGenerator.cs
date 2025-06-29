using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.MouseVisualization;

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
                Margin = new Thickness(8),
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
            GenerateKeyElements(canvas, layout);

            // マウス要素を生成
            GenerateMouseElements(canvas, layout);

            return canvas;
        }

        /// <summary>
        /// キーボードキー要素を生成
        /// </summary>
        private static void GenerateKeyElements(Canvas canvas, LayoutConfig layout)
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
        /// マウス要素を生成
        /// </summary>
        private static void GenerateMouseElements(Canvas canvas, LayoutConfig layout)
        {
            if (layout.Mouse == null || !layout.Mouse.IsVisible) return;

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
            Canvas.SetLeft(directionCanvas, layout.Mouse.Position.X + 15);
            Canvas.SetTop(directionCanvas, layout.Mouse.Position.Y + 50);
            canvas.Children.Add(directionCanvas);
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
                BorderBrush = GetBrushFromColor(global.ForegroundColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Background = BrushFactory.CreateKeyboardKeyBackground()  // 共通ファクトリーから生成
            };
            
            // イベントハンドラーは後でKeyEventBinderで設定されます

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
                Foreground = GetBrushFromColor(global.ForegroundColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(global.FontFamily))
            {
                textBlock.FontFamily = new FontFamily(global.FontFamily);
            }

            return textBlock;
        }

        /// <summary>
        /// マウス本体要素を作成
        /// </summary>
        private static Border CreateMouseBody(MouseSettings mouseSettings)
        {
            var border = new Border
            {
                Name = "MouseBody",
                Width = 60,
                Height = 100,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(28, 28, 25, 25),
                Background = BrushFactory.CreateMouseBodyBackground(),
                Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = 3,
                    ShadowDepth = 2,
                    Opacity = 0.5
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
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                BorderThickness = new Thickness(1),
                Background = BrushFactory.CreateMouseButtonBackground(),
                Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    BlurRadius = 2,
                    ShadowDepth = 1,
                    Opacity = 0.3
                }
            };

            // ボタン種類に応じてCornerRadiusを設定
            border.CornerRadius = buttonName switch
            {
                "MouseLeft" => new CornerRadius(12, 5, 8, 8),
                "MouseRight" => new CornerRadius(5, 12, 8, 8),
                "MouseWheelButton" => new CornerRadius(5),
                "MouseButton4" or "MouseButton5" => new CornerRadius(5, 2, 2, 5),
                _ => new CornerRadius(2)
            };

            // ホイールボタンの場合はスクロールインジケーターを追加
            if (buttonName == "MouseWheelButton")
            {
                var stackPanel = new StackPanel();
                
                var scrollUpIndicator = new TextBlock
                {
                    Name = "ScrollUpIndicator",
                    Text = "▲",
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var scrollDownIndicator = new TextBlock
                {
                    Name = "ScrollDownIndicator",
                    Text = "▼",
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(scrollUpIndicator);
                stackPanel.Children.Add(scrollDownIndicator);
                border.Child = stackPanel;
            }
            else
            {
                // 通常のマウスボタンには空のTextBlockを配置
                border.Child = new TextBlock
                {
                    Text = "",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            return border;
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

        /// <summary>
        /// 色文字列からBrushを取得
        /// </summary>
        private static Brush GetBrushFromColor(string colorString)
        {
            return BrushFactory.CreateBrushFromString(colorString, Brushes.White);
        }

        /// <summary>
        /// 動的UI要素に名前を登録（後のイベントバインド用）
        /// </summary>
        public static void RegisterElementNames(Canvas canvas, Window parentWindow)
        {
            foreach (UIElement child in canvas.Children)
            {
                if (child is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
                {
                    try
                    {
                        parentWindow.RegisterName(element.Name, element);
                        
                        // 子要素も登録
                        RegisterChildElementNames(element, parentWindow);
                    }
                    catch
                    {
                        // 既に登録されている場合はスキップ
                    }
                }
            }
        }

        /// <summary>
        /// 子要素の名前を再帰的に登録
        /// </summary>
        private static void RegisterChildElementNames(FrameworkElement parent, Window parentWindow)
        {
            if (parent is Border border && border.Child is FrameworkElement borderChild)
            {
                if (!string.IsNullOrEmpty(borderChild.Name))
                {
                    try
                    {
                        parentWindow.RegisterName(borderChild.Name, borderChild);
                    }
                    catch
                    {
                        // 既に登録されている場合はスキップ
                    }
                }

                if (borderChild is StackPanel stackPanel)
                {
                    foreach (var child in stackPanel.Children)
                    {
                        if (child is FrameworkElement stackChild && !string.IsNullOrEmpty(stackChild.Name))
                        {
                            try
                            {
                                parentWindow.RegisterName(stackChild.Name, stackChild);
                            }
                            catch
                            {
                                // 既に登録されている場合はスキップ
                            }
                        }
                    }
                }
            }
            else if (parent is Canvas childCanvas)
            {
                foreach (UIElement child in childCanvas.Children)
                {
                    if (child is FrameworkElement childElement && !string.IsNullOrEmpty(childElement.Name))
                    {
                        try
                        {
                            parentWindow.RegisterName(childElement.Name, childElement);
                        }
                        catch
                        {
                            // 既に登録されている場合はスキップ
                        }
                    }
                }
            }
        }
    }
}