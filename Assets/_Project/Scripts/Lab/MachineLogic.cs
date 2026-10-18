using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Zentrifuge (GDD 6.3) und Kryokammer (GDD 6.4) als reine Regeln (Prototyp zentriLoad/zentriStop/zentriBreak,
    /// kryoLoad/kryoTake, updateMachines). Alle Werte aus machinesFixed.
    /// </summary>
    public static class MachineLogic
    {
        // ---------- Zentrifuge ----------

        /// <summary>Rohserum oder normales, noch nicht konzentriertes Serum.</summary>
        public static bool CanCentrifuge(Serum s) =>
            s.stage == SerumStage.Raw || (!s.IsUltimate && !s.IsConcentrated && s.stage == SerumStage.Ready);

        public static int FindForCentrifuge(GameState s) => s.serums.FindIndex(CanCentrifuge);

        /// <summary>Erstes passendes Serum aus dem Regal laden; die Drehzahl steigt ab jetzt.</summary>
        public static Feedback LoadCentrifuge(GameState s)
        {
            if (s.centrifuge != null) return Feedback.Error("ui.prompts.centrifugeSpinDown");
            int i = FindForCentrifuge(s);
            if (i < 0) return Feedback.Error("ui.errors.noCentrifugeSerum");
            var serum = s.serums[i];
            s.serums.RemoveAt(i);
            s.selectedSerum = Mathf.Clamp(s.selectedSerum, 0, Mathf.Max(0, s.serums.Count - 1));
            s.centrifuge = new CentrifugeJob { serum = serum };
            return Feedback.Ok;
        }

        /// <summary>Liegt die Drehzahl in der grünen Zone?</summary>
        public static bool InGreen(float rpm, GameConfig cfg)
        {
            var g = cfg.machinesFixed.centrifuge.zones.green;
            return rpm >= g[0] && rpm <= g[1];
        }

        /// <summary>Bremsen: Ergebnis je nach Drehzahl; danach läuft der Rotor aus.</summary>
        public static CentrifugeResult Brake(GameState s, GameConfig cfg)
        {
            var job = s.centrifuge;
            var c = cfg.machinesFixed.centrifuge;
            job.spinning = false;
            float r = job.rpm;
            if (r < c.zones.tooEarly) return CentrifugeResult.TooEarly;
            bool perfect = InGreen(r, cfg);
            if (job.serum.stage == SerumStage.Raw)
            {
                job.serum.stage = SerumStage.Centrifuged;
                return perfect ? CentrifugeResult.CentrifugedPerfect : CentrifugeResult.Centrifuged;
            }
            job.serum.conc = perfect ? c.concGreen : c.concYellow;
            return perfect ? CentrifugeResult.ConcentratedPerfect : CentrifugeResult.Concentrated;
        }

        // ---------- Kryokammer ----------

        /// <summary>Zentrifugiertes oder normales, noch nicht tiefgekühltes Serum.</summary>
        public static bool CanFreeze(Serum s) =>
            s.stage == SerumStage.Centrifuged || (!s.IsUltimate && !s.frozen && s.stage == SerumStage.Ready);

        public static int FindForCryo(GameState s) => s.serums.FindIndex(CanFreeze);

        public static Feedback LoadCryo(GameState s, GameConfig cfg)
        {
            var c = cfg.machinesFixed.cryo;
            if (s.planIdx < c.unlockPlanIdx) return Feedback.Error("ui.prompts.cryoLocked", ("n", c.unlockPlanIdx + 1));
            if (s.cryo != null) return Feedback.Error("ui.prompts.cryoFreezing", ("sec", Mathf.CeilToInt(s.cryo.freezeLeft)));
            int i = FindForCryo(s);
            if (i < 0) return Feedback.Error("ui.errors.noCryoSerum");
            var serum = s.serums[i];
            s.serums.RemoveAt(i);
            s.selectedSerum = Mathf.Clamp(s.selectedSerum, 0, Mathf.Max(0, s.serums.Count - 1));
            s.cryo = new CryoJob { serum = serum, freezeLeft = c.freezeSec, windowLeft = c.takeWindowSec };
            return Feedback.Ok;
        }

        /// <summary>Herausnehmen im Zeitfenster. Ab Plan 3 wird zentrifugiertes Rohserum ultimativ.</summary>
        public static Feedback TakeCryo(GameState s, GameConfig cfg, out CryoResult result)
        {
            result = CryoResult.Frozen;
            var job = s.cryo;
            if (job == null || !job.Ready) return Feedback.Error("ui.errors.noCryoSerum");
            if (s.serums.Count >= cfg.mixing.rackSize) return Feedback.Error("ui.errors.rackFullShort");
            var serum = job.serum;
            if (serum.stage == SerumStage.Centrifuged)
            {
                serum.stage = SerumStage.Ready;
                var ult = cfg.UltimateForRecipe(serum.dose);
                if (s.planIdx >= cfg.machinesFixed.ultimate.unlockPlanIdx && ult != null)
                {
                    serum.ultimateId = ult.id;
                    result = CryoResult.Ultimate;
                }
                else
                {
                    serum.frozen = true;
                    result = CryoResult.NoReaction;
                }
            }
            else serum.frozen = true;
            s.serums.Add(serum);
            s.cryo = null;
            return Feedback.Ok;
        }

        // ---------- Zeit ----------

        /// <summary>Drehzahl, Auslauf, Einfrieren und Zeitfenster weiterrechnen.</summary>
        public static void Tick(GameState s, GameConfig cfg, float dt, List<MachineEvent> events)
        {
            var c = cfg.machinesFixed.centrifuge;
            var z = s.centrifuge;
            if (z != null)
            {
                if (z.spinning)
                {
                    z.rpm += dt * (c.rpmAccelBase + c.rpmAccelPerRpm * z.rpm);
                    if (z.rpm >= 1f)
                    {
                        s.centrifuge = null;
                        events.Add(MachineEvent.CentrifugeOverspeed);
                    }
                }
                else
                {
                    z.rpm = Mathf.Max(0f, z.rpm - dt * c.spinDown);
                    if (z.rpm <= 0f && s.serums.Count < cfg.mixing.rackSize)
                    {
                        s.serums.Add(z.serum);
                        s.centrifuge = null;
                        events.Add(MachineEvent.CentrifugeReturned);
                    }
                }
            }

            var k = s.cryo;
            if (k != null)
            {
                if (k.freezeLeft > 0f)
                {
                    k.freezeLeft -= dt;
                    if (k.freezeLeft <= 0f)
                    {
                        k.freezeLeft = 0f;
                        events.Add(MachineEvent.CryoReady);
                    }
                }
                else
                {
                    k.windowLeft -= dt;
                    if (k.windowLeft <= 0f)
                    {
                        s.cryo = null;
                        events.Add(MachineEvent.CryoIceBlock);
                    }
                }
            }
        }
    }
}
