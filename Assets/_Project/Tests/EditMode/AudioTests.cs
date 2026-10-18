using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Ton (GDD 14, Prototyp SFX/updateAudio).</summary>
    public class AudioTests
    {
        AudioConfig c;

        [OneTimeSetUp]
        public void Setup() => c = TestData.Config().audio;

        [Test]
        public void MooLowerWithGrowth()
        {
            Assert.AreEqual(1f, AudioLogic.MooPitch(0, c), 1e-4f);
            Assert.AreEqual((150f - 3 * 12f) / 150f, AudioLogic.MooPitch(3, c), 1e-4f, "150 − 12·A");
            Assert.IsFalse(AudioLogic.MooWobble(1, false, c));
            Assert.IsTrue(AudioLogic.MooWobble(2, false, c));
            Assert.IsTrue(AudioLogic.MooWobble(0, true, c));
        }

        [Test]
        public void GrumbleByTextAndAnger()
        {
            Assert.AreEqual(0, AudioLogic.GrumbleCount("", c));
            Assert.AreEqual(5, AudioLogic.GrumbleCount(new string('x', 30), c));
            Assert.AreEqual(12, AudioLogic.GrumbleCount(new string('x', 500), c), "höchstens 12 Silben");
            Assert.Less(AudioLogic.GrumblePitch(80f, 0f, c), AudioLogic.GrumblePitch(0f, 0f, c), "tiefer bei Wut");
            Assert.Greater(AudioLogic.GrumbleVolume(80f, c), AudioLogic.GrumbleVolume(0f, c), "lauter bei Wut");
        }

        [Test]
        public void WhirAndMarch()
        {
            Assert.AreEqual(60f / 400f, AudioLogic.WhirPitch(0f, c), 1e-4f);
            Assert.AreEqual(760f / 400f, AudioLogic.WhirPitch(1f, c), 1e-4f);
            Assert.AreEqual(1f, AudioLogic.MarchPitch(0, c), 1e-4f);
            Assert.AreEqual(523f / 392f, AudioLogic.MarchPitch(2, c), 1e-4f);
            Assert.AreEqual(AudioLogic.MarchPitch(0, c), AudioLogic.MarchPitch(c.marchNotes.Length, c), "Melodie wiederholt sich");
        }

        [Test]
        public void ChannelsAndMute()
        {
            Assert.AreEqual(c.master * 0.5f * 0.8f, AudioLogic.Channel(true, c, 0.5f, 0.8f, 0.2f, false), 1e-5f);
            Assert.AreEqual(c.master * 0.5f * 0.2f, AudioLogic.Channel(false, c, 0.5f, 0.8f, 0.2f, false), 1e-5f);
            Assert.AreEqual(0f, AudioLogic.Channel(false, c, 1f, 1f, 1f, true), "Ton aus");
        }

        [Test]
        public void ToastSoundsPointToExistingTexts()
        {
            var texts = TestData.Texts();
            Assert.AreEqual("fanfare", AudioLogic.SoundForToast("ui.toasts.planMet", c));
            Assert.AreEqual(AudioLogic.Voice, AudioLogic.SoundForToast("ui.toasts.cloned", c));
            Assert.IsNull(AudioLogic.SoundForToast("ui.toasts.gibtEsNicht", c));
            foreach (var t in c.toastSounds) Assert.IsTrue(texts.Has(t.key), "Text fehlt: " + t.key);
        }
    }
}
