using System;
using System.Windows;
using System.Windows.Controls;

namespace KeyOverlayFPS.Utils
{
    /// <summary>
    /// Canvas要素の共通操作を提供するヘルパークラス
    /// 重複するCanvas要素操作を統一化
    /// </summary>
    public static class CanvasElementHelper
    {
        /// <summary>
        /// Canvas子要素からBorder要素のみを抽出して反復処理
        /// </summary>
        /// <param name="canvas">対象のCanvas</param>
        /// <param name="action">各Border要素に対して実行するアクション</param>
        public static void ForEachBorderElement(Canvas canvas, Action<Border> action)
        {
            if (canvas == null || action == null) return;

            foreach (var child in canvas.Children)
            {
                if (child is Border border)
                {
                    action(border);
                }
            }
        }

        /// <summary>
        /// Canvas子要素から条件に一致するFrameworkElementを抽出して反復処理
        /// </summary>
        /// <param name="canvas">対象のCanvas</param>
        /// <param name="predicate">要素の条件判定</param>
        /// <param name="action">条件に一致した要素に対して実行するアクション</param>
        public static void ForEachMatchingElement(Canvas canvas, Func<FrameworkElement, bool> predicate, Action<FrameworkElement> action)
        {
            if (canvas == null || predicate == null || action == null) return;

            foreach (var child in canvas.Children)
            {
                if (child is FrameworkElement element && predicate(element))
                {
                    action(element);
                }
            }
        }

        /// <summary>
        /// Canvas要素の位置を設定
        /// </summary>
        /// <param name="element">対象の要素</param>
        /// <param name="left">左位置</param>
        /// <param name="top">上位置</param>
        public static void SetPosition(FrameworkElement element, double left, double top)
        {
            if (element == null) return;

            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);
        }

        /// <summary>
        /// Canvas要素の可視性を設定
        /// </summary>
        /// <param name="element">対象の要素</param>
        /// <param name="isVisible">可視性</param>
        public static void SetVisibility(FrameworkElement element, bool isVisible)
        {
            if (element == null) return;

            element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}