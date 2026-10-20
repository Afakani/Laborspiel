using System.Collections.Generic;
using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Tagesablauf, Quote, Sonderwünsche, Einführung, Spielwerte und Spielende (GDD 7, 13).</summary>
    public class DayTests
    {
        GameConfig cfg;
        readonly List<DayEvent> events = new List<DayEvent>();

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        GameState Started(int dayBefore = 0)
        {
            var s = GameState.Create(cfg);
            s.dayNum = dayBefore;
            DayLogic.StartDay(s, cfg, () => 0f);
            return s;
        }

        [Test]
        public void StartDaySetsQuotaAndContractFromDayTwo()
        {
            var s = Started();
            Assert.AreEqual(1, s.dayNum);
            Assert.AreEqual(cfg.day.targets[0].m, s.quota.target.m);
            Assert.AreEqual(cfg.day.dayLengthSec, s.quota.left);
            Assert.AreEqual(cfg.day.prizes[0], s.quota.prize);
            Assert.IsNull(s.contract, "Tag 1 ohne Sonderauftrag");
            Assert.AreEqual(0, s.tutorialStep, "Tag 1 mit Einführung");
            var s2 = Started(1);
            Assert.IsNotNull(s2.contract);
            Assert.AreEqual(-1, s2.tutorialStep);
            Assert.AreEqual(cfg.wishes.firstAfterSec, s2.wishTimer);
        }

        [Test]
        public void OverfulfilmentAndContractPayMore()
        {
            var s = Started(1);
            s.contract = new Contract { data = new ContractConfig { k = "m", minQ = 80f, who = "x", what = "y" } };
            float basePay = Production.PayPerTon(cfg, 'm');
            Assert.AreEqual(10f * basePay * cfg.contracts.payMult, DayLogic.Pay(s, cfg, 'm', 10f, 90f, out _), 0.001f);
            Assert.AreEqual(10f * basePay, DayLogic.Pay(s, cfg, 'm', 10f, 50f, out _), 0.001f, "unter Mindestqualität normal");
            s.quota.prod.m = s.quota.target.m;
            Assert.AreEqual(10f * basePay * (1f + cfg.day.overfulfilPayBonus), DayLogic.Pay(s, cfg, 'm', 10f, 50f, out bool over), 0.001f);
            Assert.IsTrue(over, "erste Übererfüllung wird gemeldet");
            DayLogic.Pay(s, cfg, 'm', 1f, 50f, out over);
            Assert.IsFalse(over, "nur einmal");
        }

        [Test]
        public void PlanMetPaysPrizeAndCalmsDirector()
        {
            var s = Started();
            s.anger = 50f;
            float money = s.Money;
            s.quota.prod.m = s.quota.target.m;
            s.quota.prod.e = s.quota.target.e;
            s.quota.prod.f = s.quota.target.f;
            events.Clear();
            DayLogic.Tick(s, cfg, 0.1f, events);
            CollectionAssert.Contains(events, DayEvent.PlanMet);
            Assert.AreEqual(money + cfg.day.prizes[0], s.Money, 0.001f);
            Assert.AreEqual(50f + cfg.anger.planMet, s.anger, 0.001f);
            var r = DayLogic.EndDay(s, cfg, false);
            Assert.IsTrue(r.met);
            Assert.AreEqual(1, s.planIdx);
            Assert.IsNull(s.quota);
        }

        [Test]
        public void FailedDayRepeatsAndAngers()
        {
            var s = Started();
            events.Clear();
            DayLogic.Tick(s, cfg, cfg.day.dayLengthSec + 1f, events);
            CollectionAssert.Contains(events, DayEvent.TimeUp);
            var r = DayLogic.EndDay(s, cfg, false);
            Assert.IsFalse(r.met);
            Assert.AreEqual(0, s.planIdx);
            Assert.AreEqual(1, s.quotasFailed);
            Assert.AreEqual(cfg.anger.planFailed, s.anger, 0.001f);
            s.anger = cfg.anger.max - 1f;
            DayLogic.StartDay(s, cfg, () => 0f);
            Assert.IsTrue(DayLogic.EndDay(s, cfg, false).fired);
        }

        [Test]
        public void CollapseCountsAsFailedEvenIfMet()
        {
            var s = Started();
            s.quota.met = true;
            var r = DayLogic.EndDay(s, cfg, true);
            Assert.IsFalse(r.met);
            Assert.AreEqual(0, s.planIdx);
        }

        [Test]
        public void LastPlanEndsTheGame()
        {
            var s = GameState.Create(cfg);
            s.planIdx = cfg.day.plans - 1;
            DayLogic.StartDay(s, cfg, () => 0f);
            s.quota.met = true;
            Assert.IsTrue(DayLogic.EndDay(s, cfg, false).final);
        }

        [Test]
        public void MeatWishSucceedsWhenDelivered()
        {
            var s = Started(1);
            s.wishTimer = 0.01f;
            float roll = 0f; // erster Wunschtyp = Fleisch
            events.Clear();
            DayLogic.TickWish(s, cfg, 0.1f, false, () => roll, events);
            CollectionAssert.Contains(events, DayEvent.WishNew);
            Assert.AreEqual("meat", s.wish.Id);
            Assert.AreEqual(cfg.wishes.types[0].amount.@base, s.wish.need, 0.001f);
            float money = s.Money;
            DayLogic.Pay(s, cfg, 'f', s.wish.need, 50f, out _);
            events.Clear();
            DayLogic.TickWish(s, cfg, 0.1f, false, () => roll, events);
            CollectionAssert.Contains(events, DayEvent.WishDone);
            Assert.AreEqual(money + DayLogic.WishReward(s, cfg), s.Money, 0.001f);
            Assert.IsNull(s.wish);
        }

        [Test]
        public void CalmWishFailsOnCrisis()
        {
            var s = Started(1);
            s.wish = new Wish { type = System.Array.Find(cfg.wishes.types, t => t.id == "calm"), need = 1f, left = 1f };
            events.Clear();
            DayLogic.TickWish(s, cfg, 0.5f, true, () => 0f, events);
            DayLogic.TickWish(s, cfg, 0.6f, false, () => 0f, events);
            CollectionAssert.Contains(events, DayEvent.WishFailed);
            Assert.AreEqual(cfg.anger.wishFailed, s.anger, 0.001f);
        }

        [Test]
        public void QualityWishCountsProcessedAnimal()
        {
            var s = Started(1);
            var type = System.Array.Find(cfg.wishes.types, t => t.id == "quality");
            s.wish = new Wish { type = type, need = 1f, left = 10f };
            DayLogic.OnProcessed(s, type.minQuality - 1f);
            Assert.AreEqual(0f, s.wish.got);
            DayLogic.OnProcessed(s, type.minQuality);
            Assert.AreEqual(1f, s.wish.got);
        }

        [Test]
        public void TutorialFollowsTheCoreLoop()
        {
            var s = Started();
            events.Clear();
            var none = new TutorialLogic.Context();
            TutorialLogic.Tick(s, cfg, none, 0.1f, events);
            Assert.AreEqual(0, s.tutorialStep, "erst zur Mischstation");
            TutorialLogic.Tick(s, cfg, new TutorialLogic.Context { mixingOpen = true }, 0.1f, events);
            Assert.AreEqual(1, s.tutorialStep);
            s.serums.Add(new Serum(new Dose(0, 1, 0), SerumStage.Raw));
            TutorialLogic.Tick(s, cfg, none, 0.1f, events);
            TutorialLogic.Tick(s, cfg, new TutorialLogic.Context { hasAnimal = true }, 0.1f, events);
            TutorialLogic.Tick(s, cfg, new TutorialLogic.Context { hasAnimal = true, animalDosed = true }, 0.1f, events);
            Assert.AreEqual("watch", TutorialLogic.Current(s, cfg).id);
            TutorialLogic.Tick(s, cfg, none, cfg.tutorial.readSec, events);
            Assert.AreEqual("process", TutorialLogic.Current(s, cfg).id, "Lese-Schritt nach readSec weiter");
            s.totals.f = 1f;
            TutorialLogic.Tick(s, cfg, none, 0.1f, events);
            Assert.AreEqual("order", TutorialLogic.Current(s, cfg).id);
            TutorialLogic.Tick(s, cfg, none, 100f, events);
            Assert.AreEqual("order", TutorialLogic.Current(s, cfg).id, "Bestellen ist kein Lese-Schritt");
            s.pendingOrder = new int[OrderLogic.Items];
            TutorialLogic.Tick(s, cfg, none, 0.1f, events);
            TutorialLogic.Tick(s, cfg, none, cfg.tutorial.readSec, events);
            TutorialLogic.Tick(s, cfg, none, cfg.tutorial.readSec, events);
            Assert.AreEqual(-1, s.tutorialStep);
            CollectionAssert.Contains(events, DayEvent.TutorialDone);
        }

        [Test]
        public void IdleAngerAfterDelay()
        {
            var s = GameState.Create(cfg);
            float idle = 0f;
            Assert.IsFalse(DayLogic.TickIdle(ref idle, false, s, cfg, cfg.anger.idleAfterSec - 0.5f));
            Assert.AreEqual(0f, s.anger);
            Assert.IsTrue(DayLogic.TickIdle(ref idle, false, s, cfg, 1f), "erstes Mal Leerlauf meldet");
            Assert.Greater(s.anger, 0f);
            Assert.IsFalse(DayLogic.TickIdle(ref idle, false, s, cfg, 1f));
            DayLogic.TickIdle(ref idle, true, s, cfg, 1f);
            Assert.AreEqual(0f, idle);
        }

        [Test]
        public void WorldValuesAndEndings()
        {
            var s = GameState.Create(cfg);
            var w = WorldValues.Compute(s, cfg);
            Assert.AreEqual(cfg.world.popMax, w.pop, 0.001f, "Start ohne Hunger");
            Assert.AreEqual(cfg.world.envMax, w.env, 0.001f);
            s.explosions = 2;
            Assert.AreEqual(cfg.world.envMax - 2 * cfg.world.envPerExplosion, WorldValues.Compute(s, cfg).env, 0.001f);
            Assert.AreEqual("hero_alive", WorldValues.EndingKey(GameOverKind.Final, s, cfg));
            Production.Deliver(s, cfg, 'm', 100f, 0f);
            Assert.AreEqual("hero_dead", WorldValues.EndingKey(GameOverKind.Final, s, cfg), "Volk verhungert bei Qualität 0");
            Assert.AreEqual("fired", WorldValues.EndingKey(GameOverKind.Fired, s, cfg));
        }
    }
}
