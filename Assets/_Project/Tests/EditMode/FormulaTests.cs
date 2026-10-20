using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Formeln gegen die Testtabelle aus 05_Unity_Architektur.md (Werte aus dem Prototyp).</summary>
    public class FormulaTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.PrototypeConfig();

        // Dosis A-B-C, Stabilität, Qualität, Rate Kuh, Fleisch Kuh
        [TestCase(0, 0, 0, 100f, 80f, 2.00f, 6.00f)]
        [TestCase(0, 1, 0, 82f, 68f, 3.60f, 6.60f)]
        [TestCase(1, 1, 0, 62f, 60f, 3.90f, 10.80f)]
        [TestCase(2, 2, 0, 12f, 40f, 5.80f, 15.60f)]
        [TestCase(1, 1, 1, 82f, 68f, 3.65f, 10.17f)]
        [TestCase(2, 3, 5, 32f, 68f, 5.09f, 11.83f)]
        [TestCase(0, 4, 2, 58f, 48f, 6.92f, 7.85f)]
        public void Cow(int a, int b, int c, float stab, float qual, float rate, float meat)
        {
            var d = new Dose(a, b, c);
            Assert.AreEqual(stab, Formula.Stability(d, cfg), 0.001f, "Stabilität");
            Assert.AreEqual(qual, Formula.Quality(d, cfg), 0.001f, "Qualität");
            Assert.AreEqual(rate, Formula.Rate(AnimalKind.Cow, d, cfg), 0.005f, "Rate");
            Assert.AreEqual(meat, Formula.Meat(AnimalKind.Cow, d, cfg), 0.005f, "Fleisch");
        }

        [Test]
        public void Hen_0_4_2()
        {
            var d = new Dose(0, 4, 2);
            Assert.AreEqual(58f, Formula.Stability(d, cfg), 0.001f);
            Assert.AreEqual(48f, Formula.Quality(d, cfg), 0.001f);
            Assert.AreEqual(2.77f, Formula.Rate(AnimalKind.Hen, d, cfg), 0.005f);
            Assert.AreEqual(2.62f, Formula.Meat(AnimalKind.Hen, d, cfg), 0.005f);
        }

        [Test]
        public void StabilityIsClamped()
        {
            Assert.AreEqual(100f, Formula.Stability(new Dose(0, 0, 3), cfg), 0.001f);
            Assert.AreEqual(0f, Formula.Stability(new Dose(4, 4, 0), cfg), 0.001f);
        }

        [Test]
        public void ConcentratedScalesOnlyTheGain()
        {
            // 1-1-0 Kuh: Rate 3.9, konzentriert ×1.5 → 2 + 1.9·1.5 = 4.85
            var d = new Dose(1, 1, 0);
            Assert.AreEqual(4.85f, Formula.AnimalRate(AnimalKind.Cow, d, null, 1.5f, cfg), 0.001f);
            Assert.AreEqual(3.9f, Formula.AnimalRate(AnimalKind.Cow, d, null, 0f, cfg), 0.001f);
        }

        [Test]
        public void AnimalStabilityIncludesFrozenAndMusic()
        {
            var d = new Dose(1, 1, 0); // 62
            Assert.AreEqual(62f + 20f + 15f, Formula.AnimalStability(d, null, cfg.formulas.frozenStabBonus, true, cfg), 0.001f);
            Assert.AreEqual(100f, Formula.AnimalStability(new Dose(0, 0, 1), null, 20f, true, cfg), 0.001f);
        }

        [Test]
        public void UltimateOmegaOverridesFormula()
        {
            var omega = cfg.UltimateById("omega");
            Assert.NotNull(omega);
            var d = omega.Recipe;
            Assert.AreEqual(100f, Formula.AnimalStability(d, omega, 0f, false, cfg), 0.001f);
            Assert.AreEqual(92f, Formula.AnimalQuality(d, omega, false, null, false, cfg), 0.001f);
            Assert.AreEqual(7f, Formula.AnimalRate(AnimalKind.Cow, d, omega, 0f, cfg), 0.001f);
            Assert.AreEqual(7.8f, Formula.AnimalMeat(AnimalKind.Cow, d, omega, 0f, null, cfg), 0.001f);
        }

        [Test]
        public void QualityModifiers()
        {
            var d = new Dose(1, 1, 0); // 60
            Assert.AreEqual(75f, Formula.AnimalQuality(d, null, true, null, false, cfg), 0.001f);
            Assert.AreEqual(24f, Formula.AnimalQuality(d, null, false, null, true, cfg), 0.001f);
            var coin = cfg.FunRecipeFor(AnimalKind.Hen, new Dose(0, 4, 2));
            Assert.NotNull(coin);
            Assert.AreEqual(33f, Formula.AnimalQuality(new Dose(0, 4, 2), null, false, coin, false, cfg), 0.001f);
        }

        [Test]
        public void SingingCowMeatFactor()
        {
            var sing = cfg.FunRecipeFor(AnimalKind.Cow, new Dose(3, 0, 3));
            Assert.NotNull(sing);
            float plain = Formula.Meat(AnimalKind.Cow, new Dose(3, 0, 3), cfg);
            Assert.AreEqual(plain * 0.8f, Formula.AnimalMeat(AnimalKind.Cow, new Dose(3, 0, 3), null, 0f, sing, cfg), 0.001f);
        }

        [Test]
        public void ProbeHints()
        {
            var calm = Formula.Probe(Dose.Zero, cfg);
            Assert.AreEqual(0, calm.meatLevel);
            Assert.AreEqual(0, calm.milkLevel);
            Assert.AreEqual(0, calm.taste);   // Qualität 80 → –
            Assert.AreEqual(0, calm.danger);  // ruhig

            var wild = Formula.Probe(new Dose(2, 2, 0), cfg); // Fleisch 2.6, Rate 2.9, Q 40, S 12
            Assert.AreEqual(3, wild.meatLevel);
            Assert.AreEqual(3, wild.milkLevel);
            Assert.AreEqual(-2, wild.taste);
            Assert.AreEqual(2, wild.danger);

            // Fleischfaktor 1.8 liegt genau auf der Schwelle (Rundung), daher hier nicht geprüft.
            var mid = Formula.Probe(new Dose(1, 1, 0), cfg); // Rate 1.95, Q 60, S 62
            Assert.AreEqual(3, mid.milkLevel);
            Assert.AreEqual(-1, mid.taste);
            Assert.AreEqual(0, mid.danger);
        }
    }
}
