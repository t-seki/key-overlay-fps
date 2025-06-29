using System;
using System.Collections.Generic;

namespace KeyOverlayFPS.MouseVisualization
{
    /// <summary>
    /// マウス移動方向の計算とユーティリティ機能を提供するクラス
    /// </summary>
    public static class DirectionCalculator
    {
        /// <summary>
        /// 各方向の中心角度（度）- 16方向対応
        /// </summary>
        private static readonly Dictionary<MouseDirection, double> DirectionCenterAngles = new()
        {
            { MouseDirection.East, 0.0 },
            { MouseDirection.EastNorthEast, 22.5 },
            { MouseDirection.NorthEast, 45.0 },
            { MouseDirection.NorthNorthEast, 67.5 },
            { MouseDirection.North, 90.0 },
            { MouseDirection.NorthNorthWest, 112.5 },
            { MouseDirection.NorthWest, 135.0 },
            { MouseDirection.WestNorthWest, 157.5 },
            { MouseDirection.West, 180.0 },
            { MouseDirection.WestSouthWest, 202.5 },
            { MouseDirection.SouthWest, 225.0 },
            { MouseDirection.SouthSouthWest, 247.5 },
            { MouseDirection.South, 270.0 },
            { MouseDirection.SouthSouthEast, 292.5 },
            { MouseDirection.SouthEast, 315.0 },
            { MouseDirection.EastSouthEast, 337.5 }
        };


        /// <summary>
        /// 移動量から方向を計算
        /// </summary>
        /// <param name="deltaX">X軸の移動量</param>
        /// <param name="deltaY">Y軸の移動量</param>
        /// <returns>計算された方向</returns>
        public static MouseDirection CalculateDirection(double deltaX, double deltaY)
        {
            // 移動量が0の場合は東を返す（デフォルト）
            if (Math.Abs(deltaX) < 0.001 && Math.Abs(deltaY) < 0.001)
                return MouseDirection.East;

            // 角度を計算（ラジアン）
            // Y軸は下向きが正なので反転し、座標系を数学的な座標系に合わせる
            var angle = Math.Atan2(-deltaY, deltaX);
            
            // ラジアンを度に変換
            var degrees = angle * 180.0 / Math.PI;
            
            // 0-360度の範囲に正規化
            if (degrees < 0) degrees += 360;
            
            // 方向を決定
            return GetDirectionFromAngle(degrees);
        }

        /// <summary>
        /// 角度から方向を取得（16方向対応）
        /// </summary>
        /// <param name="degrees">角度（度）</param>
        /// <returns>対応する方向</returns>
        public static MouseDirection GetDirectionFromAngle(double degrees)
        {
            // 角度を0-360度の範囲に正規化
            degrees = ((degrees % 360) + 360) % 360;

            // 16方向に分割（22.5度刻み）
            var directionIndex = (int)Math.Round(degrees / 22.5) % 16;
            
            return (MouseDirection)directionIndex;
        }


        /// <summary>
        /// 方向から中心角度を取得（16方向対応）
        /// </summary>
        /// <param name="direction">方向</param>
        /// <returns>中心角度（度）</returns>
        public static double GetCenterAngle(MouseDirection direction)
        {
            return DirectionCenterAngles.TryGetValue(direction, out var angle) ? angle : 0.0;
        }

    }
}