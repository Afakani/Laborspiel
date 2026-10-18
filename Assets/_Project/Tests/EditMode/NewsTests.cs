using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Staats-TV, Rekorde, Logbuch und Fenster (GDD 11, 12, 7.7).</summary>
    public class NewsTests
    {
        GameConfig cfg;
        TextDatabase texts;

        [OneTimeSetUp]
        public void Setup()
        {
            cfg = TestData.Config();
            texts = TestData.Texts();
        }

        [Test]
        public void LevelFollowsFamine()
        {
            var n = cfg.news;
            Assert.AreEqual(NewsLevel.Boom, NewsLogic.Level(0f, cfg));
            Assert.AreEqual(NewsLevel.Warn, NewsLogic.Level(n.levelWarn, cfg));
            Assert.AreEqual(NewsLevel.Crisis, NewsLogic.Level(n.levelCrisis, cfg));
            Assert.AreEqual(NewsLevel.Dead, NewsLogic.Level(n.levelDead, cfg));
            Assert.IsFalse(NewsLogic.AnchorGone(n.anchorGoneFrom - 0.01f, cfg));
            Assert.IsTrue(NewsLogic.AnchorGone(n.anchorGoneFrom, cfg));
        }

        [Test]
        public void ComposeFillsPlaceholdersAndPutsFactsFirst()
        {
            var s = GameState.Create(cfg);
            s.dayNum = 2;
            s.newsFacts.Add("newsFacts.flyAway");
            s.newsFacts.Add("newsFacts.inspection");
            s.lastPlanLine = "Tag 2: Plan 1 von 5 erfüllt";
            var c = NewsLogic.Compose(s, cfg, texts, () => 0f);
            Assert.AreEqual(NewsLevel.Boom, c.level, "Startqualität 80 → kein Hunger");
            Assert.IsFalse(c.head.Contains("{"), "Platzhalter ersetzt: " + c.head);
            Assert.AreEqual(texts.Get("newsFacts.inspection"), c.ticker[0], "neueste Tat zuerst");
            Assert.AreEqual(texts.Get("newsFacts.flyAway"), c.ticker[1]);
            Assert.AreEqual(s.lastPlanLine, c.ticker[2]);
            Assert.AreEqual(0, s.newsFacts.Count, "Taten verbraucht");
            Assert.IsFalse(c.special);
        }

        [Test]
        public void ExplosionFactPicksFromList()
        {
            var s = GameState.Create(cfg);
            s.newsFacts.Add("newsFacts.explosion");
            var c = NewsLogic.Compose(s, cfg, texts, () => 0f);
            CollectionAssert.Contains(texts.GetList("newsFacts.explosion"), c.ticker[0]);
        }

        [Test]
        public void SpecialNewsAfterUltimate()
        {
            var s = GameState.Create(cfg);
            s.specialNewsUltimate = UltimateIds.Omega;
            var c = NewsLogic.Compose(s, cfg, texts, () => 0f);
            Assert.IsTrue(c.special);
            Assert.AreEqual(texts.Get("ui.news.specialTag"), c.tag);
            StringAssert.Contains(cfg.UltimateById(UltimateIds.Omega).displayName, c.head);
            Assert.IsNull(s.specialNewsUltimate, "nur einmal");
        }

        [Test]
        public void FamineGivesDeadBroadcast()
        {
            var s = GameState.Create(cfg);
            Production.Deliver(s, cfg, 'm', 100f, 0f);
            var c = NewsLogic.Compose(s, cfg, texts, () => 0f);
            Assert.AreEqual(NewsLevel.Dead, c.level);
            Assert.IsTrue(c.anchorGone);
        }

        [Test]
        public void MeatAndQualityPercent()
        {
            var s = GameState.Create(cfg);
            s.meatTargetTotal = 30f;
            Production.Deliver(s, cfg, 'f', 45f, cfg.world.avgQStart + 10f);
            var w = WorldValues.Compute(s, cfg);
            Assert.AreEqual(50f, w.meat, 0.01f, "45 t bei 30 t Soll = +50 %");
            Assert.AreEqual(10f, w.quality, 0.01f);
            Assert.AreEqual("+50", NewsLogic.Signed(50f));
            Assert.AreEqual("−5", NewsLogic.Signed(-5f));
        }

        [Test]
        public void RecordFirstSilentThenAnnounced()
        {
            var s = GameState.Create(cfg);
            Assert.IsFalse(RecordLogic.Check(s, cfg, RecordLogic.Rate, 2f), "erster Wert still");
            Assert.AreEqual(2f, s.records[RecordLogic.Rate]);
            Assert.IsFalse(RecordLogic.Check(s, cfg, RecordLogic.Rate, 2f + cfg.records.epsilon * 0.5f), "zu knapp");
            Assert.IsTrue(RecordLogic.Check(s, cfg, RecordLogic.Rate, 3f));
            Assert.IsFalse(RecordLogic.Check(s, cfg, RecordLogic.Rate, 2.5f), "kleiner zählt nicht");
            Assert.AreEqual(3f, s.records[RecordLogic.Rate]);
        }

        [Test]
        public void LogbookShowsOnlyDiscovered()
        {
            var s = GameState.Create(cfg);
            Assert.AreEqual(0, LogbookLogic.Build(s, cfg, texts).Count);
            s.discoveredRecipes.Add("2-1-0");
            s.discoveredRecipes.Add("1-0-0");
            s.discoveredRecipes.Add("U:" + UltimateIds.Omega);
            s.foundFunRecipes.Add(UltimateIds.SingingCow);
            s.records[RecordLogic.DayMilk] = 123f;
            var list = LogbookLogic.Build(s, cfg, texts);
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(LogbookSection.Combo, list[0].section);
            Assert.AreEqual(new Dose(1, 0, 0).Key, list[0].sketch.Key, "nach Gesamtdosis sortiert");
            Assert.AreEqual(LogbookSection.Secret, list[2].section);
            Assert.AreEqual(LogbookSection.Curiosity, list[3].section);
            Assert.AreEqual(LogbookSection.Records, list[4].section);
            StringAssert.Contains("123", list[4].pros[0]);
        }

        [Test]
        public void WindowModes()
        {
            var w = new WorldValues { pop = 100f, food = 100f };
            Assert.AreEqual(2, WindowView.Mode(w, cfg), "Festumzug");
            w.food = 60f;
            Assert.AreEqual(0, WindowView.Mode(w, cfg), "nichts");
            w.pop = 50f;
            Assert.AreEqual(1, WindowView.Mode(w, cfg), "Schlange");
        }
    }
}
