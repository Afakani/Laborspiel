using System.Collections.Generic;
using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Risiko-System (GDD 5.2/5.3) und Infektion (GDD 8) ohne Szene.</summary>
    public class RiskTests
    {
        GameConfig cfg;
        readonly List<RiskEvent> events = new List<RiskEvent>();

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        AnimalStats Animal(int a, int b, int c)
        {
            var s = new AnimalStats();
            s.Reset(cfg.animal.eggInterval);
            s.kind = AnimalKind.Cow;
            s.dose = new Dose(a, b, c);
            return s;
        }

        /// <summary>Simuliert seconds Sekunden in Schritten von dt und sammelt alle Ereignisse.</summary>
        List<RiskEvent> Run(AnimalStats s, GameState g, float seconds, float dt = 0.1f, float roll = 1f)
        {
            var all = new List<RiskEvent>();
            var state = AnimalState.Ok;
            for (float t = 0f; t < seconds; t += dt)
            {
                events.Clear();
                RiskLogic.Step(s, state, g, cfg, dt, false, () => roll, events);
                all.AddRange(events);
                if (events.Contains(RiskEvent.Unstable)) state = AnimalState.Unstable;
                if (events.Contains(RiskEvent.FlyAway)) break;
            }
            return all;
        }

        [Test]
        public void StableAnimalHasNoRisks()
        {
            var s = Animal(1, 0, 1);
            Assert.GreaterOrEqual(s.BaseStability(false, cfg), cfg.risks.activeBelowStab);
            var g = GameState.Create(cfg);
            var ev = Run(s, g, 30f, 0.1f, 0f);
            Assert.AreEqual(0f, s.heat);
            Assert.AreEqual(0f, s.acidTimer);
            Assert.AreEqual(0f, s.floatHeight);
            Assert.IsFalse(g.beltBroken);
            CollectionAssert.DoesNotContain(ev, RiskEvent.Unstable);
        }

        [Test]
        public void HeatRisesWithGrowth()
        {
            var s = Animal(2, 2, 0);
            float s0 = s.BaseStability(false, cfg);
            Assert.Less(s0, cfg.risks.activeBelowStab);
            var g = GameState.Create(cfg);
            events.Clear();
            RiskLogic.Step(s, AnimalState.Ok, g, cfg, 1f, false, () => 1f, events);
            Assert.AreEqual(2 * cfg.risks.heatPerA * (1f - s0 / 100f), s.heat, 0.001f);
        }

        [Test]
        public void TwoTwoZeroBecomesUnstableWithWarningsOnce()
        {
            var s = Animal(2, 2, 0);
            var g = GameState.Create(cfg);
            var ev = Run(s, g, 120f);
            Assert.AreEqual(1, ev.FindAll(e => e == RiskEvent.Unstable).Count, "genau einmal INSTABIL");
            Assert.LessOrEqual(ev.FindAll(e => e == RiskEvent.WarnHeat).Count, 1);
            Assert.LessOrEqual(ev.FindAll(e => e == RiskEvent.WarnStab).Count, 1);
            Assert.AreEqual(cfg.risks.unstableCountdownSec, s.unstableTimer, 0.001f);
        }

        [Test]
        public void FloatingAwayEndsTheAnimal()
        {
            var s = Animal(0, 4, 0);
            Assert.Less(s.BaseStability(false, cfg), cfg.risks.activeBelowStab);
            s.floatHeight = cfg.risks.floatEscape - 0.01f;
            var ev = Run(s, GameState.Create(cfg), 5f);
            CollectionAssert.Contains(ev, RiskEvent.FlyAway);
        }

        [Test]
        public void HeavyAnimalBreaksBelt()
        {
            var s = Animal(3, 0, 0);
            var g = GameState.Create(cfg);
            events.Clear();
            RiskLogic.Step(s, AnimalState.Ok, g, cfg, 0.1f, false, () => 0f, events);
            Assert.IsTrue(g.beltBroken);
            CollectionAssert.Contains(events, RiskEvent.BeltBroken);
        }

        [Test]
        public void UltimateHasNoRisks()
        {
            var s = Animal(0, 0, 0);
            s.ultimate = cfg.ultimates[0];
            s.dose = s.ultimate.Recipe;
            var g = GameState.Create(cfg);
            Run(s, g, 30f, 0.1f, 0f);
            Assert.AreEqual(0f, s.heat);
            Assert.IsFalse(g.beltBroken);
            Assert.AreEqual(0f, g.blackoutTimer);
        }

        // ---------- Infektion ----------

        [Test]
        public void InfectionOnlyFromDayTwoAndWhenHit()
        {
            var g = GameState.Create(cfg);
            var sym = new InfectionLogic.Symptoms();
            g.dayNum = cfg.risks.infectionFromDay - 1;
            Assert.AreEqual(InfectionLogic.InfectResult.NotYet, InfectionLogic.OnExplosion(g, cfg, 1f, 0f, sym));
            g.dayNum = cfg.risks.infectionFromDay;
            Assert.AreEqual(InfectionLogic.InfectResult.Missed, InfectionLogic.OnExplosion(g, cfg, cfg.risks.infectionDistance + 1f, 0.99f, sym));
            Assert.AreEqual(InfectionLogic.InfectResult.Infected, InfectionLogic.OnExplosion(g, cfg, 2f, 0.99f, sym));
            Assert.IsTrue(g.infected);
            Assert.AreEqual(g.dayNum + cfg.risks.infectionDays - 1, g.infectedUntilDay);
            Assert.AreEqual(cfg.infection.levelStart, g.infectionLevel, 0.0001f);
            Assert.AreEqual(InfectionLogic.InfectResult.More, InfectionLogic.OnExplosion(g, cfg, 2f, 0f, sym));
            Assert.AreEqual(cfg.infection.levelStart + cfg.infection.levelMore, g.infectionLevel, 0.0001f);
        }

        [Test]
        public void SymptomsComeAndGo()
        {
            var g = GameState.Create(cfg);
            var sym = new InfectionLogic.Symptoms();
            g.dayNum = cfg.risks.infectionFromDay;
            InfectionLogic.OnExplosion(g, cfg, 0f, 0f, sym);
            Assert.IsNull(InfectionLogic.Tick(g, cfg, sym, cfg.infection.firstSymptomSec - 0.5f, () => 0f));
            string started = InfectionLogic.Tick(g, cfg, sym, 1f, () => 0f);
            Assert.AreEqual(cfg.infection.symptoms[0], started);
            InfectionLogic.Tick(g, cfg, sym, cfg.infection.symptomDurationSec[0] + 0.1f, () => 0f);
            Assert.IsNull(sym.current);
        }

        [Test]
        public void AntidoteCuresAndCollapseAfterDeadline()
        {
            var g = GameState.Create(cfg);
            var sym = new InfectionLogic.Symptoms();
            Assert.IsFalse(InfectionLogic.SelfInject(g, sym).IsOk, "nicht infiziert");
            g.dayNum = cfg.risks.infectionFromDay;
            InfectionLogic.OnExplosion(g, cfg, 0f, 0f, sym);
            g.antidote = 0;
            Assert.AreEqual("ui.errors.noAntidoteBuy", InfectionLogic.SelfInject(g, sym).key);
            g.antidote = 1;
            Assert.IsTrue(InfectionLogic.SelfInject(g, sym).IsOk);
            Assert.IsFalse(g.infected);
            Assert.AreEqual(0, g.antidote);

            InfectionLogic.OnExplosion(g, cfg, 0f, 0f, sym);
            Assert.IsFalse(InfectionLogic.TryCollapse(g, cfg, sym), "Frist noch nicht vorbei");
            g.dayNum = g.infectedUntilDay;
            float anger = g.anger;
            Assert.IsTrue(InfectionLogic.TryCollapse(g, cfg, sym));
            Assert.IsFalse(g.infected);
            Assert.AreEqual(anger + cfg.anger.collapse, g.anger, 0.0001f);
        }
    }
}
