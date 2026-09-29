using NUnit.Framework;
using KeyOverlayFPS.MouseVisualization;
using System;

namespace KeyOverlayFPS.Tests
{
    [TestFixture]
    public class DirectionCalculatorTests
    {
        [Test]
        public void CalculateDirection_East_ReturnsEast()
        {
            var result = DirectionCalculator.CalculateDirection(10, 0);
            Assert.That(result, Is.EqualTo(MouseDirection.East));
        }

        [Test]
        public void CalculateDirection_West_ReturnsWest()
        {
            var result = DirectionCalculator.CalculateDirection(-10, 0);
            Assert.That(result, Is.EqualTo(MouseDirection.West));
        }

        [Test]
        public void CalculateDirection_North_ReturnsNorth()
        {
            var result = DirectionCalculator.CalculateDirection(0, -10);
            Assert.That(result, Is.EqualTo(MouseDirection.North));
        }

        [Test]
        public void CalculateDirection_South_ReturnsSouth()
        {
            var result = DirectionCalculator.CalculateDirection(0, 10);
            Assert.That(result, Is.EqualTo(MouseDirection.South));
        }

        [Test]
        public void CalculateDirection_NorthEast_ReturnsNorthEast()
        {
            var result = DirectionCalculator.CalculateDirection(10, -10);
            Assert.That(result, Is.EqualTo(MouseDirection.NorthEast));
        }

        [Test]
        public void CalculateDirection_SouthWest_ReturnsSouthWest()
        {
            var result = DirectionCalculator.CalculateDirection(-10, 10);
            Assert.That(result, Is.EqualTo(MouseDirection.SouthWest));
        }

        [Test]
        public void CalculateDirection_ZeroMovement_ReturnsEast()
        {
            var result = DirectionCalculator.CalculateDirection(0, 0);
            Assert.That(result, Is.EqualTo(MouseDirection.East));
        }

        [Test]
        public void CalculateDirection_VerySmallMovement_ReturnsEast()
        {
            var result = DirectionCalculator.CalculateDirection(0.0001, 0.0001);
            Assert.That(result, Is.EqualTo(MouseDirection.East));
        }

        [TestCase(22.5, MouseDirection.EastNorthEast)]
        [TestCase(45, MouseDirection.NorthEast)]
        [TestCase(67.5, MouseDirection.NorthNorthEast)]
        [TestCase(90, MouseDirection.North)]
        [TestCase(112.5, MouseDirection.NorthNorthWest)]
        [TestCase(135, MouseDirection.NorthWest)]
        [TestCase(157.5, MouseDirection.WestNorthWest)]
        [TestCase(180, MouseDirection.West)]
        [TestCase(202.5, MouseDirection.WestSouthWest)]
        [TestCase(225, MouseDirection.SouthWest)]
        [TestCase(247.5, MouseDirection.SouthSouthWest)]
        [TestCase(270, MouseDirection.South)]
        [TestCase(292.5, MouseDirection.SouthSouthEast)]
        [TestCase(315, MouseDirection.SouthEast)]
        [TestCase(337.5, MouseDirection.EastSouthEast)]
        [TestCase(360, MouseDirection.East)] // 360度は0度と同じ
        public void GetDirectionFromAngle_ExactAngles_ReturnsCorrectDirection(double angle, MouseDirection expected)
        {
            var result = DirectionCalculator.GetDirectionFromAngle(angle);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(11, MouseDirection.East)] // 11°付近はEast (-11.25° ~ +11.25°)
        [TestCase(12, MouseDirection.EastNorthEast)] // 12°付近はEastNorthEast
        [TestCase(349, MouseDirection.East)] // 349°付近はEast
        [TestCase(348, MouseDirection.EastSouthEast)] // 348°付近はEastSouthEast
        public void GetDirectionFromAngle_BorderAngles_ReturnsCorrectDirection(double angle, MouseDirection expected)
        {
            var result = DirectionCalculator.GetDirectionFromAngle(angle);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(-90, MouseDirection.South)] // 負の角度
        [TestCase(-45, MouseDirection.SouthEast)]
        [TestCase(450, MouseDirection.North)] // 360度を超える角度
        public void GetDirectionFromAngle_NormalizedAngles_ReturnsCorrectDirection(double angle, MouseDirection expected)
        {
            var result = DirectionCalculator.GetDirectionFromAngle(angle);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetCenterAngle_AllDirections_ReturnsCorrectAngles()
        {
            Assert.That(DirectionCalculator.GetCenterAngle(MouseDirection.East), Is.EqualTo(0));
            Assert.That(DirectionCalculator.GetCenterAngle(MouseDirection.EastNorthEast), Is.EqualTo(22.5));
            Assert.That(DirectionCalculator.GetCenterAngle(MouseDirection.NorthEast), Is.EqualTo(45));
            Assert.That(DirectionCalculator.GetCenterAngle(MouseDirection.North), Is.EqualTo(90));
            Assert.That(DirectionCalculator.GetCenterAngle(MouseDirection.West), Is.EqualTo(180));
            Assert.That(DirectionCalculator.GetCenterAngle(MouseDirection.South), Is.EqualTo(270));
            Assert.That(DirectionCalculator.GetCenterAngle(MouseDirection.EastSouthEast), Is.EqualTo(337.5));
        }

    }
}