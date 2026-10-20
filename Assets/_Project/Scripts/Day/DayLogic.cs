using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Meta-Kreislauf ohne Szene (GDD 7, Prototyp startDay/endDay/updateBoss/newWish/updateWish/updateTutorial):
    /// Schichtbeginn mit Soll und Sonderauftrag, Quote mit Prämie, Sonderwünsche, Einführung, Schichtende mit
    /// Planfortschritt oder Wiederholung. Wut-Änderungen passieren hier, Meldungen macht der Aufrufer.
    /// </summary>
    public static class DayLogic
    {
        /// <summary>Ergebnis eines Schichtendes.</summary>
        public struct DayResult
        {
            public bool met;
            public bool collapse;
            /// <summary>Alle Pläne erfüllt → Spielende „final“.</summary>
            public bool final;
            /// <summary>Wut am Maximum → entlassen.</summary>
            public bool fired;
            public int planBefore;
        }

        // ---------- Schicht ----------

        public static void StartDay(GameState s, GameConfig cfg, Func<float> rnd)
        {
            s.dayNum++;
            var d = cfg.day;
            int p = Mathf.Clamp(s.planIdx, 0, d.targets.Length - 1);
            s.quota = new Quota
            {
                target = d.targets[p],
                left = d.dayLengthSec,
                duration = d.dayLengthSec,
                prize = d.prizes[p]
            };
            s.wish = null;
            s.contract = null;
            // Kantinen-Schub und Schutzkittel gelten nur eine Schicht
            s.mealBoost = 1f;
            s.coatOn = false;
            if (s.dayNum >= cfg.wishes.fromDay)
            {
                s.tutorialStep = -1;
                s.wishTimer = cfg.wishes.firstAfterSec;
            }
            if (s.dayNum >= cfg.contracts.fromDay && cfg.contracts.list.Length > 0)
            {
                int i = Mathf.Min(cfg.contracts.list.Length - 1, Mathf.FloorToInt(rnd() * cfg.contracts.list.Length));
                s.contract = new Contract { data = cfg.contracts.list[i] };
            }
        }

        /// <summary>Zeitschritt der Quote: Prämie bei Erfüllung, Meldung bei Ablauf.</summary>
        public static void Tick(GameState s, GameConfig cfg, float dt, List<DayEvent> events)
        {
            var q = s.quota;
            if (q == null) return;
            q.left -= dt;
            if (!q.met && q.Reached)
            {
                q.met = true;
                s.quotasMet++;
                s.AddAnger(cfg.anger.planMet, cfg);
                if (q.prize > 0f) s.AddMoney(q.prize);
                events.Add(DayEvent.PlanMet);
            }
            if (q.left <= 0f) events.Add(DayEvent.TimeUp);
        }

        /// <summary>Schichtende: Plan geschafft → nächster Plan, sonst Wut und Wiederholung. collapse = Infektionsfrist abgelaufen.</summary>
        public static DayResult EndDay(GameState s, GameConfig cfg, bool collapse)
        {
            var q = s.quota;
            var r = new DayResult { planBefore = s.planIdx, collapse = collapse, met = q != null && q.met && !collapse };
            if (q != null) s.meatTargetTotal += q.target.f;
            s.quota = null;
            s.wish = null;
            s.contract = null;
            if (r.met) s.planIdx++;
            else
            {
                s.AddAnger(cfg.anger.planFailed, cfg);
                s.quotasFailed++;
            }
            r.final = s.planIdx >= cfg.day.plans;
            r.fired = s.anger >= cfg.anger.max;
            return r;
        }

        /// <summary>Freischaltungen beim Erreichen eines Plan-Index (Prototyp checkUnlocks) als Textpfade mit Name.</summary>
        public static List<(string key, string name)> Unlocks(int planIdx, GameConfig cfg)
        {
            var list = new List<(string, string)>();
            foreach (var m in cfg.buildMachineTypes)
                if (m.unlockPlanIdx == planIdx && planIdx > 0) list.Add(("ui.toasts.unlockMachine", m.displayName));
            var f = cfg.machinesFixed;
            if (f.cryo.unlockPlanIdx == planIdx && planIdx > 0) list.Add(("ui.toasts.unlockCryo", null));
            if (f.tasting.unlockPlanIdx == planIdx && planIdx > 0) list.Add(("ui.toasts.unlockTasting", null));
            if (f.ultimate.unlockPlanIdx == planIdx && planIdx > 0) list.Add(("ui.toasts.unlockUltRumor", null));
            return list;
        }

        // ---------- Lieferung ----------

        /// <summary>
        /// Bezahlung einer Lieferung mit Quote (Übererfüllung), Sonderauftrag und Sonderwunsch. Liefert den Preis;
        /// newlyOver = Ware hat das Soll gerade überschritten.
        /// </summary>
        public static float Pay(GameState s, GameConfig cfg, char good, float tons, float quality, out bool newlyOver)
        {
            newlyOver = false;
            float pay = tons * Production.PayPerTon(cfg, good);
            var q = s.quota;
            if (q != null)
            {
                if (Quota.Get(q.prod, good) >= Quota.Get(q.target, good))
                {
                    pay *= 1f + cfg.day.overfulfilPayBonus;
                    int i = Quota.Index(good);
                    if (!q.over[i])
                    {
                        q.over[i] = true;
                        newlyOver = true;
                    }
                }
                Quota.Add(q.prod, good, tons);
            }
            var c = s.contract;
            if (c != null && c.Good == good && quality >= c.data.minQ)
            {
                pay *= cfg.contracts.payMult;
                c.got += tons;
            }
            var w = s.wish;
            if (w != null && ((w.Id == "meat" && good == 'f') || (w.Id == "eggs" && good == 'e'))) w.got += tons;
            return pay;
        }

        // ---------- Sonderwünsche ----------

        public static void TickWish(GameState s, GameConfig cfg, float dt, bool crisis, Func<float> rnd, List<DayEvent> events)
        {
            var c = cfg.wishes;
            if (s.dayNum < c.fromDay || s.quota == null) return;
            var w = s.wish;
            if (w == null)
            {
                s.wishTimer -= dt;
                if (s.wishTimer > 0f) return;
                s.wishTimer = Mathf.Lerp(c.intervalSec[0], c.intervalSec[1], rnd());
                int i = Mathf.Min(c.types.Length - 1, Mathf.FloorToInt(rnd() * c.types.Length));
                var t = c.types[i];
                s.wish = new Wish
                {
                    type = t,
                    need = t.id == "meat" || t.id == "eggs" ? t.amount.@base + t.amount.perPlan * s.planIdx : 1f,
                    left = t.durationSec
                };
                events.Add(DayEvent.WishNew);
                return;
            }
            w.left -= dt;
            if (w.Id == "calm" && crisis) w.ok = false;
            if ((w.Id == "meat" || w.Id == "eggs" || w.Id == "quality") && w.got >= w.need)
            {
                FinishWish(s, cfg, true, events);
                return;
            }
            if (w.left <= 0f) FinishWish(s, cfg, w.Id == "calm" && w.ok, events);
        }

        /// <summary>Verarbeitetes Tier für den Qualitäts-Wunsch melden.</summary>
        public static void OnProcessed(GameState s, float quality)
        {
            var w = s.wish;
            if (w != null && w.Id == "quality" && quality >= w.type.minQuality) w.got = 1f;
        }

        public static float WishReward(GameState s, GameConfig cfg) => cfg.wishes.reward.@base + cfg.wishes.reward.perPlan * s.planIdx;

        static void FinishWish(GameState s, GameConfig cfg, bool success, List<DayEvent> events)
        {
            s.wish = null;
            if (success)
            {
                s.AddMoney(WishReward(s, cfg));
                s.AddAnger(cfg.anger.wishDone, cfg);
                events.Add(DayEvent.WishDone);
            }
            else
            {
                s.AddAnger(cfg.anger.wishFailed, cfg);
                events.Add(DayEvent.WishFailed);
            }
        }

        // Einführung (Tag 1): siehe TutorialLogic

        // ---------- Leerlauf ----------

        /// <summary>Leerlauf-Wut (GDD 7.2): nach idleAfterSec ohne Bewegung oder Aktion steigt die Wut. Liefert true beim ersten Mal.</summary>
        public static bool TickIdle(ref float idleTimer, bool busy, GameState s, GameConfig cfg, float dt)
        {
            if (busy)
            {
                idleTimer = 0f;
                return false;
            }
            bool wasIdle = idleTimer > cfg.anger.idleAfterSec;
            idleTimer += dt;
            if (idleTimer <= cfg.anger.idleAfterSec) return false;
            s.AddAnger(cfg.anger.idlePerSec * dt, cfg);
            return !wasIdle;
        }
    }
}
