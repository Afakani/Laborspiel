using System.Linq;
using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Kombinationsnamen und Vor-/Nachteile (GDD 4.4).</summary>
    public class ComboTests
    {
        GameConfig cfg;
        TextDatabase texts;

        [OneTimeSetUp]
        public void Setup()
        {
            cfg = TestData.Config();
            texts = TestData.Texts();
        }

        [TestCase(1, 1, 1, "Planformel")]
        [TestCase(0, 2, 0, "Euterflut")]
        [TestCase(3, 3, 0, "Wahnsinnsmischung")]
        [TestCase(4, 0, 0, "Mastmischung")]
        [TestCase(0, 0, 3, "Stabilisator")]
        [TestCase(4, 1, 0, "Mastmischung (extrem)")]
        [TestCase(2, 2, 3, "Kombimischung (stabilisiert)")]
        [TestCase(1, 4, 0, "Melkmischung (extrem)")]
        [TestCase(1, 3, 2, "Melkmischung (stabilisiert)")]
        public void Names(int a, int b, int c, string expected)
        {
            Assert.AreEqual(expected, Formula.ComboName(new Dose(a, b, c), cfg, texts));
        }

        [Test]
        public void Info_1_1_0()
        {
            var info = Formula.GetComboInfo(new Dose(1, 1, 0), cfg);
            var pros = ComboTexts.Effects(info.pros, texts);
            var cons = ComboTexts.Effects(info.cons, texts);
            CollectionAssert.AreEqual(new[] { "Fleisch +80 %", "Milch/Eier +95 %" }, pros);
            CollectionAssert.AreEqual(new[] { "Geschmack/Qualität −20" }, cons);
        }

        [Test]
        public void Info_2_2_0_HasAllWarnings()
        {
            var info = Formula.GetComboInfo(new Dose(2, 2, 0), cfg);
            var types = info.cons.Select(e => e.type).ToList();
            CollectionAssert.Contains(types, ComboEffectType.LowStability);
            CollectionAssert.Contains(types, ComboEffectType.Heat);
            CollectionAssert.Contains(types, ComboEffectType.Acid);
            CollectionAssert.DoesNotContain(types, ComboEffectType.Float);
        }

        [Test]
        public void Info_OnlyC()
        {
            var info = Formula.GetComboInfo(new Dose(0, 0, 2), cfg);
            var pros = ComboTexts.Effects(info.pros, texts);
            var cons = ComboTexts.Effects(info.cons, texts);
            CollectionAssert.AreEqual(new[] { "Qualität +16", "stabilisiert (+40 Stabilität)" }, pros);
            CollectionAssert.AreEqual(new[] { "kein Ertragsgewinn" }, cons);
        }

        [Test]
        public void Info_Dampened()
        {
            var info = Formula.GetComboInfo(new Dose(1, 1, 1), cfg);
            var cons = ComboTexts.Effects(info.cons, texts);
            CollectionAssert.Contains(cons, "Wirkung gedämpft −13 %");
        }

        [Test]
        public void ProbeText()
        {
            string s = ComboTexts.Probe(Formula.Probe(new Dose(2, 2, 0), cfg), texts);
            Assert.AreEqual("Fleisch ↑↑↑ · Milch ↑↑↑ · Geschmack ↓↓ · Probe brodelt gefährlich", s);
        }
    }
}
