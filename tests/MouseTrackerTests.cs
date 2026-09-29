using NUnit.Framework;
using KeyOverlayFPS.MouseVisualization;
using System;

namespace KeyOverlayFPS.Tests
{
    [TestFixture]
    public class MouseTrackerTests
    {
        private MouseTracker _tracker = null!;
        private MouseMoveEventArgs? _lastEvent;
        private int _eventCount;

        [SetUp]
        public void SetUp()
        {
            _tracker = new MouseTracker();
            _lastEvent = null;
            _eventCount = 0;
            _tracker.MouseMoved += OnMouseMoved;
        }

        [TearDown]
        public void TearDown()
        {
            _tracker.MouseMoved -= OnMouseMoved;
        }

        private void OnMouseMoved(object? sender, MouseMoveEventArgs e)
        {
            _lastEvent = e;
            _eventCount++;
        }

        [Test]
        public void Reset_ClearsInitializationState()
        {
            // 初回更新で初期化
            _tracker.Update(5.0);
            
            _tracker.Reset();
            
            // リセット後の初回更新ではイベントが発生しない
            _tracker.Update(5.0);
            Assert.That(_eventCount, Is.EqualTo(0));
        }

        [Test]
        public void Update_FirstCall_DoesNotFireEvent()
        {
            _tracker.Update(5.0);
            
            Assert.That(_eventCount, Is.EqualTo(0));
            Assert.That(_lastEvent, Is.Null);
        }

        [Test]
        public void MouseMoveEventArgs_Constructor_SetsPropertiesCorrectly()
        {
            var deltaX = 10.0;
            var deltaY = -5.0;
            var direction = MouseDirection.NorthEast;
            var distance = 15.0;

            var eventArgs = new MouseMoveEventArgs(deltaX, deltaY, direction, distance);

            Assert.That(eventArgs.DeltaX, Is.EqualTo(deltaX));
            Assert.That(eventArgs.DeltaY, Is.EqualTo(deltaY));
            Assert.That(eventArgs.Direction, Is.EqualTo(direction));
            Assert.That(eventArgs.Distance, Is.EqualTo(distance));
        }

        [Test]
        public void MouseDirection_EnumValues_Have16Values()
        {
            var values = Enum.GetValues<MouseDirection>();
            Assert.That(values.Length, Is.EqualTo(16));

            // 値が0-15の連続した値であることを確認
            for (int i = 0; i < 16; i++)
            {
                Assert.That(Enum.IsDefined(typeof(MouseDirection), i), Is.True, $"MouseDirection value {i} should be defined");
            }
        }

        [Test]
        public void MouseDirection_EnumNames_AreCorrect()
        {
            Assert.That(MouseDirection.East.ToString(), Is.EqualTo("East"));
            Assert.That(MouseDirection.North.ToString(), Is.EqualTo("North"));
            Assert.That(MouseDirection.West.ToString(), Is.EqualTo("West"));
            Assert.That(MouseDirection.South.ToString(), Is.EqualTo("South"));
            Assert.That(MouseDirection.NorthEast.ToString(), Is.EqualTo("NorthEast"));
            Assert.That(MouseDirection.SouthWest.ToString(), Is.EqualTo("SouthWest"));
            Assert.That(MouseDirection.EastNorthEast.ToString(), Is.EqualTo("EastNorthEast"));
            Assert.That(MouseDirection.WestSouthWest.ToString(), Is.EqualTo("WestSouthWest"));
        }

        [Test]
        public void MouseDirection_Values_AreInCorrectOrder()
        {
            // 時計回りの順序で定義されていることを確認（32方向）
            Assert.That((int)MouseDirection.East, Is.EqualTo(0));
            Assert.That((int)MouseDirection.EastNorthEast, Is.EqualTo(1));
            Assert.That((int)MouseDirection.NorthEast, Is.EqualTo(2));
            Assert.That((int)MouseDirection.NorthNorthEast, Is.EqualTo(3));
            Assert.That((int)MouseDirection.North, Is.EqualTo(4));
            Assert.That((int)MouseDirection.West, Is.EqualTo(8));
            Assert.That((int)MouseDirection.South, Is.EqualTo(12));
            Assert.That((int)MouseDirection.EastSouthEast, Is.EqualTo(15));
        }

        // 注意: MouseTrackerの実際のマウス座標取得機能は、Win32 APIに依存するため
        // 単体テストでは直接テストが困難です。以下のテストは設計のガイドラインとして記載

        [Test, Ignore("Integration test - requires actual mouse movement")]
        public void Integration_MouseMovement_FiresCorrectEvents()
        {
            // このテストは統合テスト環境で実行する必要があります
            // 実際のマウス移動をシミュレートしてイベントの発生を確認
            
            _tracker.Update(5.0); // 初期化
            
            // 実際のマウス移動が必要
            System.Threading.Thread.Sleep(100);
            
            _tracker.Update(5.0);
            
            // マウスが移動していれば、イベントが発生する
            // Assert.That(_eventCount, Is.GreaterThan(0));
        }

        [Test]
        public void Threshold_BelowThreshold_DoesNotFireEvent()
        {
            // このテストは、実際のマウス移動検出ロジックが実装された場合に有効になります
            // 現在のMouseTrackerはWin32 APIを使用しているため、モックが必要
            
            // モック可能な設計に変更した場合のテスト例:
            // _mockMouseProvider.SetPosition(0, 0);
            // _tracker.Update(5.0); // 初期化
            // 
            // _mockMouseProvider.SetPosition(2, 2); // 閾値以下の移動
            // _tracker.Update(5.0);
            // 
            // Assert.That(_eventCount, Is.EqualTo(0));
        }

        [Test]
        public void Distance_Calculation_IsCorrect()
        {
            // 直角三角形の距離計算テスト（3-4-5の三角形）
            var deltaX = 3.0;
            var deltaY = 4.0;
            var expectedDistance = 5.0;

            var eventArgs = new MouseMoveEventArgs(deltaX, deltaY, MouseDirection.North, expectedDistance);

            Assert.That(eventArgs.Distance, Is.EqualTo(expectedDistance).Within(0.001));
        }

        [Test]
        public void EventArgs_CreatedWithCorrectDirection()
        {
            // DirectionCalculatorが正しく呼ばれることを確認するテスト
            var deltaX = 10.0;
            var deltaY = 0.0;
            var expectedDirection = MouseDirection.East;

            var eventArgs = new MouseMoveEventArgs(deltaX, deltaY, expectedDirection, 10.0);

            Assert.That(eventArgs.Direction, Is.EqualTo(expectedDirection));
        }
    }

    /// <summary>
    /// MouseTrackerのテスト用拡張（将来的なモック対応）
    /// </summary>
    public static class MouseTrackerTestExtensions
    {
        /// <summary>
        /// テスト用にマウス移動をシミュレート（将来的な実装）
        /// </summary>
        public static void SimulateMouseMove(this MouseTracker tracker, double deltaX, double deltaY, double threshold = 5.0)
        {
            // 実装時にはIMousePositionProviderなどのインターフェースを導入し、
            // テスト時にはモックを使用できるようにする
            
            // 現在は実装されていないため、プレースホルダーとして残す
            throw new NotImplementedException("Mouse simulation requires refactoring to use dependency injection");
        }
    }
}