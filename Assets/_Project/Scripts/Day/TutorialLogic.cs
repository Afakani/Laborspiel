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

        /// <summary>Einführung abbrechen (Pause-Knopf).</summary>
        public static void Skip(GameState s)
        {
            s.tutorialStep = -1;
            s.tutorialTime = 0f;
        }
    }
}
