using System.Collections.Generic;
using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Zentrifuge, Kryokammer und Herstellung ultimativer Seren (GDD 6.3, 6.4, 4.5).</summary>
    public class MachineTests
    {
        GameConfig cfg;
        readonly List<MachineEvent> events = new List<MachineEvent>();

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        GameState WithSerum(Dose d, SerumStage stage, int planIdx = 0)
        {
            var s = GameState.Create(cfg);
            s.planIdx = planIdx;
            s.serums.Add(new Serum(d, stage));
            return s;
        }

        /// <summary>Zentrifuge bis zur Drehzahl r hochlaufen lassen.</summary>
        void SpinTo(GameState s, float r)
        {
            while (s.centrifuge != null && s.centrifuge.rpm < r)
                MachineLogic.Tick(s, cfg, 0.01f, events);
        }

        float Mid(float[] zone) => (zone[0] + zone[1]) * 0.5f;

        [Test]
        public void RpmFollowsFormula()
        {
            var s = WithSerum(new Dose(1, 0, 0), SerumStage.Ready);
            Assert.IsTrue(MachineLogic.LoadCentrifuge(s).IsOk);
            Assert.AreEqual(0, s.serums.Count);
            MachineLogic.Tick(s, cfg, 1f, events);
            Assert.AreEqual(cfg.machinesFixed.centrifuge.rpmAccelBase, s.centrifuge.rpm, 0.0001f);
        }

        [Test]
        public void BrakeZonesConcentrateNormalSerum()
        {
            var c = cfg.machinesFixed.centrifuge;
            var s = WithSerum(new Dose(1, 0, 0), SerumStage.Ready);
            MachineLogic.LoadCentrifuge(s);
            SpinTo(s, Mid(c.zones.green));
            Assert.AreEqual(CentrifugeResult.ConcentratedPerfect, MachineLogic.Brake(s, cfg));
            Assert.AreEqual(c.concGreen, s.centrifuge.serum.conc, 0.001f);

            var s2 = WithSerum(new Dose(1, 0, 0), SerumStage.Ready);
            MachineLogic.LoadCentrifuge(s2);
            SpinTo(s2, Mid(c.zones.yellow));
            Assert.AreEqual(CentrifugeResult.Concentrated, MachineLogic.Brake(s2, cfg));
            Assert.AreEqual(c.concYellow, s2.centrifuge.serum.conc, 0.001f);

            var s3 = WithSerum(new Dose(1, 0, 0), SerumStage.Ready);
            MachineLogic.LoadCentrifuge(s3);
            SpinTo(s3, c.zones.tooEarly * 0.5f);
            Assert.AreEqual(CentrifugeResult.TooEarly, MachineLogic.Brake(s3, cfg));
            Assert.IsFalse(s3.centrifuge.serum.IsConcentrated);
        }

        [Test]
        public void RawSerumBecomesCentrifugedAndReturnsAfterSpinDown()
        {
            var s = WithSerum(new Dose(2, 3, 5), SerumStage.Raw);
            MachineLogic.LoadCentrifuge(s);
            SpinTo(s, Mid(cfg.machinesFixed.centrifuge.zones.red));
            Assert.AreEqual(CentrifugeResult.Centrifuged, MachineLogic.Brake(s, cfg));
            events.Clear();
            for (int i = 0; i < 400 && s.centrifuge != null; i++) MachineLogic.Tick(s, cfg, 0.05f, events);
            CollectionAssert.Contains(events, MachineEvent.CentrifugeReturned);
            Assert.AreEqual(SerumStage.Centrifuged, s.serums[0].stage);
            Assert.IsFalse(s.serums[0].IsConcentrated, "Rohserum wird nicht konzentriert");
        }

        [Test]
        public void SpinDownWaitsForRackSpace()
        {
            var s = WithSerum(new Dose(1, 0, 0), SerumStage.Ready);
            MachineLogic.LoadCentrifuge(s);
            SpinTo(s, 0.8f);
            MachineLogic.Brake(s, cfg);
            for (int i = 0; i < cfg.mixing.rackSize; i++) s.serums.Add(new Serum(new Dose(0, 1, 0), SerumStage.Ready));
            for (int i = 0; i < 100; i++) MachineLogic.Tick(s, cfg, 0.05f, events);
            Assert.IsNotNull(s.centrifuge, "Regal voll: Serum bleibt in der Zentrifuge");
            s.serums.RemoveAt(0);
            MachineLogic.Tick(s, cfg, 0.05f, events);
            Assert.IsNull(s.centrifuge);
        }

        [Test]
        public void OverspeedDestroysSerum()
        {
            var s = WithSerum(new Dose(1, 0, 0), SerumStage.Ready);
            MachineLogic.LoadCentrifuge(s);
            events.Clear();
            for (int i = 0; i < 2000 && s.centrifuge != null; i++) MachineLogic.Tick(s, cfg, 0.01f, events);
            CollectionAssert.Contains(events, MachineEvent.CentrifugeOverspeed);
            Assert.IsNull(s.centrifuge);
            Assert.AreEqual(0, s.serums.Count);
        }

        [Test]
        public void CryoLockedBeforePlanTwo()
        {
            var s = WithSerum(new Dose(1, 0, 0), SerumStage.Ready, cfg.machinesFixed.cryo.unlockPlanIdx - 1);
            Assert.IsFalse(MachineLogic.LoadCryo(s, cfg).IsOk);
            s.planIdx = cfg.machinesFixed.cryo.unlockPlanIdx;
            Assert.IsTrue(MachineLogic.LoadCryo(s, cfg).IsOk);
        }

        [Test]
        public void CryoFreezesThenWindowThenIceBlock()
        {
            var c = cfg.machinesFixed.cryo;
            var s = WithSerum(new Dose(1, 0, 0), SerumStage.Ready, c.unlockPlanIdx);
            MachineLogic.LoadCryo(s, cfg);
            Assert.IsFalse(MachineLogic.TakeCryo(s, cfg, out _).IsOk, "noch nicht fertig");
            events.Clear();
            MachineLogic.Tick(s, cfg, c.freezeSec + 0.01f, events);
            CollectionAssert.Contains(events, MachineEvent.CryoReady);
            MachineLogic.Tick(s, cfg, c.takeWindowSec - 1f, events);
            Assert.IsNotNull(s.cryo);
            MachineLogic.Tick(s, cfg, 1.1f, events);
            CollectionAssert.Contains(events, MachineEvent.CryoIceBlock);
            Assert.IsNull(s.cryo);
            Assert.AreEqual(0, s.serums.Count, "Eisblock: Serum verloren");
        }

        [Test]
        public void TakeFrozenNormalSerum()
        {
            var c = cfg.machinesFixed.cryo;
            var s = WithSerum(new Dose(1, 0, 0), SerumStage.Ready, c.unlockPlanIdx);
            MachineLogic.LoadCryo(s, cfg);
            MachineLogic.Tick(s, cfg, c.freezeSec + 0.01f, events);
            Assert.IsTrue(MachineLogic.TakeCryo(s, cfg, out var r).IsOk);
            Assert.AreEqual(CryoResult.Frozen, r);
            Assert.IsTrue(s.serums[0].frozen);
        }

        [Test]
        public void CentrifugedRecipeOnlyUltimateFromPlanThree()
        {
            var c = cfg.machinesFixed.cryo;
            var early = WithSerum(new Dose(2, 3, 5), SerumStage.Centrifuged, cfg.machinesFixed.ultimate.unlockPlanIdx - 1);
            MachineLogic.LoadCryo(early, cfg);
            MachineLogic.Tick(early, cfg, c.freezeSec + 0.01f, events);
            MachineLogic.TakeCryo(early, cfg, out var r1);
            Assert.AreEqual(CryoResult.NoReaction, r1);
            Assert.IsFalse(early.serums[0].IsUltimate);
            Assert.IsTrue(early.serums[0].frozen);
        }

        [Test]
        public void FullChainMakesOmega()
        {
            // Prompt-Test: Plan 3, 2-3-5 mischen → Zentrifuge grün → Kryo → Ω
            var s = GameState.Create(cfg);
            s.planIdx = cfg.machinesFixed.ultimate.unlockPlanIdx;
            s.substances[0] = 2;
            s.substances[1] = 3;
            s.substances[2] = 5;
            for (int i = 0; i < 3; i++) MixingLogic.SetDose(s, cfg, i, s.substances[i]);
            Assert.IsTrue(MixingLogic.TryStartMix(s, cfg).IsOk);
            Serum mixed = null;
            for (int i = 0; i < 1000 && mixed == null; i++) mixed = MixingLogic.Tick(s, cfg, 0.05f);
            Assert.AreEqual(SerumStage.Raw, mixed.stage, "Rezept brodelt als Rohserum");

            MachineLogic.LoadCentrifuge(s);
            SpinTo(s, Mid(cfg.machinesFixed.centrifuge.zones.green));
            Assert.AreEqual(CentrifugeResult.CentrifugedPerfect, MachineLogic.Brake(s, cfg));
            for (int i = 0; i < 400 && s.centrifuge != null; i++) MachineLogic.Tick(s, cfg, 0.05f, events);

            Assert.IsTrue(MachineLogic.LoadCryo(s, cfg).IsOk);
            MachineLogic.Tick(s, cfg, cfg.machinesFixed.cryo.freezeSec + 0.01f, events);
            Assert.IsTrue(MachineLogic.TakeCryo(s, cfg, out var r).IsOk);
            Assert.AreEqual(CryoResult.Ultimate, r);
            Assert.AreEqual(UltimateIds.Omega, s.serums[0].ultimateId);
        }
    }
}
