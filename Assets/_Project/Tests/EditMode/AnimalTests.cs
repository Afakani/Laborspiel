using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Tierwerte und Lieferung (GDD 4.3, 4.6, 7.1).</summary>
    public class AnimalTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        AnimalStats Animal(AnimalKind kind, Dose d)
        {
            var a = new AnimalStats();
            a.Reset(cfg.animal.eggInterval);
            a.kind = kind;
            a.dose = d;
            return a;
        }

        [Test]
        public void Cow_1_1_0_MatchesPrompt4()
        {
            var a = Animal(AnimalKind.Cow, new Dose(1, 1, 0));
            Assert.AreEqual(62f, a.BaseStability(false, cfg), 0.001f);
            Assert.AreEqual(60f, a.Quality(cfg), 0.001f);
            Assert.AreEqual(3.9f, a.Rate(false, cfg), 0.001f);
            Assert.AreEqual(10.8f, a.Meat(cfg), 0.001f);
            Assert.AreEqual('m', a.Produces(cfg));
        }

        [Test]
        public void AcidAndBlackout()
        {
            var a = Animal(AnimalKind.Cow, new Dose(1, 1, 0));
            a.acid = true;
            Assert.AreEqual(3.9f * 0.4f, a.Rate(false, cfg), 0.001f);
            Assert.AreEqual(3.9f * 0.4f * 0.5f, a.Rate(true, cfg), 0.001f);
            Assert.AreEqual(10.8f * 0.5f, a.Meat(cfg), 0.001f);
            Assert.AreEqual(24f, a.Quality(cfg), 0.001f);
        }

        [Test]
        public void MilkHenGivesMilk()
        {
            var a = Animal(AnimalKind.Hen, new Dose(1, 3, 1));
            a.fun = cfg.FunRecipeFor(AnimalKind.Hen, a.dose);
            Assert.NotNull(a.fun);
            Assert.AreEqual('m', a.Produces(cfg));
            Assert.AreEqual(Formula.Rate(AnimalKind.Hen, a.dose, cfg) * 1.6f, a.Rate(false, cfg), 0.001f);
            var plain = Animal(AnimalKind.Hen, new Dose(0, 1, 0));
            Assert.AreEqual('e', plain.Produces(cfg));
        }

        [Test]
        public void FrozenBonusRaisesBaseStability()
        {
            var a = Animal(AnimalKind.Cow, new Dose(2, 2, 0)); // 12
            a.stabBonus = cfg.formulas.frozenStabBonus;
            Assert.AreEqual(32f, a.BaseStability(false, cfg), 0.001f);
            Assert.AreEqual(47f, a.BaseStability(true, cfg), 0.001f);
        }

        [Test]
        public void DeliverPaysAndTracksQuality()
        {
            var s = GameState.Create(cfg);
            float before = s.Money;
            Production.Deliver(s, cfg, 'm', 10f, 60f);
            Production.Deliver(s, cfg, 'f', 10f, 80f);
            Assert.AreEqual(before + 10f * 1.0f + 10f * 2.2f, s.Money, 0.001f);
            Assert.AreEqual(10f, s.totals.m, 0.001f);
            Assert.AreEqual(10f, s.totals.f, 0.001f);
            Assert.AreEqual(70f, Production.AverageQuality(s, cfg), 0.001f);
            Assert.AreEqual(cfg.world.avgQStart, Production.AverageQuality(GameState.Create(cfg), cfg), 0.001f);
        }
    }
}
