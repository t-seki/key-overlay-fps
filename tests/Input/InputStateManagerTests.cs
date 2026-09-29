using System.Collections.Generic;
using System.Reflection;
using KeyOverlayFPS.Input;
using NUnit.Framework;

namespace KeyOverlayFPS.Tests.Input
{
    /// <summary>
    /// InputStateManager の照合ロジックのテストクラス
    /// </summary>
    /// <remarks>
    /// フックは張らない。押下イベントは private ハンドラーを直接呼んで再現する
    /// </remarks>
    [TestFixture]
    public class InputStateManagerTests
    {
        private const int KeyA = 0x41;

        private HashSet<int> _physicallyDown = null!;
        private long _now;
        private InputStateManager _manager = null!;

        [SetUp]
        public void SetUp()
        {
            _physicallyDown = new HashSet<int>();
            _now = 1000;
            _manager = new InputStateManager(vk => _physicallyDown.Contains(vk), () => _now);
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
        }

        private void HookKeyDown(int vk)
        {
            typeof(InputStateManager)
                .GetMethod("OnKeyPressed", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(_manager, new object?[] { null, new KeyboardEventArgs(vk, true, false) });
        }

        private void HookMouseDown(int vk)
        {
            typeof(InputStateManager)
                .GetMethod("OnMouseButtonPressed", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(_manager, new object?[] { null, new MouseButtonHookEventArgs(vk) });
        }

        [Test]
        public void Constructor_DoesNotStartHooks()
        {
            Assert.That(_manager.IsEnabled, Is.False);
        }

        [Test]
        public void Reconcile_KeyReallyDown_IsNeverCleared()
        {
            HookKeyDown(KeyA);
            _physicallyDown.Add(KeyA);

            _manager.ReconcileKeyStates();
            _now += InputStateManager.ReconcileReleaseThresholdMs * 10;
            _manager.ReconcileKeyStates();

            Assert.That(_manager.IsKeyPressed(KeyA), Is.True);
        }

        [Test]
        public void Reconcile_TransientMismatch_DoesNotClear()
        {
            HookKeyDown(KeyA);

            _manager.ReconcileKeyStates();
            _now += InputStateManager.ReconcileReleaseThresholdMs - 1;
            _manager.ReconcileKeyStates();

            Assert.That(_manager.IsKeyPressed(KeyA), Is.True);
        }

        [Test]
        public void Reconcile_MismatchOverThreshold_ClearsAndRaisesEvent()
        {
            HookKeyDown(KeyA);
            KeyStateChangedEventArgs? raised = null;
            _manager.KeyStateChanged += (s, e) => raised = e;

            _manager.ReconcileKeyStates();
            _now += InputStateManager.ReconcileReleaseThresholdMs;
            _manager.ReconcileKeyStates();

            Assert.That(_manager.IsKeyPressed(KeyA), Is.False);
            Assert.That(raised, Is.Not.Null);
            Assert.That(raised!.VirtualKeyCode, Is.EqualTo(KeyA));
            Assert.That(raised.IsPressed, Is.False);
        }

        [Test]
        public void Reconcile_MatchingInBetween_ResetsMeasurement()
        {
            HookKeyDown(KeyA);

            _manager.ReconcileKeyStates();
            _now += InputStateManager.ReconcileReleaseThresholdMs - 10;
            _physicallyDown.Add(KeyA);
            _manager.ReconcileKeyStates();
            _physicallyDown.Remove(KeyA);
            _now += 50;
            _manager.ReconcileKeyStates();

            Assert.That(_manager.IsKeyPressed(KeyA), Is.True);
        }

        [Test]
        public void KeyPressEvent_ResetsMeasurement()
        {
            HookKeyDown(KeyA);

            _manager.ReconcileKeyStates();
            _now += InputStateManager.ReconcileReleaseThresholdMs - 10;
            _manager.ReconcileKeyStates();

            HookKeyDown(KeyA); // キーリピートなど、押下イベントが来る
            _now += 50;
            _manager.ReconcileKeyStates();

            Assert.That(_manager.IsKeyPressed(KeyA), Is.True);
        }

        [Test]
        public void Reconcile_MouseButton_IsCleared()
        {
            HookMouseDown(VirtualKeyCodes.VK_LBUTTON);

            _manager.ReconcileKeyStates();
            _now += InputStateManager.ReconcileReleaseThresholdMs;
            _manager.ReconcileKeyStates();

            Assert.That(_manager.IsKeyPressed(VirtualKeyCodes.VK_LBUTTON), Is.False);
        }

        [TestCase(true, VirtualKeyCodes.VK_LBUTTON, VirtualKeyCodes.VK_RBUTTON, true)]
        [TestCase(true, VirtualKeyCodes.VK_RBUTTON, VirtualKeyCodes.VK_LBUTTON, true)]
        [TestCase(false, VirtualKeyCodes.VK_LBUTTON, VirtualKeyCodes.VK_RBUTTON, false)]
        [TestCase(true, VirtualKeyCodes.VK_MBUTTON, VirtualKeyCodes.VK_LBUTTON, false)]
        [TestCase(true, KeyA, VirtualKeyCodes.VK_LBUTTON, false)]
        public void IsKeyDownConsideringSwap_TreatsLeftRightAsEitherWhenSwapped(
            bool swapped, int queried, int physicallyDown, bool expected)
        {
            bool result = InputStateManager.IsKeyDownConsideringSwap(queried, swapped, vk => vk == physicallyDown);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
