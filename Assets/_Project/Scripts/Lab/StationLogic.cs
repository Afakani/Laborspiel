using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Nebenstationen als reine Regeln (Prototyp crankGramophone, tasteAnimal, sendRohrpost, takeProbe):
    /// Grammophon, Verkostungsautomat, Rohrpost und Probenmikroskop. Werte aus machinesFixed und anger.
    /// </summary>
    public static class StationLogic
    {
        // ---------- Grammophon (GDD 6.5) ----------

        public static bool MusicPlaying(GameState s) => s.musicLeft > 0f;

        /// <summary>Kurbel fertig: Volksmusik für musicSec.</summary>
        public static Feedback Crank(GameState s, GameConfig cfg)
        {
            if (MusicPlaying(s)) return Feedback.Error("ui.prompts.gramophonePlaying", ("sec", Mathf.CeilToInt(s.musicLeft)));
            s.musicLeft = cfg.machinesFixed.gramophone.musicSec;
            return Feedback.Ok;
        }

        // ---------- Verkostungsautomat (GDD 6.6) ----------

        /// <summary>Siegel „STAATLICH GEPRÜFT“: kostet, Qualität +qualityBonus. animalOk = Tier steht in der Kammer.</summary>
        public static Feedback Taste(GameState s, GameConfig cfg, AnimalStats a, bool animalOk)
        {
            var t = cfg.machinesFixed.tasting;
            if (s.planIdx < t.unlockPlanIdx) return Feedback.Error("ui.prompts.tastingLocked", ("n", t.unlockPlanIdx + 1));
            if (!animalOk) return Feedback.Error("ui.errors.noTastingAnimal");
            if (a.stamped) return Feedback.Error("ui.errors.alreadyStamped");
            if (!s.TrySpend(t.cost)) return Feedback.Error("ui.errors.tastingFee", ("n", Mathf.RoundToInt(t.cost)));
            a.stamped = true;
            return Feedback.Ok;
        }

        /// <summary>Spruch-Liste nach Qualität (tasting.hi/mid/lo).</summary>
        public static string TastingList(float quality, GameConfig cfg)
        {
            var t = cfg.machinesFixed.tasting;
            return quality >= t.hiFrom ? "tasting.hi" : quality >= t.midFrom ? "tasting.mid" : "tasting.lo";
        }

        // ---------- Rohrpost (GDD 6.7) ----------

        /// <summary>Kaviar an den Direktor: Wut sinkt, Kapsel fliegt.</summary>
        public static Feedback SendCaviar(GameState s, GameConfig cfg)
        {
            if (s.caviar <= 0) return Feedback.Error("ui.errors.noCaviar");
            s.caviar--;
            s.AddAnger(cfg.anger.caviar, cfg);
            s.rohrpostLeft = cfg.machinesFixed.rohrpost.capsuleSec;
            s.NotifyInventoryChanged();
            return Feedback.Ok;
        }

        // ---------- Probenmikroskop (GDD 6.2) ----------

        /// <summary>Kann die aktuelle Mischung untersucht werden (unbekannt, noch nicht geprüft)?</summary>
        public static bool CanProbe(GameState s) =>
            s.mix.Total > 0 && !s.discoveredRecipes.Contains(s.mix.Key) && !s.probedRecipes.Contains(s.mix.Key);

        /// <summary>Probe der aktuellen Reglerstellung: kostet, Ergebnis nach analyseSec.</summary>
        public static Feedback TakeProbe(GameState s, GameConfig cfg)
        {
            var m = cfg.machinesFixed.microscope;
            if (s.mix.Total == 0) return Feedback.Error("ui.errors.mixSetSliders");
            if (!CanProbe(s)) return Feedback.Error("ui.mixing.probeRunning");
            if (!s.TrySpend(m.cost)) return Feedback.Error("ui.errors.probeFee", ("n", Mathf.RoundToInt(m.cost)));
            s.probedRecipes.Add(s.mix.Key);
            s.probeLeft = m.analyseSec;
            return Feedback.Ok;
        }

        // ---------- Zeit ----------

        /// <summary>Musik, Mikroskop und Rohrpost-Kapsel weiterzählen.</summary>
        public static void Tick(GameState s, float dt)
        {
            if (s.musicLeft > 0f) s.musicLeft = Mathf.Max(0f, s.musicLeft - dt);
            if (s.probeLeft > 0f) s.probeLeft = Mathf.Max(0f, s.probeLeft - dt);
            if (s.rohrpostLeft > 0f) s.rohrpostLeft = Mathf.Max(0f, s.rohrpostLeft - dt);
        }
    }
}
