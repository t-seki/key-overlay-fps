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
        /// 移動方向を計算（32方向）
        /// </summary>
        /// <param name="deltaX">X軸の移動量</param>
        /// <param name="deltaY">Y軸の移動量</param>
        /// <returns>移動方向</returns>
        private static MouseDirection CalculateDirection(double deltaX, double deltaY)
        {
            // 角度を計算（ラジアン） - Y軸反転で数学座標系に変換
            var angle = Math.Atan2(-deltaY, deltaX); // Y軸反転: 上移動で正の角度
            
            // ラジアンを度に変換し、0-360度の範囲に正規化
            var degrees = angle * 180.0 / Math.PI;
            if (degrees < 0) degrees += 360;
            
            // MouseDirection列挙型に合わせた角度計算
            // East(0) = 0度, North(8) = 90度, West(16) = 180度, South(24) = 270度
            var directionIndex = (int)Math.Round(degrees / 11.25) % 32;
            
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
    /// マウスの移動方向（32方向）
    /// </summary>
    public enum MouseDirection
    {
        /// <summary>東（右）</summary>
        East = 0,
        /// <summary>東微北</summary>
        East_11_25 = 1,
        /// <summary>東北東</summary>
        EastNorthEast = 2,
        /// <summary>東北東微北</summary>
        East_33_75 = 3,
        /// <summary>北東</summary>
        NorthEast = 4,
        /// <summary>北東微北</summary>
        North_56_25 = 5,
        /// <summary>北北東</summary>
        NorthNorthEast = 6,
        /// <summary>北北東微北</summary>
        North_78_75 = 7,
        /// <summary>北（上）</summary>
        North = 8,
        /// <summary>北微西</summary>
        North_101_25 = 9,
        /// <summary>北北西</summary>
        NorthNorthWest = 10,
        /// <summary>北北西微西</summary>
        North_123_75 = 11,
        /// <summary>北西</summary>
        NorthWest = 12,
        /// <summary>北西微西</summary>
        West_146_25 = 13,
        /// <summary>西北西</summary>
        WestNorthWest = 14,
        /// <summary>西北西微西</summary>
        West_168_75 = 15,
        /// <summary>西（左）</summary>
        West = 16,
        /// <summary>西微南</summary>
        West_191_25 = 17,
        /// <summary>西南西</summary>
        WestSouthWest = 18,
        /// <summary>西南西微南</summary>
        West_213_75 = 19,
        /// <summary>南西</summary>
        SouthWest = 20,
        /// <summary>南西微南</summary>
        South_236_25 = 21,
        /// <summary>南南西</summary>
        SouthSouthWest = 22,
        /// <summary>南南西微南</summary>
        South_258_75 = 23,
        /// <summary>南（下）</summary>
        South = 24,
        /// <summary>南微東</summary>
        South_281_25 = 25,
        /// <summary>南南東</summary>
        SouthSouthEast = 26,
        /// <summary>南南東微東</summary>
        South_303_75 = 27,
        /// <summary>南東</summary>
        SouthEast = 28,
        /// <summary>南東微東</summary>
        South_326_25 = 29,
        /// <summary>東南東</summary>
        EastSouthEast = 30,
        /// <summary>東南東微東</summary>
        East_348_75 = 31
    }
}