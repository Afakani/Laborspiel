using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Risiko-System am lebenden Tier (GDD 5.2/5.3, Prototyp updateSubject): Hitze, Säure, Schweben, Bandriss,
    /// Kabelriss, Warnzeichen, Krise, Instabilität. Reine Logik ohne Szene, damit sie getestet werden kann.
    /// Die Kammer reagiert auf die gemeldeten Ereignisse (Meldungen, Zustandswechsel, Effekte).
    /// </summary>
    public static class RiskLogic
    {
        /// <summary>
        /// Ein Zeitschritt für ein Tier im Zustand Ok oder Unstable. rnd liefert Zufallszahlen in [0, 1).
        /// </summary>
        public static void Step(AnimalStats s, AnimalState state, GameState g, GameConfig cfg, float dt, bool music,
            Func<float> rnd, List<RiskEvent> events)
        {
            var r = cfg.risks;
            var w = cfg.warnings;
            int a = s.dose.A, b = s.dose.B;
            float s0 = s.BaseStability(music, cfg);
            bool active = !s.IsUltimate && s0 < r.activeBelowStab;
            float risk = s.IsUltimate ? 0f : 1f - s0 / 100f;

            // Hitze
            if (active && a >= r.heatMinA) s.heat = Mathf.Min(r.heatUnstable, s.heat + a * r.heatPerA * risk * dt);
            else s.heat = Mathf.Max(0f, s.heat - r.heatDecay * dt);
            if (music) s.heat = Mathf.Max(0f, s.heat - r.heatMusicDecay * dt);

            // Warnzeichen (je einmal pro Tier)
            if (s.heat > w.heat[0] && s.heat <= w.heat[1]) Warn(s, RiskWarnings.Heat, RiskEvent.WarnHeat, events);
            if (!s.acid && s.acidTimer > w.acidT) Warn(s, RiskWarnings.Acid, RiskEvent.WarnAcid, events);
            if (s.floatHeight > w.@float[0] && s.floatHeight <= w.@float[1]) Warn(s, RiskWarnings.Float, RiskEvent.WarnFloat, events);
            if (state == AnimalState.Ok && s.stability < w.stab) Warn(s, RiskWarnings.Stab, RiskEvent.WarnStab, events);

            // Säure (dauerhaft, sobald die Schwelle erreicht ist)
            if (active && b >= r.acidMinB && !s.acid)
            {
                s.acidTimer += dt * risk;
                if (s.acidTimer > r.acidThreshold)
                {
                    s.acid = true;
                    events.Add(RiskEvent.AcidMilk);
                }
            }

            // Schweben
            if (active && b >= r.floatMinB) s.floatHeight += r.floatPerB * (b - r.floatFromB) * risk * dt;
            else s.floatHeight = Mathf.Max(0f, s.floatHeight - r.floatDecay * dt);

            // Bandriss und Kabelriss (Zufall pro Sekunde)
            if (active && !g.beltBroken && a >= r.beltBreakMinA && rnd() < dt * r.beltBreakChancePerAPerSec * a)
            {
                g.beltBroken = true;
                events.Add(RiskEvent.BeltBroken);
            }
            if (!s.IsUltimate && g.blackoutTimer <= 0f && s0 < r.cableTearBelowStab && rnd() < dt * r.cableTearChancePerSec)
            {
                g.blackoutTimer = r.blackoutSec;
                events.Add(RiskEvent.CableTorn);
            }

            // Aktuelle Stabilität und Krise
            s.stability = Mathf.Clamp(s0 - s.heat * r.heatStabPenalty, 0f, cfg.formulas.statMax);
            s.minStability = Mathf.Min(s.minStability, s.stability);
            s.crisis = s.heat > r.heatCrisis ? CrisisKind.Overheat
                : s.acid ? CrisisKind.Acid
                : s.floatHeight > r.floatCrisis ? CrisisKind.Escape
                : CrisisKind.None;

            if (s.floatHeight > r.floatEscape)
            {
                events.Add(RiskEvent.FlyAway);
                return;
            }
            if (state == AnimalState.Ok && (s.stability <= 0f || s.heat >= r.heatUnstable))
            {
                s.unstableTimer = r.unstableCountdownSec;
                events.Add(RiskEvent.Unstable);
            }
        }

        static void Warn(AnimalStats s, RiskWarnings flag, RiskEvent e, List<RiskEvent> events)
        {
            if ((s.warned & flag) != 0) return;
            s.warned |= flag;
            events.Add(e);
        }
    }
}
