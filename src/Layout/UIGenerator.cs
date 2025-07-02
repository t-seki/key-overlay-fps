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
            foreach (UIElement child in canvas.Children)
            {
                if (child is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
                {
                    try
                    {
                        parentWindow.UnregisterName(element.Name);
                        
                        // 子要素も解除
                        UnregisterChildElementNames(element, parentWindow);
                    }
                    catch
                    {
                        // 未登録の場合はスキップ
                    }
                }
            }
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
                        // 既存の名前がある場合は先に解除してから再登録
                        try
                        {
                            parentWindow.UnregisterName(element.Name);
                        }
                        catch { }
                        
                        parentWindow.RegisterName(element.Name, element);
                        
                        // 子要素も登録
                        RegisterChildElementNames(element, parentWindow);
                    }
                    catch
                    {
                        // 登録に失敗した場合はスキップ
                    }
                }
            }
        }

        /// <summary>
        /// 子要素の名前を再帰的に解除
        /// </summary>
        private static void UnregisterChildElementNames(FrameworkElement parent, Window parentWindow)
        {
            if (parent is Border border && border.Child is FrameworkElement borderChild)
            {
                if (!string.IsNullOrEmpty(borderChild.Name))
                {
                    try
                    {
                        parentWindow.UnregisterName(borderChild.Name);
                    }
                    catch
                    {
                        // 未登録の場合はスキップ
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
                                parentWindow.UnregisterName(stackChild.Name);
                            }
                            catch
                            {
                                // 未登録の場合はスキップ
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
                            parentWindow.UnregisterName(childElement.Name);
                        }
                        catch
                        {
                            // 未登録の場合はスキップ
                        }
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
                        // 既存の名前がある場合は先に解除してから再登録
                        try
                        {
                            parentWindow.UnregisterName(borderChild.Name);
                        }
                        catch { }
                        
                        parentWindow.RegisterName(borderChild.Name, borderChild);
                    }
                    catch
                    {
                        // 登録に失敗した場合はスキップ
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
                                // 既存の名前がある場合は先に解除してから再登録
                                try
                                {
                                    parentWindow.UnregisterName(stackChild.Name);
                                }
                                catch { }
                                
                                parentWindow.RegisterName(stackChild.Name, stackChild);
                            }
                            catch
                            {
                                // 登録に失敗した場合はスキップ
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
                            // 既存の名前がある場合は先に解除してから再登録
                            try
                            {
                                parentWindow.UnregisterName(childElement.Name);
                            }
                            catch { }
                            
                            parentWindow.RegisterName(childElement.Name, childElement);
                        }
                        catch
                        {
                            // 登録に失敗した場合はスキップ
                        }
                    }
                }
            }
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