using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Spielerführung nach Spieltest: Pflicht-Mischung und -Bestellung der Einführung, Empfehlung, Warnmeldungen.</summary>
    public class GuidanceTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        GameState Day(int day)
        {
            var s = GameState.Create(cfg);
            for (int i = 0; i < day; i++) DayLogic.StartDay(s, cfg, () => 0.5f);
            s.tutorialStep = -1;
            return s;
        }

        static int StepIndex(GameConfig cfg, string id) => System.Array.FindIndex(cfg.tutorial.steps, x => x.id == id);

        [Test]
        public void TutorialOnlyAcceptsTheGivenMix()
        {
            var s = Day(1);
            s.tutorialStep = StepIndex(cfg, "mix");
            var need = TutorialLogic.RequiredMix(s, cfg);
            Assert.IsTrue(need.HasValue);
            s.mix = new Dose(0, 2, 0);
            Assert.IsFalse(MixingLogic.TryStartMix(s, cfg).IsOk, "falsche Mischung wird abgelehnt");
            s.mix = need.Value;
            Assert.IsTrue(MixingLogic.TryStartMix(s, cfg).IsOk);

            var free = Day(1);
            free.tutorialStep = StepIndex(cfg, "clone");
            Assert.IsFalse(TutorialLogic.RequiredMix(free, cfg).HasValue, "nach dem Mischschritt frei");
        }

        [Test]
        public void TutorialOrderIsTheRecommendation()
        {
            var s = Day(1);
            s.SetMoney(5000f);
            s.tutorialStep = StepIndex(cfg, "order");
            var need = TutorialLogic.RequiredOrder(s, cfg);
            Assert.IsNotNull(need);
            Assert.IsTrue(OrderLogic.Same(need, OrderLogic.Recommended(s, cfg, false)));
            Assert.Greater(OrderLogic.Sum(cfg, need), 0f);

            s.SetMoney(10f);
            Assert.IsNull(TutorialLogic.RequiredOrder(s, cfg), "zu wenig Geld: freie Wahl, Einführung hängt nicht");
            s.tutorialStep = -1;
            s.SetMoney(5000f);
            Assert.IsNull(TutorialLogic.RequiredOrder(s, cfg));
        }

        [Test]
        public void RecommendationFollowsTomorrowsPlanAndFitsTheCash()
        {
            var s = Day(1);
            var today = OrderLogic.Recommended(s, cfg, false);
            s.quota.met = true;
            var tomorrow = OrderLogic.Recommended(s, cfg, false);
            Assert.GreaterOrEqual(OrderLogic.Sum(cfg, tomorrow), OrderLogic.Sum(cfg, today), "Plan erfüllt: Empfehlung für den nächsten Plan");
            s.SetMoney(200f);
            Assert.LessOrEqual(OrderLogic.Sum(cfg, OrderLogic.Recommended(s, cfg, true)), 200f);
        }

        [Test]
        public void AngerWarnsOncePerLevel()
        {
            var s = Day(1);
            Assert.IsNull(WarningLogic.Tick(s, cfg));
            s.anger = cfg.anger.tiers[0];
            Assert.AreEqual(WarningLogic.AngerAnnoyed, WarningLogic.Tick(s, cfg));
            Assert.IsNull(WarningLogic.Tick(s, cfg), "nur einmal");
            s.anger = cfg.anger.tiers[1];
            Assert.AreEqual(WarningLogic.AngerFurious, WarningLogic.Tick(s, cfg));
            s.anger = 0f;
            WarningLogic.Tick(s, cfg);
            s.anger = cfg.anger.tiers[1];
            Assert.AreEqual(WarningLogic.AngerFurious, WarningLogic.Tick(s, cfg), "nach Beruhigung wieder");
        }

        [Test]
        public void TimeWarningOncePerShiftOnlyIfPlanOpen()
        {
            var s = Day(1);
            s.quota.left = cfg.day.lateNoticeSec + 1f;
            Assert.IsNull(WarningLogic.Tick(s, cfg));
            s.quota.left = cfg.day.lateNoticeSec;
            Assert.AreEqual(WarningLogic.TimeLow, WarningLogic.Tick(s, cfg));
            Assert.IsNull(WarningLogic.Tick(s, cfg));

            var met = Day(1);
            met.quota.met = true;
            met.quota.left = 1f;
            Assert.IsNull(WarningLogic.Tick(met, cfg), "Plan erfüllt: keine Warnung");
        }

        [Test]
        public void InspectorIsAnnouncedBeforeHeComes()
        {
            var s = Day(cfg.inspection.fromDay);
            InspectionLogic.OnDayStart(s, cfg);
            s.playTime = s.nextInspectionAt - cfg.inspection.announceSec - 1f;
            Assert.IsNull(WarningLogic.Tick(s, cfg));
            s.playTime += 2f;
            Assert.AreEqual(WarningLogic.InspectionSoon, WarningLogic.Tick(s, cfg));
            Assert.IsNull(WarningLogic.Tick(s, cfg));
            InspectionLogic.Start(s, cfg);
            Assert.IsFalse(s.inspectionAnnounced, "nächste Kontrolle wird wieder angekündigt");
        }

        [Test]
        public void AlertTextsExist()
        {
            var t = TestData.Texts();
            foreach (var id in new[] { WarningLogic.AngerAnnoyed, WarningLogic.AngerFurious, WarningLogic.TimeLow, WarningLogic.InspectionSoon })
                Assert.IsFalse(t.Get("ui.alerts." + id).StartsWith("ui."), id);
            Assert.IsFalse(t.Get("ui.order.recommend").StartsWith("ui."));
        }
    }
}
