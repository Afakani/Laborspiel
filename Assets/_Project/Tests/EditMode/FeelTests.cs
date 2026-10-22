using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Rückmeldung und Gefühl: Geld-Zahlen sammeln, Angstlaute des Tiers.</summary>
    public class FeelTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void SmallIncomeIsCollectedSpendingShowsAtOnce()
        {
            var f = cfg.feel;
            var m = new MoneyPopupLogic();
            Assert.AreEqual(0f, m.Add(1f), "Einnahmen werden gesammelt");
            m.Add(2f);
            Assert.AreEqual(0f, m.Tick(f.moneyIntervalSec + 0.1f, f), "unter moneyMin noch nichts");
            m.Add(f.moneyMin);
            Assert.AreEqual(3f + f.moneyMin, m.Tick(0.01f, f), 0.001f, "Intervall schon um: alles zusammen");
            Assert.AreEqual(0f, m.Tick(0.01f, f), "danach wieder leer");
            Assert.AreEqual(-40f, m.Add(-40f), "Ausgaben sofort");
        }

        [Test]
        public void AnimalComplainsBeforeTheCrisis()
        {
            var f = cfg.feel;
            Assert.IsFalse(AudioLogic.Distressed(false, false, 90f, f));
            Assert.IsTrue(AudioLogic.Distressed(false, false, f.distressBelowStab - 1f, f), "niedrige Stabilität");
            Assert.IsTrue(AudioLogic.Distressed(true, false, 90f, f), "instabil");
            Assert.IsTrue(AudioLogic.Distressed(false, true, 90f, f), "Krise");
        }

        [Test]
        public void FeelValuesAreSet()
        {
            var f = cfg.feel;
            Assert.Greater(f.cheerSec, 0f);
            Assert.Greater(f.moneyRiseSec, 0f);
            Assert.Greater(f.tvShakeM, 0f);
            Assert.Greater(f.distressPitch, 1f);
            Assert.IsFalse(TestData.Texts().Get("ui.hud.moneyPlus").StartsWith("ui."));
        }
    }
}
