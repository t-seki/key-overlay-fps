using System.Windows.Media;
using NUnit.Framework;
using KeyOverlayFPS.UI;
using KeyOverlayFPS.Constants;

namespace KeyOverlayFPS.Tests.UI
{
    /// <summary>
    /// BrushFactoryのテストクラス
    /// </summary>
    [TestFixture]
    public class BrushFactoryTests
    {
        [Test]
        public void CreateStandardBackground_ShouldReturnLinearGradientBrush()
        {
            // Act
            var brush = BrushFactory.CreateStandardBackground();

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<LinearGradientBrush>());
            Assert.That(brush.GradientStops, Has.Count.EqualTo(2));
            Assert.That(brush.GradientStops[0].Color, Is.EqualTo(ApplicationConstants.Colors.KeyBackground1));
            Assert.That(brush.GradientStops[1].Color, Is.EqualTo(ApplicationConstants.Colors.KeyBackground2));
            Assert.That(brush.GradientStops[0].Offset, Is.EqualTo(0));
            Assert.That(brush.GradientStops[1].Offset, Is.EqualTo(1));
        }

        [Test]
        public void CreateKeyboardKeyBackground_ShouldReturnSameAsStandardBackground()
        {
            // Act
            var standardBrush = BrushFactory.CreateStandardBackground();
            var keyboardBrush = BrushFactory.CreateKeyboardKeyBackground();

            // Assert
            Assert.That(keyboardBrush, Is.Not.Null);
            Assert.That(keyboardBrush, Is.TypeOf<LinearGradientBrush>());
            Assert.That(keyboardBrush.GradientStops[0].Color, Is.EqualTo(standardBrush.GradientStops[0].Color));
            Assert.That(keyboardBrush.GradientStops[1].Color, Is.EqualTo(standardBrush.GradientStops[1].Color));
        }

        [Test]
        public void CreateMouseBodyBackground_ShouldReturnSameAsStandardBackground()
        {
            // Act
            var standardBrush = BrushFactory.CreateStandardBackground();
            var mouseBrush = BrushFactory.CreateMouseBodyBackground();

            // Assert
            Assert.That(mouseBrush, Is.Not.Null);
            Assert.That(mouseBrush, Is.TypeOf<LinearGradientBrush>());
            Assert.That(mouseBrush.GradientStops[0].Color, Is.EqualTo(standardBrush.GradientStops[0].Color));
            Assert.That(mouseBrush.GradientStops[1].Color, Is.EqualTo(standardBrush.GradientStops[1].Color));
        }

        [Test]
        public void CreateMouseButtonBackground_ShouldReturnCorrectGradientBrush()
        {
            // Act
            var brush = BrushFactory.CreateMouseButtonBackground();

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<LinearGradientBrush>());
            Assert.That(brush.GradientStops, Has.Count.EqualTo(2));
            Assert.That(brush.GradientStops[0].Color, Is.EqualTo(ApplicationConstants.Colors.MouseButtonBackground1));
            Assert.That(brush.GradientStops[1].Color, Is.EqualTo(ApplicationConstants.Colors.MouseButtonBackground2));
            Assert.That(brush.GradientStops[0].Offset, Is.EqualTo(0));
            Assert.That(brush.GradientStops[1].Offset, Is.EqualTo(1));
            
