using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace KeyOverlayFPS.Layout
{
    /// <summary>
    /// 動的生成されたUI要素の検索とキャッシュ管理を担当するクラス
    /// </summary>
    public class UIElementLocator
    {
        private readonly Dictionary<string, FrameworkElement> _elementCache = new();

        /// <summary>
        /// 要素キャッシュを構築
        /// </summary>
        public void BuildCache(Canvas canvas)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));
            
            _elementCache.Clear();
            BuildElementCacheRecursive(canvas);
        }

        /// <summary>
        /// 要素を名前で検索（キャッシュ付き）
        /// </summary>
        public T? FindElement<T>(string name) where T : FrameworkElement
        {
            return _elementCache.TryGetValue(name, out var element) ? element as T : null;
        }

        /// <summary>
        /// 要素キャッシュを再帰的に構築
        /// </summary>
        private void BuildElementCacheRecursive(DependencyObject parent)
        {
            if (parent is FrameworkElement element && !string.IsNullOrEmpty(element.Name))
            {
                _elementCache[element.Name] = element;
            }

            // 子要素を再帰的に処理
            var childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                BuildElementCacheRecursive(child);
            }

            // Canvas子要素の特別処理
            if (parent is Canvas canvas)
            {
                foreach (UIElement child in canvas.Children)
                {
                    BuildElementCacheRecursive(child);
                }
            }

            // Border子要素の特別処理
            if (parent is Border border && border.Child != null)
            {
                BuildElementCacheRecursive(border.Child);
            }

            // StackPanel子要素の特別処理
            if (parent is StackPanel stackPanel)
            {
                foreach (UIElement child in stackPanel.Children)
                {
                    BuildElementCacheRecursive(child);
                }
            }
        }
    }
}