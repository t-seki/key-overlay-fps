using KeyOverlayFPS.Colors;
using NUnit.Framework;

namespace KeyOverlayFPS.Tests.Colors
{
    [TestFixture]
    public class SimpleColorManagerTests
    {
        [Test]
        public void FindBackgroundIndex_TransparentName_MatchesTransparentOption()
        {
            Assert.That(SimpleColorManager.FindBackgroundIndex("Transparent"), Is.EqualTo(0));
        }

        [Test]
        public void FindBackgroundIndex_SavedArgbString_Matches()
        {
            var blue = System.Windows.Media.Colors.Blue.ToString();
            Assert.That(SimpleColorManager.FindBackgroundIndex(blue), Is.EqualTo(2));
        }

        [Test]
        public void FindBackgroundIndex_TransparentArgbString_DoesNotMatch()
        {
            Assert.That(SimpleColorManager.FindBackgroundIndex("#00FFFFFF"), Is.EqualTo(-1));
        }

        [Test]
        public void FindForegroundIndex_ColorName_Matches()
        {
            Assert.That(SimpleColorManager.FindForegroundIndex("White"), Is.EqualTo(0));
        }

        [Test]
        public void FindForegroundIndex_SavedArgbString_Matches()
        {
            var red = System.Windows.Media.Colors.Red.ToString();
            Assert.That(SimpleColorManager.FindForegroundIndex(red), Is.EqualTo(5));
        }

        [Test]
        public void FindHighlightIndex_DefaultValue_MatchesGreen()
        {
            Assert.That(SimpleColorManager.FindHighlightIndex("#B400FF00"), Is.EqualTo(0));
        }

        [Test]
        public void FindHighlightIndex_EverySavedOption_MatchesItself()
        {
            for (int i = 0; i < SimpleColorManager.HighlightMenuOptions.Length; i++)
            {
                var saved = SimpleColorManager.HighlightMenuOptions[i].Color.ToString();
                Assert.That(SimpleColorManager.FindHighlightIndex(saved), Is.EqualTo(i));
            }
        }

        [TestCase("#123456")]
        [TestCase("not a color")]
        [TestCase("")]
        [TestCase(null)]
        public void FindIndex_UnknownOrUnreadable_ReturnsMinusOne(string? setting)
        {
            Assert.Multiple(() =>
            {
                Assert.That(SimpleColorManager.FindBackgroundIndex(setting), Is.EqualTo(-1));
                Assert.That(SimpleColorManager.FindForegroundIndex(setting), Is.EqualTo(-1));
                Assert.That(SimpleColorManager.FindHighlightIndex(setting), Is.EqualTo(-1));
            });
        }
    }
}
