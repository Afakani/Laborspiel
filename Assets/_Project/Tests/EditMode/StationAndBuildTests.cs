using NUnit.Framework;
using UnityEngine;

namespace PlanErfuellt.Tests
{
    /// <summary>Grammophon, Verkostung, Rohrpost, Mikroskop (GDD 6.2, 6.5–6.7) und Baumodus (GDD 6.10).</summary>
    public class StationAndBuildTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void GramophonePlaysAndRaisesStability()
        {
            var s = GameState.Create(cfg);
            Assert.IsTrue(StationLogic.Crank(s, cfg).IsOk);
            Assert.AreEqual(cfg.machinesFixed.gramophone.musicSec, s.musicLeft, 0.001f);
            Assert.IsFalse(StationLogic.Crank(s, cfg).IsOk, "läuft schon");
            var a = new AnimalStats { dose = new Dose(2, 2, 0) };
            Assert.AreEqual(a.BaseStability(false, cfg) + cfg.formulas.musicStabBonus, a.BaseStability(true, cfg), 0.001f);
            StationLogic.Tick(s, cfg.machinesFixed.gramophone.musicSec + 1f);
            Assert.IsFalse(StationLogic.MusicPlaying(s));
        }

        [Test]
        public void TastingStampsOnceAndCosts()
        {
            var t = cfg.machinesFixed.tasting;
            var s = GameState.Create(cfg);
            var a = new AnimalStats { dose = new Dose(1, 1, 0) };
            Assert.IsFalse(StationLogic.Taste(s, cfg, a, true).IsOk, "vor Plan 2 gesperrt");
            s.planIdx = t.unlockPlanIdx;
            Assert.IsFalse(StationLogic.Taste(s, cfg, a, false).IsOk, "ohne Tier");
            float q = a.Quality(cfg);
            float money = s.Money;
            Assert.IsTrue(StationLogic.Taste(s, cfg, a, true).IsOk);
            Assert.AreEqual(money - t.cost, s.Money, 0.001f);
            Assert.AreEqual(q + t.qualityBonus, a.Quality(cfg), 0.001f);
            Assert.IsFalse(StationLogic.Taste(s, cfg, a, true).IsOk, "nur ein Siegel");
            Assert.AreEqual("tasting.hi", StationLogic.TastingList(t.hiFrom, cfg));
            Assert.AreEqual("tasting.mid", StationLogic.TastingList(t.midFrom, cfg));
            Assert.AreEqual("tasting.lo", StationLogic.TastingList(t.midFrom - 1f, cfg));
        }

        [Test]
        public void CaviarCalmsDirector()
        {
            var s = GameState.Create(cfg);
            s.anger = 60f;
            s.caviar = 1;
            Assert.IsTrue(StationLogic.SendCaviar(s, cfg).IsOk);
            Assert.AreEqual(60f + cfg.anger.caviar, s.anger, 0.001f);
            Assert.AreEqual(0, s.caviar);
            Assert.Greater(s.rohrpostLeft, 0f);
            Assert.IsFalse(StationLogic.SendCaviar(s, cfg).IsOk, "kein Kaviar mehr");
        }

        [Test]
        public void ProbeOnlyForUnknownMixAndOnce()
        {
            var s = GameState.Create(cfg);
            Assert.IsFalse(StationLogic.TakeProbe(s, cfg).IsOk, "leere Mischung");
            s.mix = new Dose(1, 2, 0);
            float money = s.Money;
            Assert.IsTrue(StationLogic.TakeProbe(s, cfg).IsOk);
            Assert.AreEqual(money - cfg.machinesFixed.microscope.cost, s.Money, 0.001f);
            Assert.AreEqual(cfg.machinesFixed.microscope.analyseSec, s.probeLeft, 0.001f);
            Assert.IsFalse(StationLogic.TakeProbe(s, cfg).IsOk, "schon geprüft");
            s.mix = new Dose(1, 0, 0);
            s.discoveredRecipes.Add(s.mix.Key);
            Assert.IsFalse(StationLogic.CanProbe(s), "bekannte Mischung braucht keine Probe");
        }

        [Test]
        public void PriceGrowsPerSameType()
        {
            var s = GameState.Create(cfg);
            s.SetMoney(100000f);
            var pump = BuildLogic.Type(cfg, BuildLogic.Pump);
            Assert.AreEqual(pump.price, BuildLogic.Price(s, cfg, pump), 0.001f);
            Assert.IsTrue(BuildLogic.Place(s, cfg, pump, 0, 1).IsOk);
            Assert.AreEqual(Mathf.Round(pump.price * cfg.buildMachines.priceFactor), BuildLogic.Price(s, cfg, pump), 0.001f);
            Assert.AreEqual(1, s.machinesBuilt);
        }

