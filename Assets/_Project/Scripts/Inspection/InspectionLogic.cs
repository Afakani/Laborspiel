using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Parteikontrolle und Kommissar (GDD 9, Prototyp updatePlay/startInspection/offerVodka/rackSuspicious).
    /// Reine Regeln ohne Unity-Objekte; Zeitbasis ist GameState.playTime. Alle Werte aus authority/inspection.
    /// </summary>
    public static class InspectionLogic
    {
        // ---------- Parteikontrolle ----------

        /// <summary>Anstieg der Parteikontrolle pro s ohne den Abbau (authority.decay).</summary>
        public static float AuthorityGain(in ChamberView v, bool covered, bool music, bool contaminated, GameConfig cfg)
        {
            var a = cfg.authority;
            float gain = 0f;
            if (v.mutated)
            {
                float m = a.@base
                          + (v.stability < cfg.risks.activeBelowStab ? a.lowStab : 0f)
                          + (v.floatHeight > a.floatingFrom ? a.floating : 0f)
                          + v.glow * a.glowFactor
                          + (v.IsOmega ? a.omega : v.IsKoloss ? a.koloss : 0f);
                gain += m * (covered ? a.clothFactor : 1f) * (music ? a.musicFactor : 1f);
            }
            if (v.Singing && v.ok) gain += a.singingCow;
            if (contaminated) gain += a.contaminated;
            return gain;
        }

        // ---------- Ablauf ----------

        /// <summary>Schichtbeginn: am ersten Kontrolltag die erste Kontrolle nach festem Abstand einplanen.</summary>
        public static void OnDayStart(GameState s, GameConfig cfg)
        {
            if (s.dayNum == cfg.inspection.fromDay)
                s.nextInspectionAt = s.playTime + cfg.inspection.firstAfterDay2StartSec;
        }

        /// <summary>Erstes Ultimativ: der Oberkommissar kommt nach bossAfterUltSec. false = war schon da.</summary>
        public static bool ScheduleBoss(GameState s, GameConfig cfg)
        {
            if (s.bossDone) return false;
            s.bossAt = s.playTime + cfg.inspection.bossAfterUltSec;
            return true;
        }

        static bool BossDue(GameState s) => s.bossAt > 0f && !s.bossDone && s.playTime >= s.bossAt;

        /// <summary>Soll jetzt eine Kontrolle beginnen?</summary>
        public static bool ShouldStart(GameState s, GameConfig cfg)
        {
            var c = cfg.inspection;
            return s.inspection == null && s.dayNum >= c.fromDay
                   && (s.authority >= c.triggerAuthority || s.playTime >= s.nextInspectionAt || BossDue(s))
                   && s.playTime - s.inspectionDoneAt > c.minGapSec;
        }

        /// <summary>Kontrolle beginnen (Kommissar tritt rechts ein).</summary>
        public static Inspection Start(GameState s, GameConfig cfg)
        {
            s.inspection = new Inspection { x = cfg.inspector.startX, boss = BossDue(s) };
            s.inspectionAnnounced = false;
            return s.inspection;
        }

        /// <summary>Stehen verdächtige Seren offen im Regal (ultimativ, roh/zentrifugiert oder A+B hoch)?</summary>
        public static bool RackSuspicious(GameState s, GameConfig cfg)
        {
            foreach (var serum in s.serums)
                if (serum.IsUltimate || serum.stage != SerumStage.Ready || serum.dose.A + serum.dose.B >= cfg.inspection.susp.rackSuspiciousAB)
                    return true;
            return false;
        }

        /// <summary>Verdacht pro s am Tier, wenn der Kommissar in Reichweite der Kammer ist.</summary>
        public static float AnimalSuspicion(in ChamberView v, bool covered, GameConfig cfg)
        {
            if (!v.mutated) return 0f;
            var c = cfg.inspection.susp;
            if (!covered) return v.IsKoloss ? c.uncoveredKoloss : c.uncovered;
            if (v.IsKoloss) return c.coveredKoloss;
            if (v.IsOmega) return c.coveredOmega;
            return c.coveredBase + v.Level(c) * c.coveredPerDose + (v.unstable ? c.coveredUnstable : 0f);
        }

        /// <summary>Multiplikator für den Verdacht (Oberkommissar, Musik, Singende Kuh).</summary>
        public static float SuspicionMult(bool boss, bool music, in ChamberView v, GameConfig cfg)
        {
            var m = cfg.inspection.mult;
            return (boss ? m.boss : 1f) * (music ? m.music : 1f) * (v.Singing ? m.singingCow : 1f);
        }

        /// <summary>
        /// Ein Schritt: Parteikontrolle, Start, Gang, Verdacht, Ende. passed enthält die eben bestandene Kontrolle.
        /// chamberX und mixingX in Prototyp-Koordinaten.
        /// </summary>
        public static void Step(GameState s, GameConfig cfg, float dt, in ChamberView v, bool music,
            float chamberX, float mixingX, List<InspectionEvent> events, out Inspection passed)
        {
            passed = null;
            var a = cfg.authority;
            var c = cfg.inspection;
            s.AddAuthority((AuthorityGain(v, s.chamberCovered, music, s.contaminated, cfg) - a.decay) * dt, cfg);

            if (ShouldStart(s, cfg))
            {
                Start(s, cfg);
                events.Add(InspectionEvent.Started);
            }
            var ins = s.inspection;
            if (ins == null) return;

            if (ins.pause > 0f) ins.pause -= dt;
            else
            {
                ins.x -= c.speed * dt;
                float mult = SuspicionMult(ins.boss, music, v, cfg);
                if (Mathf.Abs(ins.x - chamberX) < c.range)
                {
                    ins.suspicion += AnimalSuspicion(v, s.chamberCovered, cfg) * mult * dt;
                    if (s.contaminated) ins.suspicion += c.susp.contaminated * mult * dt;
                }
                if (Mathf.Abs(ins.x - mixingX) < c.susp.rackRange && !s.rackHidden && RackSuspicious(s, cfg))
                {
                    ins.suspicion += c.susp.rack * mult * dt;
                    if (!ins.rackWarned)
                    {
                        ins.rackWarned = true;
                        events.Add(InspectionEvent.RackWarning);
                    }
                }
            }

            if (ins.suspicion >= c.suspMax)
            {
                ins.suspicion = c.suspMax;
                events.Add(InspectionEvent.Scandal);
                return;
            }
            if (ins.x < cfg.inspector.endX)
            {
                Pass(s, cfg);
                passed = ins;
                events.Add(InspectionEvent.Passed);
            }
        }

        /// <summary>Verschwiegenheitsprämie der Kontrolle.</summary>
        public static float Prize(bool boss, GameConfig cfg) => cfg.inspection.prize * (boss ? cfg.inspection.bossPrizeMult : 1f);

        /// <summary>Kontrolle bestanden: Prämie, Wut sinkt, Parteikontrolle zurückgesetzt, nächste Kontrolle geplant.</summary>
        public static void Pass(GameState s, GameConfig cfg)
        {
            var ins = s.inspection;
            if (ins == null) return;
            float prize = Prize(ins.boss, cfg);
            s.AddMoney(prize);
            s.earned += prize;
            s.AddAnger(cfg.anger.inspectionPassed, cfg);
            s.authority = cfg.authority.afterInspection;
            s.inspection = null;
            s.inspectionDoneAt = s.playTime;
            s.nextInspectionAt = s.playTime + cfg.inspection.gapSec;
            if (ins.boss)
            {
                s.bossDone = true;
                s.bossAt = 0f;
            }
            s.newsFacts.Add(ins.boss ? "newsFacts.bossInspection" : "newsFacts.inspection");
        }

        /// <summary>Wodka anbieten: Pause und weniger Verdacht.</summary>
        public static Feedback OfferVodka(GameState s, GameConfig cfg)
        {
            var ins = s.inspection;
            if (ins == null || ins.Drinking) return Feedback.Error("ui.prompts.vodkaDrinking");
            if (s.vodka <= 0) return Feedback.Error("ui.errors.noVodka");
            s.vodka--;
            ins.pause = cfg.inspection.vodka.pauseSec;
            ins.suspicion = Mathf.Max(0f, ins.suspicion - cfg.inspection.vodka.suspReduce);
            s.NotifyInventoryChanged();
            return Feedback.Ok;
        }
    }
}
