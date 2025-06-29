using System.Windows;
using System.Windows.Input;

namespace KeyOverlayFPS.UI
{
    /// <summary>
    /// ウィンドウのドラッグ操作を管理するクラス
    /// </summary>
    public class WindowDragHandler
    {
        private readonly Window _window;
        private bool _isDragging = false;
        private Point _dragStartPoint;

        public WindowDragHandler(Window window)
        {
            _window = window ?? throw new System.ArgumentNullException(nameof(window));
        }

        /// <summary>
        /// ドラッグ操作を開始
        /// </summary>
        public void StartDrag(MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(_window);
            _window.CaptureMouse();
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        public void HandleMouseMove(MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(_window);
                var screen = _window.PointToScreen(currentPosition);
                var window = _window.PointToScreen(_dragStartPoint);
                
                _window.Left = screen.X - window.X + _window.Left;
                _window.Top = screen.Y - window.Y + _window.Top;
            }
        }

        /// <summary>
        /// ドラッグ操作を終了
        /// </summary>
        public void EndDrag()
        {
            if (_isDragging)
            {
                _isDragging = false;
                _window.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// 現在ドラッグ中かどうか
        /// </summary>
        public bool IsDragging => _isDragging;
    }
}