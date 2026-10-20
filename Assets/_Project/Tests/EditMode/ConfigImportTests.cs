using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Prüft, ob der Importer die JSON vollständig in die Assets überträgt.</summary>
    public class ConfigImportTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void StartValues()
        {
            Assert.AreEqual(900f, cfg.start.money);
            Assert.AreEqual(3, cfg.start.inventory.A);
            Assert.AreEqual(3, cfg.start.inventory.B);
            Assert.AreEqual(2, cfg.start.inventory.C);
            Assert.AreEqual(1, cfg.start.antidote);
        }

        [Test]
        public void DayTargets()
        {
            Assert.AreEqual(5, cfg.day.plans);
            Assert.AreEqual(300f, cfg.day.dayLengthSec);
            Assert.AreEqual(5, cfg.day.targets.Length);
            Assert.AreEqual(900f, cfg.day.targets[4].m);
            Assert.AreEqual(140f, cfg.day.targets[4].f);
            Assert.AreEqual(2.2f, cfg.day.payPerTon.f, 0.0001f);
        }

        [Test]
        public void NestedSections()
        {
            Assert.AreEqual(11f, cfg.formulas.stability.perA);
            Assert.AreEqual(0.4f, cfg.formulas.acid.quality, 0.0001f);
            Assert.AreEqual(2.4f, cfg.warnings.@float[1], 0.0001f);
            Assert.AreEqual(1.6f, cfg.inspection.mult.boss, 0.0001f);
            Assert.AreEqual(0.75f, cfg.machinesFixed.centrifuge.zones.green[0], 0.0001f);
            Assert.AreEqual(4, cfg.wishes.types.Length);
            Assert.AreEqual(150f, cfg.wishes.reward.@base);
            Assert.AreEqual("Krankenhaus", cfg.contracts.list[0].who);
            Assert.AreEqual("Wachstum", cfg.substances[0].displayName);
        }

        [Test]
        public void SeparateAssets()
        {
            Assert.AreEqual(2, cfg.ultimates.Count);
            var koloss = cfg.UltimateById("koloss");
            Assert.AreEqual("Fleischkoloss", koloss.displayName);
            Assert.AreEqual(new Dose(4, 1, 4), koloss.Recipe);
            Assert.AreEqual(koloss, cfg.UltimateForRecipe(new Dose(4, 1, 4)));

            Assert.AreEqual(3, cfg.funRecipes.Count);
            var milkhen = cfg.FunRecipeFor(AnimalKind.Hen, new Dose(1, 3, 1));
            Assert.NotNull(milkhen);
            Assert.AreEqual(1.6f, milkhen.rateMult, 0.0001f);
            Assert.AreEqual(1f, milkhen.meatMult, 0.0001f, "Standardwert bleibt 1, wenn die JSON nichts angibt");

            Assert.AreEqual(4, cfg.buildMachineTypes.Count);
            Assert.AreEqual(1.4f, cfg.buildMachineTypes[0].milkPerSec, 0.0001f);
            Assert.AreEqual("Melkpumpe", cfg.buildMachineTypes[0].displayName);

            Assert.AreEqual(21, cfg.comboNames.Count);
            Assert.AreEqual("Planformel", cfg.FixedComboName("1-1-1"));
        }

        [Test]
        public void Layout()
        {
            var L = cfg.layout;
            Assert.AreEqual(3.1f, L.chamber.playerCollisionRadius, 0.0001f);
            Assert.AreEqual(16, L.stations.Length);
            var pult = L.Station("pult");
            Assert.NotNull(pult);
            Assert.AreEqual(new UnityEngine.Vector3(0f, 0f, -6.2f), pult.UnityPosition, "z wird gespiegelt");
            Assert.AreEqual(180f, L.Station("gramophone").facing);
            Assert.AreEqual(2.6f, L.pultZone.absXMax, 0.0001f);
        }

        [Test]
        public void NewGameStateUsesConfig()
        {
            var s = GameState.Create(cfg);
            Assert.AreEqual(900f, s.Money);
            CollectionAssert.AreEqual(new[] { 3, 3, 2 }, s.substances);
            Assert.AreEqual(1, s.antidote);
        }
    }
}
