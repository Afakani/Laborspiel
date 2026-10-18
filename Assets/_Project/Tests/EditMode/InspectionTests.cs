using System.Collections.Generic;
using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Parteikontrolle, Kommissar, Laken, Serum-Regal, Wodka und Oberkommissar (GDD 9, 6.8).</summary>
    public class InspectionTests
    {
        GameConfig cfg;
        readonly List<InspectionEvent> events = new List<InspectionEvent>();

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        GameState Day2()
        {
            var s = GameState.Create(cfg);
            s.dayNum = cfg.inspection.fromDay;
            return s;
        }

        static ChamberView Mutated(int a, int b, int c, float stab = 80f) => new ChamberView
        {
            mutated = true, ok = true, stability = stab, doseTotal = a + b + c
        };

        float MixingX => cfg.layout.Station("mixing").x;

        /// <summary>Kontrolle laufen lassen, bis sie bestanden oder ein Skandal ist.</summary>
        InspectionEvent Run(GameState s, ChamberView v, float maxSec = 60f)
        {
            for (float t = 0f; t < maxSec; t += 0.05f)
            {
                events.Clear();
                s.playTime += 0.05f;
                InspectionLogic.Step(s, cfg, 0.05f, v, false, cfg.layout.chamber.x, MixingX, events, out _);
                if (events.Contains(InspectionEvent.Scandal)) return InspectionEvent.Scandal;
                if (events.Contains(InspectionEvent.Passed)) return InspectionEvent.Passed;
            }
            return InspectionEvent.Started;
        }

        [Test]
        public void AuthorityGainFollowsMutationAndCloth()
        {
            var a = cfg.authority;
            var v = Mutated(2, 1, 0, cfg.risks.activeBelowStab - 1f);
            float open = InspectionLogic.AuthorityGain(v, false, false, false, cfg);
            Assert.AreEqual(a.@base + a.lowStab, open, 0.001f);
            Assert.AreEqual(open * a.clothFactor, InspectionLogic.AuthorityGain(v, true, false, false, cfg), 0.001f);
            Assert.AreEqual(open * a.musicFactor, InspectionLogic.AuthorityGain(v, false, true, false, cfg), 0.001f);
            v.ultimateId = UltimateIds.Omega;
            Assert.AreEqual(open + a.omega, InspectionLogic.AuthorityGain(v, false, false, false, cfg), 0.001f);
            Assert.AreEqual(a.contaminated, InspectionLogic.AuthorityGain(default, false, false, true, cfg), 0.001f);
        }

        [Test]
        public void AuthorityDecaysWithoutAnimal()
        {
            var s = Day2();
            s.authority = 10f;
            InspectionLogic.Step(s, cfg, 1f, default, false, 0f, MixingX, events, out _);
            Assert.AreEqual(10f - cfg.authority.decay, s.authority, 0.001f);
        }

        [Test]
        public void FirstInspectionScheduledOnDayTwo()
        {
            var s = Day2();
            InspectionLogic.OnDayStart(s, cfg);
            Assert.AreEqual(cfg.inspection.firstAfterDay2StartSec, s.nextInspectionAt, 0.001f);
            s.playTime = s.nextInspectionAt - 1f;
            Assert.IsFalse(InspectionLogic.ShouldStart(s, cfg));
            s.playTime = s.nextInspectionAt + 0.1f;
            Assert.IsTrue(InspectionLogic.ShouldStart(s, cfg));
        }

        [Test]
        public void NoInspectionOnDayOneAndTriggerByAuthority()
        {
            var s = GameState.Create(cfg);
            s.dayNum = cfg.inspection.fromDay - 1;
            s.authority = cfg.authority.max;
            Assert.IsFalse(InspectionLogic.ShouldStart(s, cfg), "Tag 1 ohne Kontrolle");
            s.dayNum = cfg.inspection.fromDay;
            Assert.IsTrue(InspectionLogic.ShouldStart(s, cfg), "Parteikontrolle über der Schwelle");
        }

        [Test]
        public void MutatedAnimalWithoutClothIsScandal()
        {
            var s = Day2();
            InspectionLogic.Start(s, cfg);
            Assert.AreEqual(InspectionEvent.Scandal, Run(s, Mutated(1, 1, 0)));
        }

        [Test]
        public void ClothAndOneOneZeroPasses()
        {
            var s = Day2();
            s.chamberCovered = true;
            s.anger = 30f;
            float money = s.Money;
            InspectionLogic.Start(s, cfg);
            Assert.AreEqual(InspectionEvent.Passed, Run(s, Mutated(1, 1, 0)));
            Assert.IsNull(s.inspection);
            Assert.AreEqual(money + cfg.inspection.prize, s.Money, 0.001f);
            Assert.AreEqual(30f + cfg.anger.inspectionPassed, s.anger, 0.001f);
            Assert.AreEqual(cfg.authority.afterInspection, s.authority, 1f);
            Assert.AreEqual(s.playTime + cfg.inspection.gapSec, s.nextInspectionAt, 0.1f);
            Assert.IsFalse(InspectionLogic.ShouldStart(s, cfg), "Mindestabstand");
        }

        [Test]
        public void CoveredSuspicionGrowsWithDosesAndInstability()
        {
            var c = cfg.inspection.susp;
            var v = Mutated(1, 1, 0);
            Assert.AreEqual(c.coveredBase + 2 * c.coveredPerDose, InspectionLogic.AnimalSuspicion(v, true, cfg), 0.001f);
            v.unstable = true;
            Assert.AreEqual(c.coveredBase + 2 * c.coveredPerDose + c.coveredUnstable, InspectionLogic.AnimalSuspicion(v, true, cfg), 0.001f);
            v.doseTotal = 20;
            v.unstable = false;
            Assert.AreEqual(c.coveredBase + c.coveredMaxDose * c.coveredPerDose, InspectionLogic.AnimalSuspicion(v, true, cfg), 0.001f);
            v.ultimateId = UltimateIds.Koloss;
            Assert.AreEqual(c.uncoveredKoloss, InspectionLogic.AnimalSuspicion(v, false, cfg), 0.001f);
            Assert.AreEqual(0f, InspectionLogic.AnimalSuspicion(default, false, cfg), "unmutiert: kein Verdacht");
        }

        [Test]
        public void OpenSuspiciousRackRaisesSuspicionUnlessCovered()
        {
            var s = Day2();
            s.serums.Add(new Serum(new Dose(3, 2, 0), SerumStage.Ready));
            Assert.IsTrue(InspectionLogic.RackSuspicious(s, cfg), "A+B ≥ 5");
            InspectionLogic.Start(s, cfg).x = MixingX;
            events.Clear();
            InspectionLogic.Step(s, cfg, 1f, default, false, 0f, MixingX, events, out _);
            CollectionAssert.Contains(events, InspectionEvent.RackWarning);
            Assert.Greater(s.inspection.suspicion, 0f);

            var s2 = Day2();
            s2.serums.Add(new Serum(new Dose(3, 2, 0), SerumStage.Ready));
            s2.rackHidden = true;
            InspectionLogic.Start(s2, cfg).x = MixingX;
            InspectionLogic.Step(s2, cfg, 1f, default, false, 0f, MixingX, events, out _);
            Assert.AreEqual(0f, s2.inspection.suspicion, 0.001f);

            var s3 = Day2();
            s3.serums.Add(new Serum(new Dose(1, 1, 0), SerumStage.Ready));
            Assert.IsFalse(InspectionLogic.RackSuspicious(s3, cfg));
            s3.serums.Add(new Serum(new Dose(0, 1, 0), SerumStage.Raw));
            Assert.IsTrue(InspectionLogic.RackSuspicious(s3, cfg), "Rohserum");
        }

        [Test]
        public void VodkaPausesAndReducesSuspicion()
        {
            var s = Day2();
            s.vodka = 1;
            Assert.IsFalse(InspectionLogic.OfferVodka(s, cfg).IsOk, "ohne Kontrolle");
            var ins = InspectionLogic.Start(s, cfg);
            ins.suspicion = 50f;
            Assert.IsTrue(InspectionLogic.OfferVodka(s, cfg).IsOk);
            Assert.AreEqual(0, s.vodka);
            Assert.AreEqual(50f - cfg.inspection.vodka.suspReduce, ins.suspicion, 0.001f);
            Assert.AreEqual(cfg.inspection.vodka.pauseSec, ins.pause, 0.001f);
            float x = ins.x;
            InspectionLogic.Step(s, cfg, 1f, default, false, 0f, MixingX, events, out _);
            Assert.AreEqual(x, ins.x, 0.001f, "steht beim Trinken");
            ins.pause = 0f;
            Assert.IsFalse(InspectionLogic.OfferVodka(s, cfg).IsOk, "kein Wodka mehr");
        }

        [Test]
        public void BossComesAfterFirstUltimateAndPaysDouble()
        {
            var s = Day2();
            s.nextInspectionAt = float.PositiveInfinity;
            Assert.IsTrue(InspectionLogic.ScheduleBoss(s, cfg));
            s.playTime += cfg.inspection.bossAfterUltSec - 1f;
            Assert.IsFalse(InspectionLogic.ShouldStart(s, cfg));
            s.playTime += 2f;
            Assert.IsTrue(InspectionLogic.ShouldStart(s, cfg));
            Assert.IsTrue(InspectionLogic.Start(s, cfg).boss);
            float money = s.Money;
            Assert.AreEqual(InspectionEvent.Passed, Run(s, default));
            Assert.AreEqual(money + cfg.inspection.prize * cfg.inspection.bossPrizeMult, s.Money, 0.001f);
            Assert.IsTrue(s.bossDone);
            Assert.IsFalse(InspectionLogic.ScheduleBoss(s, cfg), "nur einmal");
        }
    }
}