        [Test]
        public void PlacementRules()
        {
            var g = cfg.buildMachines.grid;
            var s = GameState.Create(cfg);
            s.SetMoney(100000f);
            var pump = BuildLogic.Type(cfg, BuildLogic.Pump);
            var spray = BuildLogic.Type(cfg, BuildLogic.Spray);
            Assert.IsFalse(BuildLogic.Place(s, cfg, spray, 0, 1).IsOk, "Auto-Sprüher erst ab Plan 2");
            Assert.IsFalse(BuildLogic.Place(s, cfg, pump, g.blockedFrom, g.blockedRow).IsOk, "gesperrtes Feld");
            Assert.IsFalse(BuildLogic.Place(s, cfg, pump, g.cols, 0).IsOk, "außerhalb");
            Assert.IsTrue(BuildLogic.Place(s, cfg, pump, 0, 0).IsOk);
            Assert.IsFalse(BuildLogic.Place(s, cfg, pump, 0, 0).IsOk, "Feld belegt");
            s.SetMoney(0f);
            Assert.IsFalse(BuildLogic.Place(s, cfg, pump, 1, 0).IsOk, "zu wenig Geld");
        }

        [Test]
        public void SellRefundsPartOfPrice()
        {
            var s = GameState.Create(cfg);
            s.SetMoney(10000f);
            var pump = BuildLogic.Type(cfg, BuildLogic.Pump);
            BuildLogic.Place(s, cfg, pump, 2, 2);
            float money = s.Money;
            Assert.IsNotNull(BuildLogic.Sell(s, cfg, 2, 2));
            Assert.AreEqual(money + Mathf.Round(pump.price * cfg.buildMachines.sellRefund), s.Money, 0.001f);
            Assert.AreEqual(0, s.machines.Count);
            Assert.IsNull(BuildLogic.Sell(s, cfg, 2, 2));
        }

        [Test]
        public void CellLookupMatchesCenters()
        {
            for (int gx = 0; gx < cfg.buildMachines.grid.cols; gx++)
            for (int gz = 0; gz < cfg.buildMachines.grid.rows; gz++)
            {
                var c = BuildLogic.CellCenter(cfg, gx, gz);
                Assert.IsTrue(BuildLogic.CellAt(cfg, c + new Vector2(0.4f, -0.4f), out int x, out int z));
                Assert.AreEqual((gx, gz), (x, z));
            }
        }

        [Test]
        public void MachinesWork()
        {
            var s = GameState.Create(cfg);
            s.SetMoney(100000f);
            s.planIdx = cfg.day.plans - 1;
            var pump = BuildLogic.Type(cfg, BuildLogic.Pump);
            var cool = BuildLogic.Type(cfg, BuildLogic.Cool);
            BuildLogic.Place(s, cfg, pump, 0, 1);
            BuildLogic.Place(s, cfg, pump, 1, 1);
            BuildLogic.Place(s, cfg, cool, 2, 1);
            Assert.AreEqual(2 * pump.milkPerSec, BuildLogic.PumpRate(s, cfg, true), 0.001f);
            Assert.AreEqual(0f, BuildLogic.PumpRate(s, cfg, false), "nur bei stehender Kuh");
            s.blackoutTimer = 1f;
            Assert.AreEqual(2 * pump.milkPerSec * cfg.risks.blackoutProductionFactor, BuildLogic.PumpRate(s, cfg, true), 0.001f);
            Assert.AreEqual(cool.heatPerSec, BuildLogic.CoolRate(s, cfg), 0.001f);

            s.antidote = 1;
            Assert.IsTrue(BuildLogic.DocNeeded(s, AnimalState.Unstable, CrisisKind.None));
            Assert.IsTrue(BuildLogic.DocNeeded(s, AnimalState.Ok, CrisisKind.Acid));
            Assert.IsFalse(BuildLogic.DocNeeded(s, AnimalState.Ok, CrisisKind.Escape));
            s.antidote = 0;
            Assert.IsFalse(BuildLogic.DocNeeded(s, AnimalState.Unstable, CrisisKind.None), "ohne Gegenmittel");

            s.serums.Add(new Serum(new Dose(1, 1, 0), SerumStage.Ready));
            s.lastSprayed = new Serum(new Dose(1, 1, 0), SerumStage.Ready);
            Assert.AreEqual(0, BuildLogic.FindLastSprayed(s));
            s.lastSprayed = new Serum(new Dose(2, 1, 0), SerumStage.Ready);
            Assert.AreEqual(-1, BuildLogic.FindLastSprayed(s));
        }
    }
}
