using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Einstellungen und Sprinten: Werte aus der Balancing-JSON sind sinnvoll gesetzt.</summary>
    public class SettingsTests
    {
        [Test]
        public void SprintIsFasterThanWalking()
        {
            var cfg = TestData.Config();
            Assert.Greater(cfg.player.sprintMultiplier, 1f);
            Assert.Greater(cfg.player.walkSpeed * cfg.player.sprintMultiplier, cfg.player.walkSpeed);
        }

        [Test]
        public void WindowAndMouseRanges()
        {
            var s = TestData.Config().settings;
            Assert.Greater(s.windowedWidth, 0);
            Assert.Greater(s.windowedHeight, 0);
            Assert.LessOrEqual(s.mouseMin, s.mouseDefault);
            Assert.GreaterOrEqual(s.mouseMax, s.mouseDefault);
        }

        [Test]
        public void SettingsTextsExist()
        {
            var t = TestData.Texts();
            foreach (var k in new[] { "display", "fullscreen", "windowed", "shake", "graphics" })
                Assert.IsTrue(t.Has("ui.settings." + k), k);
            StringAssert.Contains("Shift", t.Get("ui.hud.controls"));
        }
    }
}
