using System;
using System.Collections.Generic;
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
        /// Canvas子要素をFrameworkElementとして反復処理
        /// </summary>
        /// <param name="canvas">対象のCanvas</param>
        /// <param name="action">各要素に対して実行するアクション</param>
        public static void ForEachFrameworkElement(Canvas canvas, Action<FrameworkElement> action)
        {
            if (canvas == null || action == null) return;

            foreach (var child in canvas.Children)
            {
                if (child is FrameworkElement element)
                {
                    action(element);
                }
            }
        }

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
        /// Canvas子要素の名前でFrameworkElementを検索
        /// </summary>
        /// <param name="canvas">対象のCanvas</param>
        /// <param name="name">要素名</param>
        /// <returns>見つかった要素（見つからない場合はnull）</returns>
        public static T? FindElementByName<T>(Canvas canvas, string name) where T : FrameworkElement
        {
            if (canvas == null || string.IsNullOrEmpty(name)) return null;

            foreach (var child in canvas.Children)
            {
                if (child is T element && element.Name == name)
                {
                    return element;
                }
            }
            return null;
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

        /// <summary>
        /// 複数の要素の可視性を一括設定
        /// </summary>
        /// <param name="elements">対象の要素リスト</param>
        /// <param name="isVisible">可視性</param>
        public static void SetVisibilityBatch(IEnumerable<FrameworkElement> elements, bool isVisible)
        {
            if (elements == null) return;

            var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            foreach (var element in elements)
            {
                if (element != null)
                {
                    element.Visibility = visibility;
                }
            }
        }
    }
}