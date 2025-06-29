using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.MouseVisualization
{
    /// <summary>
    /// マウス移動方向の可視化を担当するクラス
    /// </summary>
    public class MouseDirectionVisualizer : IDisposable
    {
        private readonly UIElementLocator _elementLocator;
        private DispatcherTimer? _hideTimer;
        private bool _disposed = false;

        public MouseDirectionVisualizer(UIElementLocator elementLocator)
        {
            _elementLocator = elementLocator ?? throw new ArgumentNullException(nameof(elementLocator));
        }

        /// <summary>
        /// MouseTrackerとの連携を初期化
        /// </summary>
        public void Initialize(MouseTracker mouseTracker)
        {
            if (mouseTracker == null) throw new ArgumentNullException(nameof(mouseTracker));
            
            mouseTracker.MouseMoved += OnMouseMoved;
        }

        private void OnMouseMoved(object? sender, MouseMoveEventArgs e)
        {
            // マウス移動方向インジケーターの表示処理
            var directionCanvas = _elementLocator.FindElement<Canvas>("MouseDirectionCanvas");
            if (directionCanvas == null) return;

            // 方向インジケーターを探して更新
            var directionName = $"Direction{e.Direction}";
            var indicator = _elementLocator.FindElement<System.Windows.Shapes.Path>(directionName);
            
            if (indicator != null)
            {
                // 他のインジケーターをリセット
                ResetDirectionIndicators(directionCanvas);
                
                // 該当方向をハイライト
                indicator.Opacity = 0.9;
                
                // 自動非表示タイマーを開始
                StartHideTimer(directionCanvas);
            }
        }

        /// <summary>
        /// 方向インジケーターをリセット
        /// </summary>
        private void ResetDirectionIndicators(Canvas directionCanvas)
        {
            foreach (UIElement child in directionCanvas.Children)
            {
                if (child is System.Windows.Shapes.Path path && path.Name?.StartsWith("Direction") == true)
                {
                    path.Opacity = 0.0;
                }
            }
        }

        /// <summary>
        /// 自動非表示タイマーを開始
        /// </summary>
        private void StartHideTimer(Canvas directionCanvas)
        {
            _hideTimer?.Stop();
            _hideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ApplicationConstants.Timing.DirectionHideDelay)
            };
            _hideTimer.Tick += (sender, e) =>
            {
                ResetDirectionIndicators(directionCanvas);
                _hideTimer?.Stop();
            };
            _hideTimer.Start();
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _hideTimer?.Stop();
                    _hideTimer = null;
                }
                _disposed = true;
            }
        }
    }
}