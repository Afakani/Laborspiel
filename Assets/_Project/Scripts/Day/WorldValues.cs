using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Spielwerte der Welt (GDD 7.7, Prototyp recalc): Hunger, Volk, Nahrung, Umwelt.</summary>
    public struct WorldValues
    {
        public float avgQuality;
        public float famine;
        public float pop;
        public float food;
        public float env;
        /// <summary>Fleisch in % über Plan seit Spielbeginn (Prototyp W.meat).</summary>
        public float meat;
        /// <summary>Durchschnittsqualität minus Startwert in Prozentpunkten (Prototyp W.quality).</summary>
        public float quality;

        public static WorldValues Compute(GameState s, GameConfig cfg)
        {
            var w = cfg.world;
            var v = new WorldValues();
            v.avgQuality = Production.AverageQuality(s, cfg);
            v.famine = Mathf.Clamp01((w.famineQ - v.avgQuality) / w.famineRange);
            v.pop = w.popMax * (1f - v.famine);
            var sw = w.supplyWeights;
            float supply = (s.totals.m * sw.m + s.totals.e * sw.e + s.totals.f * sw.f) / Mathf.Max(1f, s.dayNum * w.supplyPerDay);
            v.food = Mathf.Clamp(w.foodBase + w.foodPerSupply * supply, 0f, w.foodMax) * (1f - w.foodFamineFactor * v.famine);
            v.env = Mathf.Clamp(w.envMax - s.totalDoses * w.envPerDose - s.explosions * w.envPerExplosion, 0f, w.envMax);
            // Fleisch-Soll bis jetzt: abgeschlossene Tage plus anteilig der laufende Tag
            float fT = s.meatTargetTotal + (s.quota != null ? s.quota.target.f * (1f - s.quota.left / cfg.day.dayLengthSec) : 0f);
            v.meat = fT > 1f ? Mathf.Round((s.totals.f / fT - 1f) * 100f) : 0f;
            v.quality = Mathf.Round(v.avgQuality - w.avgQStart);
            return v;
        }

        /// <summary>Endungs-Schlüssel (GDD 13): bei „final“ nach Volk und verfehlten Tagen.</summary>
        public static string EndingKey(GameOverKind kind, GameState s, GameConfig cfg)
        {
            if (kind == GameOverKind.Fired) return "fired";
            if (kind == GameOverKind.Scandal) return "scandal";
            var v = Compute(s, cfg);
            var e = cfg.endings;
            if (v.pop >= e.heroMinPop && s.quotasFailed <= e.heroMaxFailedDays) return "hero_alive";
            if (v.pop < e.deadBelowPop) return "hero_dead";
            return "ok";
        }
    }
}
