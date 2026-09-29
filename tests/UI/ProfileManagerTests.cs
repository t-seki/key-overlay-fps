using System;
using System.IO;
using NUnit.Framework;
using KeyOverlayFPS.Layout;
using KeyOverlayFPS.Settings;
using KeyOverlayFPS.UI;

namespace KeyOverlayFPS.Tests.UI
{
    /// <summary>
    /// ProfileManagerのテストクラス
    /// </summary>
    [TestFixture]
    public class ProfileManagerTests
    {
        private string _tempDirectory = null!;
        private SettingsManager _settingsManager = null!;
        private ProfileManager _profileManager = null!;

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "KeyOverlayFPS_Tests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempDirectory);
            _settingsManager = new SettingsManager(_tempDirectory);
            _profileManager = new ProfileManager(_settingsManager);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Test]
        public void CurrentProfile_ShouldReturnFullKeyboard65_ByDefault()
        {
            Assert.That(_profileManager.CurrentProfile, Is.EqualTo(KeyboardProfile.FullKeyboard65));
        }

        [Test]
        public void CurrentProfile_ShouldParseProfileNameFromSettings()
        {
            _settingsManager.SetCurrentProfile("FPSKeyboard");

            Assert.That(_profileManager.CurrentProfile, Is.EqualTo(KeyboardProfile.FPSKeyboard));
        }

        [Test]
        [TestCase("")]
        [TestCase("UnknownProfile")]
        public void CurrentProfile_ShouldFallbackToFullKeyboard65_ForInvalidName(string profileName)
        {
            _settingsManager.SetCurrentProfile(profileName);

            Assert.That(_profileManager.CurrentProfile, Is.EqualTo(KeyboardProfile.FullKeyboard65));
        }

        [Test]
        public void SwitchProfile_ShouldSaveProfileToSettings()
        {
            _profileManager.SwitchProfile(KeyboardProfile.FPSKeyboard);

            Assert.That(_settingsManager.Current.CurrentProfile, Is.EqualTo("FPSKeyboard"));
            Assert.That(_profileManager.CurrentProfile, Is.EqualTo(KeyboardProfile.FPSKeyboard));
        }

        [Test]
        public void IsCurrentProfile_ShouldReflectCurrentProfile()
        {
            _profileManager.SwitchProfile(KeyboardProfile.FPSKeyboard);

            Assert.That(_profileManager.IsCurrentProfile(KeyboardProfile.FPSKeyboard), Is.True);
            Assert.That(_profileManager.IsCurrentProfile(KeyboardProfile.FullKeyboard65), Is.False);
        }

        [Test]
        public void GetCurrentProfileName_ShouldReturnEnumName()
        {
            Assert.That(_profileManager.GetCurrentProfileName(), Is.EqualTo("FullKeyboard65"));

            _profileManager.SwitchProfile(KeyboardProfile.FPSKeyboard);

            Assert.That(_profileManager.GetCurrentProfileName(), Is.EqualTo("FPSKeyboard"));
        }
    }
}
