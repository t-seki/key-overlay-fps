using System;
using System.Threading;
using NUnit.Framework;
using KeyOverlayFPS.Input;

namespace KeyOverlayFPS.Tests.Input
{
    /// <summary>
    /// HookThreadのテストクラス
    /// </summary>
    /// <remarks>
    /// 実際のフックは張らず、スレッドの起動・メッセージループ・停止だけを確かめる
    /// </remarks>
    [TestFixture]
    public class HookThreadTests
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        private HookThread _hookThread = null!;

        [SetUp]
        public void SetUp()
        {
            _hookThread = new HookThread("HookThreadTests");
        }

        [TearDown]
        public void TearDown()
        {
            _hookThread.Dispose();
        }

        [Test]
        public void Start_InstallRunsOnDedicatedBackgroundThread()
        {
            int callerThreadId = Environment.CurrentManagedThreadId;
            int installThreadId = -1;
            bool installOnBackground = false;

            bool started = _hookThread.Start(
                () =>
                {
                    installThreadId = Environment.CurrentManagedThreadId;
                    installOnBackground = Thread.CurrentThread.IsBackground;
                    return true;
                },
                () => { },
                WaitTimeout);

            Assert.That(started, Is.True);
            Assert.That(_hookThread.IsRunning, Is.True);
            Assert.That(installThreadId, Is.Not.EqualTo(callerThreadId));
            Assert.That(installOnBackground, Is.True);
        }

        [Test]
        public void Stop_UninstallRunsOnSameThreadAndThreadEnds()
        {
            int installThreadId = -1;
            int uninstallThreadId = -2;
            int uninstallCount = 0;

            bool started = _hookThread.Start(
                () =>
                {
                    installThreadId = Environment.CurrentManagedThreadId;
                    return true;
                },
                () =>
                {
                    uninstallThreadId = Environment.CurrentManagedThreadId;
                    Interlocked.Increment(ref uninstallCount);
                },
                WaitTimeout);

            bool stopped = _hookThread.Stop(WaitTimeout);

            Assert.That(started, Is.True);
            Assert.That(stopped, Is.True);
            Assert.That(_hookThread.IsRunning, Is.False);
            Assert.That(uninstallCount, Is.EqualTo(1));
            Assert.That(uninstallThreadId, Is.EqualTo(installThreadId));
        }

        [Test]
        public void Start_WhenInstallFails_ReturnsFalseAndUninstalls()
        {
            int uninstallCount = 0;

            bool started = _hookThread.Start(
                () => false,
                () => Interlocked.Increment(ref uninstallCount),
                WaitTimeout);

            Assert.That(started, Is.False);
            Assert.That(_hookThread.IsRunning, Is.False);
            Assert.That(uninstallCount, Is.EqualTo(1));
        }

        [Test]
        public void Start_WhenInstallThrows_ReturnsFalseAndUninstalls()
        {
            int uninstallCount = 0;

            bool started = _hookThread.Start(
                () => throw new InvalidOperationException("テスト用の例外"),
                () => Interlocked.Increment(ref uninstallCount),
                WaitTimeout);

            Assert.That(started, Is.False);
            Assert.That(_hookThread.IsRunning, Is.False);
            Assert.That(uninstallCount, Is.EqualTo(1));
        }

        [Test]
        public void Stop_CalledTwiceOrWithoutStart_DoesNotThrow()
        {
            Assert.That(_hookThread.Stop(WaitTimeout), Is.True);

            _hookThread.Start(() => true, () => { }, WaitTimeout);

            Assert.That(_hookThread.Stop(WaitTimeout), Is.True);
            Assert.That(_hookThread.Stop(WaitTimeout), Is.True);
        }

        [Test]
        public void Start_AfterStop_StartsNewThread()
        {
            int firstThreadId = -1;
            int secondThreadId = -1;

            bool firstStarted = _hookThread.Start(
                () =>
                {
                    firstThreadId = Environment.CurrentManagedThreadId;
                    return true;
                },
                () => { },
                WaitTimeout);
            _hookThread.Stop(WaitTimeout);

            bool secondStarted = _hookThread.Start(
                () =>
                {
                    secondThreadId = Environment.CurrentManagedThreadId;
                    return true;
                },
                () => { },
                WaitTimeout);

            Assert.That(firstStarted, Is.True);
            Assert.That(secondStarted, Is.True);
            Assert.That(_hookThread.IsRunning, Is.True);
            Assert.That(firstThreadId, Is.Not.EqualTo(-1));
            Assert.That(secondThreadId, Is.Not.EqualTo(-1));
        }
    }
}
