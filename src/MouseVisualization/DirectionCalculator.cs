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
        /// 各方向の角度範囲（度）
        /// </summary>
        private static readonly Dictionary<MouseDirection, (double Start, double End)> DirectionRanges = new()
        {
            { MouseDirection.East, (348.75, 11.25) },           // 0° ± 11.25°
            { MouseDirection.EastNorthEast, (11.25, 33.75) },   // 22.5° ± 11.25°
            { MouseDirection.NorthEast, (33.75, 56.25) },       // 45° ± 11.25°
            { MouseDirection.NorthNorthEast, (56.25, 78.75) },  // 67.5° ± 11.25°
            { MouseDirection.North, (78.75, 101.25) },          // 90° ± 11.25°
            { MouseDirection.NorthNorthWest, (101.25, 123.75) }, // 112.5° ± 11.25°
            { MouseDirection.NorthWest, (123.75, 146.25) },     // 135° ± 11.25°
            { MouseDirection.WestNorthWest, (146.25, 168.75) }, // 157.5° ± 11.25°
            { MouseDirection.West, (168.75, 191.25) },          // 180° ± 11.25°
            { MouseDirection.WestSouthWest, (191.25, 213.75) }, // 202.5° ± 11.25°
            { MouseDirection.SouthWest, (213.75, 236.25) },     // 225° ± 11.25°
            { MouseDirection.SouthSouthWest, (236.25, 258.75) }, // 247.5° ± 11.25°
            { MouseDirection.South, (258.75, 281.25) },         // 270° ± 11.25°
            { MouseDirection.SouthSouthEast, (281.25, 303.75) }, // 292.5° ± 11.25°
            { MouseDirection.SouthEast, (303.75, 326.25) },     // 315° ± 11.25°
            { MouseDirection.EastSouthEast, (326.25, 348.75) }  // 337.5° ± 11.25°
        };

        /// <summary>
        /// 各方向の表示名
        /// </summary>
        private static readonly Dictionary<MouseDirection, string> DirectionNames = new()
        {
            { MouseDirection.East, "東" },
            { MouseDirection.EastNorthEast, "東北東" },
            { MouseDirection.NorthEast, "北東" },
            { MouseDirection.NorthNorthEast, "北北東" },
            { MouseDirection.North, "北" },
            { MouseDirection.NorthNorthWest, "北北西" },
            { MouseDirection.NorthWest, "北西" },
            { MouseDirection.WestNorthWest, "西北西" },
            { MouseDirection.West, "西" },
            { MouseDirection.WestSouthWest, "西南西" },
            { MouseDirection.SouthWest, "南西" },
            { MouseDirection.SouthSouthWest, "南南西" },
            { MouseDirection.South, "南" },
            { MouseDirection.SouthSouthEast, "南南東" },
            { MouseDirection.SouthEast, "南東" },
            { MouseDirection.EastSouthEast, "東南東" }
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
        /// 角度から方向を取得
        /// </summary>
        /// <param name="degrees">角度（度）</param>
        /// <returns>対応する方向</returns>
        public static MouseDirection GetDirectionFromAngle(double degrees)
        {
            // 角度を0-360度の範囲に正規化
            degrees = ((degrees % 360) + 360) % 360;

            foreach (var (direction, (start, end)) in DirectionRanges)
            {
                if (IsAngleInRange(degrees, start, end))
                {
                    return direction;
                }
            }

            // デフォルトとして東を返す
            return MouseDirection.East;
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
        /// 方向から中心角度を取得
        /// </summary>
        /// <param name="direction">方向</param>
        /// <returns>中心角度（度）</returns>
        public static double GetCenterAngle(MouseDirection direction)
        {
            return (int)direction * 22.5;
        }

        /// <summary>
        /// 指定された角度が範囲内にあるかチェック
        /// </summary>
        /// <param name="angle">チェックする角度</param>
        /// <param name="start">範囲の開始角度</param>
        /// <param name="end">範囲の終了角度</param>
        /// <returns>範囲内の場合true</returns>
        private static bool IsAngleInRange(double angle, double start, double end)
        {
            if (start <= end)
            {
                // 通常の範囲（例：30° - 60°）
                return angle >= start && angle <= end;
            }
            else
            {
                // 0度をまたぐ範囲（例：350° - 10°）
                return angle >= start || angle <= end;
            }
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
        /// 隣接する方向のリストを取得
        /// </summary>
        /// <param name="direction">基準方向</param>
        /// <returns>隣接する方向のリスト</returns>
        public static List<MouseDirection> GetAdjacentDirections(MouseDirection direction)
        {
            var directions = new List<MouseDirection>();
            var currentIndex = (int)direction;
            
            // 前の方向
            var prevIndex = (currentIndex - 1 + 16) % 16;
            directions.Add((MouseDirection)prevIndex);
            
            // 次の方向  
            var nextIndex = (currentIndex + 1) % 16;
            directions.Add((MouseDirection)nextIndex);
            
            return directions;
        }
    }
}