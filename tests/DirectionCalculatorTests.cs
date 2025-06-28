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
            Assert.AreEqual(MouseDirection.East, result);
        }

        [Test]
        public void CalculateDirection_West_ReturnsWest()
        {
            var result = DirectionCalculator.CalculateDirection(-10, 0);
            Assert.AreEqual(MouseDirection.West, result);
        }

        [Test]
        public void CalculateDirection_North_ReturnsNorth()
        {
            var result = DirectionCalculator.CalculateDirection(0, -10);
            Assert.AreEqual(MouseDirection.North, result);
        }

        [Test]
        public void CalculateDirection_South_ReturnsSouth()
        {
            var result = DirectionCalculator.CalculateDirection(0, 10);
            Assert.AreEqual(MouseDirection.South, result);
        }

        [Test]
        public void CalculateDirection_NorthEast_ReturnsNorthEast()
        {
            var result = DirectionCalculator.CalculateDirection(10, -10);
            Assert.AreEqual(MouseDirection.NorthEast, result);
        }

        [Test]
        public void CalculateDirection_SouthWest_ReturnsSouthWest()
        {
            var result = DirectionCalculator.CalculateDirection(-10, 10);
            Assert.AreEqual(MouseDirection.SouthWest, result);
        }

        [Test]
        public void CalculateDirection_ZeroMovement_ReturnsEast()
        {
            var result = DirectionCalculator.CalculateDirection(0, 0);
            Assert.AreEqual(MouseDirection.East, result);
        }

        [Test]
        public void CalculateDirection_VerySmallMovement_ReturnsEast()
        {
            var result = DirectionCalculator.CalculateDirection(0.0001, 0.0001);
            Assert.AreEqual(MouseDirection.East, result);
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
            Assert.AreEqual(expected, result);
        }

        [TestCase(11, MouseDirection.East_11_25)] // 11.25°付近は East_11_25
        [TestCase(350, MouseDirection.East_348_75)] // 348.75°付近は East_348_75
        [TestCase(34, MouseDirection.East_33_75)] // 33.75°付近は East_33_75
        [TestCase(56, MouseDirection.North_56_25)] // 56.25°付近は North_56_25
        public void GetDirectionFromAngle_BorderAngles_ReturnsCorrectDirection(double angle, MouseDirection expected)
        {
            var result = DirectionCalculator.GetDirectionFromAngle(angle);
            Assert.AreEqual(expected, result);
        }

        [TestCase(-90, MouseDirection.South)] // 負の角度
        [TestCase(-45, MouseDirection.SouthEast)]
        [TestCase(450, MouseDirection.North)] // 360度を超える角度
        public void GetDirectionFromAngle_NormalizedAngles_ReturnsCorrectDirection(double angle, MouseDirection expected)
        {
            var result = DirectionCalculator.GetDirectionFromAngle(angle);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetCenterAngle_AllDirections_ReturnsCorrectAngles()
        {
            Assert.AreEqual(0, DirectionCalculator.GetCenterAngle(MouseDirection.East));
            Assert.AreEqual(22.5, DirectionCalculator.GetCenterAngle(MouseDirection.EastNorthEast));
            Assert.AreEqual(45, DirectionCalculator.GetCenterAngle(MouseDirection.NorthEast));
            Assert.AreEqual(90, DirectionCalculator.GetCenterAngle(MouseDirection.North));
            Assert.AreEqual(180, DirectionCalculator.GetCenterAngle(MouseDirection.West));
            Assert.AreEqual(270, DirectionCalculator.GetCenterAngle(MouseDirection.South));
            Assert.AreEqual(337.5, DirectionCalculator.GetCenterAngle(MouseDirection.EastSouthEast));
        }

    }
}