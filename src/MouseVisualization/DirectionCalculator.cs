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
        /// 各方向の中心角度（度）- 32方向対応
        /// </summary>
        private static readonly Dictionary<MouseDirection, double> DirectionCenterAngles = new()
        {
            { MouseDirection.East, 0.0 },
            { MouseDirection.East_11_25, 11.25 },
            { MouseDirection.EastNorthEast, 22.5 },
            { MouseDirection.East_33_75, 33.75 },
            { MouseDirection.NorthEast, 45.0 },
            { MouseDirection.North_56_25, 56.25 },
            { MouseDirection.NorthNorthEast, 67.5 },
            { MouseDirection.North_78_75, 78.75 },
            { MouseDirection.North, 90.0 },
            { MouseDirection.North_101_25, 101.25 },
            { MouseDirection.NorthNorthWest, 112.5 },
            { MouseDirection.North_123_75, 123.75 },
            { MouseDirection.NorthWest, 135.0 },
            { MouseDirection.West_146_25, 146.25 },
            { MouseDirection.WestNorthWest, 157.5 },
            { MouseDirection.West_168_75, 168.75 },
            { MouseDirection.West, 180.0 },
            { MouseDirection.West_191_25, 191.25 },
            { MouseDirection.WestSouthWest, 202.5 },
            { MouseDirection.West_213_75, 213.75 },
            { MouseDirection.SouthWest, 225.0 },
            { MouseDirection.South_236_25, 236.25 },
            { MouseDirection.SouthSouthWest, 247.5 },
            { MouseDirection.South_258_75, 258.75 },
            { MouseDirection.South, 270.0 },
            { MouseDirection.South_281_25, 281.25 },
            { MouseDirection.SouthSouthEast, 292.5 },
            { MouseDirection.South_303_75, 303.75 },
            { MouseDirection.SouthEast, 315.0 },
            { MouseDirection.South_326_25, 326.25 },
            { MouseDirection.EastSouthEast, 337.5 },
            { MouseDirection.East_348_75, 348.75 }
        };

        /// <summary>
        /// 各方向の表示名（32方向対応）
        /// </summary>
        private static readonly Dictionary<MouseDirection, string> DirectionNames = new()
        {
            { MouseDirection.East, "東" },
            { MouseDirection.East_11_25, "東微北" },
            { MouseDirection.EastNorthEast, "東北東" },
            { MouseDirection.East_33_75, "東北東微北" },
            { MouseDirection.NorthEast, "北東" },
            { MouseDirection.North_56_25, "北東微北" },
            { MouseDirection.NorthNorthEast, "北北東" },
            { MouseDirection.North_78_75, "北北東微北" },
            { MouseDirection.North, "北" },
            { MouseDirection.North_101_25, "北微西" },
            { MouseDirection.NorthNorthWest, "北北西" },
            { MouseDirection.North_123_75, "北北西微西" },
            { MouseDirection.NorthWest, "北西" },
            { MouseDirection.West_146_25, "北西微西" },
            { MouseDirection.WestNorthWest, "西北西" },
            { MouseDirection.West_168_75, "西北西微西" },
            { MouseDirection.West, "西" },
            { MouseDirection.West_191_25, "西微南" },
            { MouseDirection.WestSouthWest, "西南西" },
            { MouseDirection.West_213_75, "西南西微南" },
            { MouseDirection.SouthWest, "南西" },
            { MouseDirection.South_236_25, "南西微南" },
            { MouseDirection.SouthSouthWest, "南南西" },
            { MouseDirection.South_258_75, "南南西微南" },
            { MouseDirection.South, "南" },
            { MouseDirection.South_281_25, "南微東" },
            { MouseDirection.SouthSouthEast, "南南東" },
            { MouseDirection.South_303_75, "南南東微東" },
            { MouseDirection.SouthEast, "南東" },
            { MouseDirection.South_326_25, "南東微東" },
            { MouseDirection.EastSouthEast, "東南東" },
            { MouseDirection.East_348_75, "東南東微東" }
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
        /// 角度から方向を取得（32方向対応）
        /// </summary>
        /// <param name="degrees">角度（度）</param>
        /// <returns>対応する方向</returns>
        public static MouseDirection GetDirectionFromAngle(double degrees)
        {
            // 角度を0-360度の範囲に正規化
            degrees = ((degrees % 360) + 360) % 360;

            // 32方向に分割（11.25度刻み）
            var directionIndex = (int)Math.Round(degrees / 11.25) % 32;
            
            return (MouseDirection)directionIndex;
        }

        /// <summary>
        /// 方向の表示名を取得
        /// </summary>
        /// <param name="direction">方向</param>
        /// <returns>表示名</returns>
        public static string GetDirectionName(MouseDirection direction)
        {
            return DirectionNames.TryGetValue(direction, out var name) ? name : direction.ToString();
        }

        /// <summary>
        /// 方向から中心角度を取得（32方向対応）
        /// </summary>
        /// <param name="direction">方向</param>
        /// <returns>中心角度（度）</returns>
        public static double GetCenterAngle(MouseDirection direction)
        {
            return DirectionCenterAngles.TryGetValue(direction, out var angle) ? angle : 0.0;
        }


        /// <summary>
        /// 2つの方向間の角度差を計算
        /// </summary>
        /// <param name="from">開始方向</param>
        /// <param name="to">終了方向</param>
        /// <returns>角度差（度）</returns>
        public static double GetAngleDifference(MouseDirection from, MouseDirection to)
        {
            var fromAngle = GetCenterAngle(from);
            var toAngle = GetCenterAngle(to);
            
            var diff = Math.Abs(toAngle - fromAngle);
            
            // 180度を超える場合は、短い方向の角度を返す
            if (diff > 180)
                diff = 360 - diff;
                
            return diff;
        }

        /// <summary>
        /// 隣接する方向のリストを取得（32方向対応）
        /// </summary>
        /// <param name="direction">基準方向</param>
        /// <returns>隣接する方向のリスト</returns>
        public static List<MouseDirection> GetAdjacentDirections(MouseDirection direction)
        {
            var directions = new List<MouseDirection>();
            var currentIndex = (int)direction;
            
            // 前の方向
            var prevIndex = (currentIndex - 1 + 32) % 32;
            directions.Add((MouseDirection)prevIndex);
            
            // 次の方向  
            var nextIndex = (currentIndex + 1) % 32;
            directions.Add((MouseDirection)nextIndex);
            
            return directions;
        }
    }
}