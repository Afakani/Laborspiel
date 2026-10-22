using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Regeln der Mischstation (GDD 4.2, Prototyp startMix/finishMix). Arbeitet nur auf GameState und Config,
    /// damit sie ohne Szene testbar sind.
    /// </summary>
    public static class MixingLogic
    {
        /// <summary>Höchste einstellbare Dosis für Stoff i: Lagerbestand, höchstens sliderMax.</summary>
        public static int MaxDose(GameState s, GameConfig cfg, int i) => Mathf.Min(cfg.mixing.sliderMax, s.substances[i]);

        /// <summary>Regler setzen (begrenzt durch Lager).</summary>
        public static void SetDose(GameState s, GameConfig cfg, int i, int value)
        {
            int v = Mathf.Clamp(value, 0, MaxDose(s, cfg, i));
            var d = s.mix;
            s.mix = new Dose(i == 0 ? v : d.A, i == 1 ? v : d.B, i == 2 ? v : d.C);
        }

        /// <summary>Regler nachziehen, falls das Lager kleiner geworden ist.</summary>
        public static void ClampToStock(GameState s, GameConfig cfg)
        {
            for (int i = 0; i < 3; i++)
                if (s.mix[i] > MaxDose(s, cfg, i)) SetDose(s, cfg, i, s.mix[i]);
        }

        public static bool IsMixing(GameState s) => s.mixTimer > 0f;

        /// <summary>Kann jetzt gemischt werden (Knopf aktiv)?</summary>
        public static bool CanMix(GameState s, GameConfig cfg) =>
            s.mix.Total > 0 && !IsMixing(s) && s.serums.Count < cfg.mixing.rackSize;

        /// <summary>Mischen starten: Stoffe verbrauchen, Timer setzen, Regler leeren.</summary>
        public static Feedback TryStartMix(GameState s, GameConfig cfg)
        {
            var m = s.mix;
            if (IsMixing(s)) return Feedback.Error("ui.errors.mixBusy");
            if (m.Total == 0) return Feedback.Error("ui.errors.mixEmpty");
            // Einführung: nur die vorgegebene Mischung
            var need = TutorialLogic.RequiredMix(s, cfg);
            if (need.HasValue && m.Key != need.Value.Key)
                return Feedback.Error("ui.errors.tutorialMix", ("a", need.Value.A), ("b", need.Value.B), ("c", need.Value.C));
            if (s.serums.Count >= cfg.mixing.rackSize) return Feedback.Error("ui.errors.rackFull", ("n", cfg.mixing.rackSize));
            for (int i = 0; i < 3; i++)
                if (s.substances[i] < m[i]) return Feedback.Error("ui.errors.notEnough", ("code", cfg.substances[i].code));

            for (int i = 0; i < 3; i++) s.substances[i] -= m[i];
            s.mixPending = m;
            s.mixTimer = cfg.mixing.mixTimeSec;
            s.mix = Dose.Zero;
            return Feedback.Ok;
        }

        /// <summary>Zeit weiterlaufen lassen; gibt das fertige Serum zurück (sonst null).</summary>
        public static Serum Tick(GameState s, GameConfig cfg, float dt)
        {
            if (!IsMixing(s)) return null;
            s.mixTimer -= dt;
            if (s.mixTimer > 0f) return null;
            s.mixTimer = 0f;
            var d = s.mixPending ?? Dose.Zero;
            s.mixPending = null;
            var serum = new Serum(d, cfg.UltimateForRecipe(d) != null ? SerumStage.Raw : SerumStage.Ready);
            s.serums.Add(serum);
            return serum;
        }
    }
}