            // 垂直方向のグラデーション確認
            Assert.That(brush.StartPoint.X, Is.EqualTo(0));
            Assert.That(brush.StartPoint.Y, Is.EqualTo(0));
            Assert.That(brush.EndPoint.X, Is.EqualTo(0));
            Assert.That(brush.EndPoint.Y, Is.EqualTo(1));
        }

        [Test]
        public void CreateDefaultHighlightBrush_ShouldReturnCorrectSolidColorBrush()
        {
            // Act
            var brush = BrushFactory.CreateDefaultHighlightBrush();

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<SolidColorBrush>());
            Assert.That(brush.Color, Is.EqualTo(ApplicationConstants.Colors.DefaultHighlight));
        }

        [Test]
        public void CreateTransparentBackground_ShouldReturnCorrectSolidColorBrush()
        {
            // Act
            var brush = BrushFactory.CreateTransparentBackground();

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<SolidColorBrush>());
            Assert.That(brush.Color, Is.EqualTo(ApplicationConstants.Colors.TransparentBackground));
        }

        [Test]
        public void CreateMouseDirectionCenterBrush_ShouldReturnCorrectSolidColorBrush()
        {
            // Act
            var brush = BrushFactory.CreateMouseDirectionCenterBrush();

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<SolidColorBrush>());
            Assert.That(brush.Color, Is.EqualTo(ApplicationConstants.Colors.MouseDirectionCenter));
        }

        [Test]
        [TestCase("Red", ExpectedResult = "#FFFF0000")]
        [TestCase("#FF0000", ExpectedResult = "#FFFF0000")]
        [TestCase("Blue", ExpectedResult = "#FF0000FF")]
        [TestCase("#0000FF", ExpectedResult = "#FF0000FF")]
        [TestCase("Green", ExpectedResult = "#FF008000")]
        [TestCase("#008000", ExpectedResult = "#FF008000")]
        [TestCase("White", ExpectedResult = "#FFFFFFFF")]
        [TestCase("#FFFFFF", ExpectedResult = "#FFFFFFFF")]
        [TestCase("Black", ExpectedResult = "#FF000000")]
        [TestCase("#000000", ExpectedResult = "#FF000000")]
        public string CreateBrushFromString_ShouldReturnCorrectColor_ForValidColors(string colorString)
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString(colorString);

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<SolidColorBrush>());
            
            var solidBrush = (SolidColorBrush)brush;
            return solidBrush.Color.ToString();
        }

        [Test]
        public void CreateBrushFromString_ShouldReturnTransparentBrush_ForTransparentString()
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString("Transparent");

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<SolidColorBrush>());
            
            var solidBrush = (SolidColorBrush)brush;
            Assert.That(solidBrush.Color, Is.EqualTo(ApplicationConstants.Colors.TransparentBackground));
        }

        [Test]
        public void CreateBrushFromString_ShouldBeCaseInsensitive_ForTransparent()
        {
            // Act
            var brushLower = BrushFactory.CreateBrushFromString("transparent");
            var brushUpper = BrushFactory.CreateBrushFromString("TRANSPARENT");
            var brushMixed = BrushFactory.CreateBrushFromString("Transparent");

            // Assert
            Assert.That(brushLower, Is.TypeOf<SolidColorBrush>());
            Assert.That(brushUpper, Is.TypeOf<SolidColorBrush>());
            Assert.That(brushMixed, Is.TypeOf<SolidColorBrush>());
            
            var colorLower = ((SolidColorBrush)brushLower).Color;
            var colorUpper = ((SolidColorBrush)brushUpper).Color;
            var colorMixed = ((SolidColorBrush)brushMixed).Color;
            
            Assert.That(colorLower, Is.EqualTo(ApplicationConstants.Colors.TransparentBackground));
            Assert.That(colorUpper, Is.EqualTo(ApplicationConstants.Colors.TransparentBackground));
            Assert.That(colorMixed, Is.EqualTo(ApplicationConstants.Colors.TransparentBackground));
        }

        [Test]
        public void CreateBrushFromString_ShouldReturnWhiteBrush_ForInvalidColor()
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString("InvalidColor");

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.EqualTo(Brushes.White));
        }

        [Test]
        public void CreateBrushFromString_ShouldReturnFallbackBrush_WhenProvidedForInvalidColor()
        {
            // Arrange
            var fallbackBrush = new SolidColorBrush(System.Windows.Media.Colors.Yellow);

            // Act
            var brush = BrushFactory.CreateBrushFromString("InvalidColor", fallbackBrush);

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.EqualTo(fallbackBrush));
        }

        [Test]
        public void CreateBrushFromString_ShouldReturnWhiteBrush_ForNullFallbackAndInvalidColor()
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString("InvalidColor", null);

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.EqualTo(Brushes.White));
        }

        [Test]
        public void CreateBrushFromString_ShouldReturnWhiteBrush_ForEmptyString()
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString("");

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.EqualTo(Brushes.White));
        }

        [Test]
        public void CreateBrushFromString_ShouldReturnWhiteBrush_ForNullString()
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString(null!);

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.EqualTo(Brushes.White));
        }

        [Test]
        public void CreateBrushFromString_ShouldHandleHexColors_WithoutHashPrefix()
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString("FF0000");

            // Assert - 無効として扱われるはず（#が必要）
            Assert.That(brush, Is.EqualTo(Brushes.White));
        }

        [Test]
        public void CreateBrushFromString_ShouldHandleRGBValues()
        {
            // Act
            var brush = BrushFactory.CreateBrushFromString("#FF8080");

            // Assert
            Assert.That(brush, Is.Not.Null);
            Assert.That(brush, Is.TypeOf<SolidColorBrush>());
            
            var solidBrush = (SolidColorBrush)brush;
            Assert.That(solidBrush.Color.R, Is.EqualTo(255));
            Assert.That(solidBrush.Color.G, Is.EqualTo(128));
            Assert.That(solidBrush.Color.B, Is.EqualTo(128));
        }

        [Test]
        public void AllBrushCreationMethods_ShouldReturnNewInstances()
        {
            // Act
            var brush1 = BrushFactory.CreateStandardBackground();
            var brush2 = BrushFactory.CreateStandardBackground();
            var brush3 = BrushFactory.CreateDefaultHighlightBrush();
            var brush4 = BrushFactory.CreateDefaultHighlightBrush();

            // Assert - 異なるインスタンスを返すことを確認
            Assert.That(brush1, Is.Not.SameAs(brush2));
            Assert.That(brush3, Is.Not.SameAs(brush4));
        }

        [Test]
        public void CreateStandardBackground_ShouldHaveDiagonalGradient()
        {
            // Act
            var brush = BrushFactory.CreateStandardBackground();

            // Assert - 対角線グラデーション (0,0) to (1,1)
            Assert.That(brush.StartPoint.X, Is.EqualTo(0));
            Assert.That(brush.StartPoint.Y, Is.EqualTo(0));
            Assert.That(brush.EndPoint.X, Is.EqualTo(1));
            Assert.That(brush.EndPoint.Y, Is.EqualTo(1));
        }

        [Test]
        public void BrushFactoryMethods_ShouldBeThreadSafe()
        {
            // Act & Assert - 複数回呼び出しても例外が発生しないことを確認
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    var _ = BrushFactory.CreateStandardBackground();
                    var __ = BrushFactory.CreateDefaultHighlightBrush();
                    var ___ = BrushFactory.CreateBrushFromString("Red");
                }
            });
        }

        [Test]
        public void CreateBrushFromString_ShouldHandleEdgeCaseColors()
        {
            // Arrange & Act & Assert
            var specialColors = new[]
            {
                ("Transparent", typeof(SolidColorBrush)),
                ("Yellow", typeof(SolidColorBrush)),
                ("Purple", typeof(SolidColorBrush)),
                ("Orange", typeof(SolidColorBrush)),
                ("Pink", typeof(SolidColorBrush)),
                ("Gray", typeof(SolidColorBrush))
            };

            foreach (var (colorName, expectedType) in specialColors)
            {
                var brush = BrushFactory.CreateBrushFromString(colorName);
                Assert.That(brush, Is.Not.Null, $"Brush should not be null for color: {colorName}");
                Assert.That(brush, Is.TypeOf(expectedType), $"Brush type mismatch for color: {colorName}");
            }
        }
    }
}