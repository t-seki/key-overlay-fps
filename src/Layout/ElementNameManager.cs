using System.Windows;
using System.Windows.Controls;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// UI要素の名前登録・解除を担当するクラス
    /// </summary>
    public static class ElementNameManager
    {
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
    }
}