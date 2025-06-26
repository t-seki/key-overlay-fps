using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace KeyOverlayFPS.MouseVisualization
{
    /// <summary>
    /// マウスの位置とクリック状態を追跡するクラス
    /// </summary>
    public class MouseTracker
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private Point _lastPosition;
        private bool _isInitialized = false;

        /// <summary>
        /// マウスの移動が発生したときのイベント
        /// </summary>
        public event EventHandler<MouseMoveEventArgs>? MouseMoved;

        /// <summary>
        /// マウス位置を更新し、移動を検出
        /// </summary>
        /// <param name="threshold">移動を検出する最小ピクセル数</param>
        public void Update(double threshold = 5.0)
        {
            var currentPosition = GetCurrentMousePosition();
            
            if (!_isInitialized)
            {
                _lastPosition = currentPosition;
                _isInitialized = true;
                return;
            }

            var deltaX = currentPosition.X - _lastPosition.X;
            var deltaY = currentPosition.Y - _lastPosition.Y;
            var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance >= threshold)
            {
                var direction = CalculateDirection(deltaX, deltaY);
                MouseMoved?.Invoke(this, new MouseMoveEventArgs(deltaX, deltaY, direction, distance));
                
                _lastPosition = currentPosition;
            }
        }

        /// <summary>
        /// 現在のマウス位置を取得
        /// </summary>
        /// <returns>マウス位置</returns>
        private static Point GetCurrentMousePosition()
        {
            if (GetCursorPos(out POINT point))
            {
                return new Point(point.X, point.Y);
            }
            return new Point(0, 0);
        }

        /// <summary>
        /// 移動方向を計算（16方向）
        /// </summary>
        /// <param name="deltaX">X軸の移動量</param>
        /// <param name="deltaY">Y軸の移動量</param>
        /// <returns>移動方向</returns>
        private static MouseDirection CalculateDirection(double deltaX, double deltaY)
        {
            // 角度を計算（ラジアン）
            var angle = Math.Atan2(-deltaY, deltaX); // Y軸は下向きが正なので反転
            
            // ラジアンを度に変換し、0-360度の範囲に正規化
            var degrees = angle * 180.0 / Math.PI;
            if (degrees < 0) degrees += 360;
            
            // 16方向に分割（22.5度刻み）
            var directionIndex = (int)Math.Round(degrees / 22.5) % 16;
            
            return (MouseDirection)directionIndex;
        }

        /// <summary>
        /// 追跡をリセット
        /// </summary>
        public void Reset()
        {
            _isInitialized = false;
        }
    }

    /// <summary>
    /// マウス移動イベントの引数
    /// </summary>
    public class MouseMoveEventArgs : EventArgs
    {
        /// <summary>
        /// X軸の移動量
        /// </summary>
        public double DeltaX { get; }

        /// <summary>
        /// Y軸の移動量
        /// </summary>
        public double DeltaY { get; }

        /// <summary>
        /// 移動方向
        /// </summary>
        public MouseDirection Direction { get; }

        /// <summary>
        /// 移動距離
        /// </summary>
        public double Distance { get; }

        public MouseMoveEventArgs(double deltaX, double deltaY, MouseDirection direction, double distance)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
            Direction = direction;
            Distance = distance;
        }
    }

    /// <summary>
    /// マウスの移動方向（16方向）
    /// </summary>
    public enum MouseDirection
    {
        /// <summary>東（右）</summary>
        East = 0,
        /// <summary>東北東</summary>
        EastNorthEast = 1,
        /// <summary>北東</summary>
        NorthEast = 2,
        /// <summary>北北東</summary>
        NorthNorthEast = 3,
        /// <summary>北（上）</summary>
        North = 4,
        /// <summary>北北西</summary>
        NorthNorthWest = 5,
        /// <summary>北西</summary>
        NorthWest = 6,
        /// <summary>西北西</summary>
        WestNorthWest = 7,
        /// <summary>西（左）</summary>
        West = 8,
        /// <summary>西南西</summary>
        WestSouthWest = 9,
        /// <summary>南西</summary>
        SouthWest = 10,
        /// <summary>南南西</summary>
        SouthSouthWest = 11,
        /// <summary>南（下）</summary>
        South = 12,
        /// <summary>南南東</summary>
        SouthSouthEast = 13,
        /// <summary>南東</summary>
        SouthEast = 14,
        /// <summary>東南東</summary>
        EastSouthEast = 15
    }
}