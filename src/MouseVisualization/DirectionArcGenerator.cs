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
    /// 16方向のマウス移動可視化で使用される円弧Pathを生成する
    /// </summary>
    public static class DirectionArcGenerator
    {
        /// <summary>
        /// 指定された方向とセグメントインデックスから円弧Pathを作成
        /// </summary>
        /// <param name="direction">マウス移動方向</param>
        /// <param name="segmentIndex">セグメント番号（0-15）</param>
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
            var startAngle = segmentIndex * anglePerSegment - anglePerSegment / 2;
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
        /// 16方向すべての円弧を生成してCanvasに追加
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
            
            // 16方向の列挙
            var directions = new MouseDirection[]
            {
                MouseDirection.East, MouseDirection.EastNorthEast,
                MouseDirection.NorthEast, MouseDirection.NorthNorthEast,
                MouseDirection.North, MouseDirection.NorthNorthWest,
                MouseDirection.NorthWest, MouseDirection.WestNorthWest,
                MouseDirection.West, MouseDirection.WestSouthWest,
                MouseDirection.SouthWest, MouseDirection.SouthSouthWest,
                MouseDirection.South, MouseDirection.SouthSouthEast,
                MouseDirection.SouthEast, MouseDirection.EastSouthEast
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
            var centerPointSize = ApplicationConstants.MouseVisualization.CenterPointSize;
            var centerPoint = new System.Windows.Shapes.Ellipse
            {
                Name = "MouseDirectionCenterPoint",
                Width = centerPointSize,
                Height = centerPointSize,
                Fill = BrushFactory.CreateMouseDirectionCenterBrush(),
                Opacity = 0.8
            };
            System.Windows.Controls.Canvas.SetLeft(centerPoint, radius - centerPointSize / 2);
            System.Windows.Controls.Canvas.SetTop(centerPoint, radius - centerPointSize / 2);
            canvas.Children.Add(centerPoint);
        }
    }
}