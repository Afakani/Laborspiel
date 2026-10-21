using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Hinweise bei erstem Auftreten (Kontrolle, Band, Not-Stopp …).</summary>
    public class HintTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        GameState Started()
        {
            var s = GameState.Create(cfg);
            DayLogic.StartDay(s, cfg, () => 0.5f);
            s.tutorialStep = -1;
            s.hintsOn = true;
            return s;
        }

        [Test]
        public void EveryHintHasAText()
        {
            var t = TestData.Texts();
            foreach (var h in cfg.tutorial.hints)
                Assert.IsFalse(string.IsNullOrEmpty(t.Get("ui.tutorial.hints." + h.id)) || t.Get("ui.tutorial.hints." + h.id).StartsWith("ui."), h.id);
            Assert.Greater(t.Count("ui.intro.pages"), 2, "Einsatzbesprechung in Seiten");
        }

        [Test]
        public void BeltHintShowsOnceUntilRepaired()
        {
            var s = Started();
            var c = new HintLogic.Context();
            Assert.IsNull(HintLogic.Tick(s, cfg, c, 0.1f));
            s.beltBroken = true;
            Assert.AreEqual("belt", HintLogic.Tick(s, cfg, c, 0.1f));
            Assert.IsNull(HintLogic.Tick(s, cfg, c, 0.1f), "bleibt stehen");
            Assert.AreEqual("belt", s.hint);
            s.beltBroken = false;
            HintLogic.Tick(s, cfg, c, 0.1f);
            Assert.IsNull(s.hint, "gelöst");
            s.beltBroken = true;
            Assert.IsNull(HintLogic.Tick(s, cfg, c, 0.1f), "nur beim ersten Mal");
        }

        [Test]
        public void UrgentHintReplacesInfoHint()
        {
            var s = Started();
            s.dayNum = 2;
            var c = new HintLogic.Context();
            string first = HintLogic.Tick(s, cfg, c, 0.1f);
            Assert.IsTrue(HintLogic.Find(first, cfg).info, "an Tag 2 kommt zuerst ein Info-Hinweis");
            c.unstable = true;
            Assert.AreEqual("unstable", HintLogic.Tick(s, cfg, c, 0.1f));
        }

        [Test]
        public void NoHintsWithoutTutorialOrDuringSteps()
        {
            var s = Started();
            s.hintsOn = false;
            s.beltBroken = true;
            Assert.IsNull(HintLogic.Tick(s, cfg, new HintLogic.Context(), 0.1f));
            s.hintsOn = true;
            s.tutorialStep = 0;
            Assert.IsNull(HintLogic.Tick(s, cfg, new HintLogic.Context(), 0.1f), "geführte Schritte haben Vorrang");
        }

        [Test]
        public void InfoHintDisappearsAfterReadTime()
        {
            var s = Started();
            s.dayNum = 2;
            var c = new HintLogic.Context();
            string first = HintLogic.Tick(s, cfg, c, 0.1f);
            Assert.IsNotNull(first);
            HintLogic.Tick(s, cfg, c, cfg.tutorial.hintInfoSec + 0.1f);
            Assert.AreNotEqual(first, s.hint, "nach der Lesezeit kommt der nächste oder keiner");
        }

        [Test]
        public void SkipTurnsHintsOff()
        {
            var s = Started();
            TutorialLogic.Skip(s);
            Assert.IsFalse(s.hintsOn);
        }
    }
}
