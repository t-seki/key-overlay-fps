using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using KeyOverlayFPS.Constants;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.MouseVisualization
{
    /// <summary>
    /// マウス方向表示用の円弧生成クラス
    /// 32方向のマウス移動可視化で使用される円弧Pathを生成する
    /// </summary>
    public static class DirectionArcGenerator
    {
        /// <summary>
        /// 指定された方向とセグメントインデックスから円弧Pathを作成
        /// </summary>
        /// <param name="direction">マウス移動方向</param>
        /// <param name="segmentIndex">セグメント番号（0-31）</param>
        /// <param name="radius">円の半径（デフォルト値使用可能）</param>
        /// <param name="strokeBrush">線のブラシ（デフォルト値使用可能）</param>
        /// <returns>作成された円弧Path</returns>
        public static Path CreateDirectionArc(
            MouseDirection direction, 
            int segmentIndex, 
            double radius = 0, 
            Brush? strokeBrush = null)
        {
            // デフォルト値の設定
            if (radius <= 0)
                radius = ApplicationConstants.MouseVisualization.CircleRadius;
                
            if (strokeBrush == null)
                strokeBrush = BrushFactory.CreateDefaultHighlightBrush();
            
            var anglePerSegment = 360.0 / ApplicationConstants.MouseVisualization.DirectionSegments;
            var startAngle = segmentIndex * anglePerSegment;
            var endAngle = startAngle + anglePerSegment;
            
            // 角度をラジアンに変換（East=0度を3時方向に配置）
            var startRadians = startAngle * Math.PI / 180;
            var endRadians = endAngle * Math.PI / 180;
            
            // 円周上の開始点と終了点を計算
            var centerX = radius;
            var centerY = radius;
            
            var startX = centerX + radius * Math.Cos(startRadians);
            var startY = centerY - radius * Math.Sin(startRadians); // Y軸反転（WPF座標系）
            var endX = centerX + radius * Math.Cos(endRadians);
            var endY = centerY - radius * Math.Sin(endRadians); // Y軸反転（WPF座標系）
            
            // 円弧のPath要素を作成
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(startX, startY)
            };
            
            var arcSegment = new ArcSegment
            {
                Point = new Point(endX, endY),
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            };
            
            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
            
            return new Path
            {
                Name = $"Direction{direction}",
                Data = pathGeometry,
                Stroke = strokeBrush,
                StrokeThickness = ApplicationConstants.MouseVisualization.StrokeThickness,
                Opacity = 0.0 // 初期状態は非表示
            };
        }
        
        /// <summary>
        /// 32方向すべての円弧を生成してCanvasに追加
        /// </summary>
        /// <param name="canvas">追加先のCanvas</param>
        /// <param name="radius">円の半径（デフォルト値使用可能）</param>
        /// <param name="strokeBrush">線のブラシ（デフォルト値使用可能）</param>
        /// <returns>方向とPathの辞書</returns>
        public static System.Collections.Generic.Dictionary<MouseDirection, Path> CreateAllDirectionArcs(
            System.Windows.Controls.Canvas canvas, 
            double radius = 0, 
            Brush? strokeBrush = null)
        {
            var directionIndicators = new System.Collections.Generic.Dictionary<MouseDirection, Path>();
            
            // 32方向の列挙
            var directions = new MouseDirection[]
            {
                MouseDirection.East, MouseDirection.East_11_25, 
                MouseDirection.EastNorthEast, MouseDirection.East_33_75,
                MouseDirection.NorthEast, MouseDirection.North_56_25, 
                MouseDirection.NorthNorthEast, MouseDirection.North_78_75,
                MouseDirection.North, MouseDirection.North_101_25, 
                MouseDirection.NorthNorthWest, MouseDirection.North_123_75,
                MouseDirection.NorthWest, MouseDirection.West_146_25, 
                MouseDirection.WestNorthWest, MouseDirection.West_168_75,
                MouseDirection.West, MouseDirection.West_191_25, 
                MouseDirection.WestSouthWest, MouseDirection.West_213_75,
                MouseDirection.SouthWest, MouseDirection.South_236_25, 
                MouseDirection.SouthSouthWest, MouseDirection.South_258_75,
                MouseDirection.South, MouseDirection.South_281_25, 
                MouseDirection.SouthSouthEast, MouseDirection.South_303_75,
                MouseDirection.SouthEast, MouseDirection.South_326_25, 
                MouseDirection.EastSouthEast, MouseDirection.East_348_75
            };
            
            for (int i = 0; i < directions.Length; i++)
            {
                var direction = directions[i];
                var arc = CreateDirectionArc(direction, i, radius, strokeBrush);
                canvas.Children.Add(arc);
                directionIndicators[direction] = arc;
            }
            
            return directionIndicators;
        }
        
        /// <summary>
        /// 方向表示の基準円と中心点を作成してCanvasに追加
        /// </summary>
        /// <param name="canvas">追加先のCanvas</param>
        /// <param name="radius">円の半径</param>
        public static void CreateBaseCircleAndCenter(System.Windows.Controls.Canvas canvas, double radius = 0)
        {
            if (radius <= 0)
                radius = ApplicationConstants.MouseVisualization.CircleRadius;
                
            var circleSize = radius * 2;
            
            // 基準円を作成
            var baseCircle = new System.Windows.Shapes.Ellipse
            {
                Name = "MouseDirectionBaseCircle",
                Width = circleSize,
                Height = circleSize,
                Stroke = Brushes.White,
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                Opacity = 0.3
            };
            System.Windows.Controls.Canvas.SetLeft(baseCircle, 0);
            System.Windows.Controls.Canvas.SetTop(baseCircle, 0);
            canvas.Children.Add(baseCircle);
            
            // 中心点を作成
            var centerPoint = new System.Windows.Shapes.Ellipse
            {
                Name = "MouseDirectionCenterPoint",
                Width = 1,
                Height = 1,
                Fill = BrushFactory.CreateMouseDirectionCenterBrush(),
                Opacity = 0.8
            };
            System.Windows.Controls.Canvas.SetLeft(centerPoint, radius - 0.5);
            System.Windows.Controls.Canvas.SetTop(centerPoint, radius - 0.5);
            canvas.Children.Add(centerPoint);
        }
    }
}