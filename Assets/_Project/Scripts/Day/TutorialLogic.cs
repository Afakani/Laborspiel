namespace PlanErfuellt
{
    /// <summary>
    /// Geführte Einführung an Tag 1 (auf Wunsch ausgebaut): Schritte aus tutorial.steps, je mit Bedingung.
    /// Lese-Schritte (watch, cloth, finish) gehen nach tutorial.readSec weiter. Zustand: GameState.tutorialStep
    /// (-1 = aus) und tutorialTime (Zeit im aktuellen Schritt).
    /// </summary>
    public static class TutorialLogic
    {
        /// <summary>Was im Labor gerade los ist.</summary>
        public struct Context
        {
            public bool mixingOpen;
            public bool hasAnimal;
            public bool animalDosed;
        }

        public static bool Active(GameState s, GameConfig cfg) => s.tutorialStep >= 0 && s.tutorialStep < cfg.tutorial.steps.Length;

        public static TutorialStepConfig Current(GameState s, GameConfig cfg) => Active(s, cfg) ? cfg.tutorial.steps[s.tutorialStep] : null;

        public static bool Done(string id, GameState s, Context c, GameConfig cfg) => id switch
        {
            "walkMixing" => c.mixingOpen || s.serums.Count > 0 || c.animalDosed,
            "mix" => s.serums.Count > 0 || c.animalDosed,
            "clone" => c.hasAnimal,
            "spray" => c.animalDosed,
            "process" => s.totals.f > 0f,
            "order" => s.pendingOrder != null,
            _ => s.tutorialTime >= cfg.tutorial.readSec
        };

        /// <summary>Zeitschritt: nächster Schritt (TutorialStep) bzw. Ende (TutorialDone) als Ereignis.</summary>
        public static void Tick(GameState s, GameConfig cfg, Context c, float dt, System.Collections.Generic.List<DayEvent> events)
        {
            if (!Active(s, cfg)) return;
            s.tutorialTime += dt;
            if (!Done(cfg.tutorial.steps[s.tutorialStep].id, s, c, cfg)) return;
            s.tutorialStep++;
            s.tutorialTime = 0f;
            if (s.tutorialStep >= cfg.tutorial.steps.Length)
            {
                s.tutorialStep = -1;
                events.Add(DayEvent.TutorialDone);
            }
            else events.Add(DayEvent.TutorialStep);
        }

        /// <summary>
        /// Pflicht-Mischung bis zum Schritt „mix“ (auf Wunsch: in der Einführung klappt nur die vorgegebene Mischung).
        /// null = freie Wahl.
        /// </summary>
        public static Dose? RequiredMix(GameState s, GameConfig cfg)
        {
            var d = cfg.tutorial.mixDose;
            // nur während einer Schicht (die Einführung läuft nur dort)
            if (!Active(s, cfg) || s.quota == null || d == null || d.Length < 3) return null;
            int mix = System.Array.FindIndex(cfg.tutorial.steps, x => x.id == "mix");
            return s.tutorialStep <= mix ? new Dose(d[0], d[1], d[2]) : (Dose?)null;
        }

        /// <summary>
        /// Pflicht-Bestellung im Schritt „order“: genau die empfohlene Bestellung (ungekürzt). Reicht die Kasse nicht,
        /// ist die Wahl frei (null), damit die Einführung nicht hängen bleibt.
        /// </summary>
        public static int[] RequiredOrder(GameState s, GameConfig cfg)
        {
            var step = Current(s, cfg);
            if (step == null || step.id != "order") return null;
            var r = OrderLogic.Recommended(s, cfg, false);
            return r != null && OrderLogic.Sum(cfg, r) <= s.Money ? r : null;
        }

        /// <summary>Einführung abbrechen (Pause-Knopf), samt Hinweisen.</summary>
        public static void Skip(GameState s)
        {
            s.tutorialStep = -1;
            s.tutorialTime = 0f;
            s.hintsOn = false;
            s.hint = null;
        }
    }
}
