namespace PlanErfuellt
{
    /// <summary>
    /// Hinweise bei erstem Auftreten (auf Wunsch nach Spieltest): erklärt eine Lage genau dann, wenn sie zum ersten Mal
    /// passiert – Kontrolle (Laken), instabiles Tier (Not-Stopp), gerissenes Band (Bandmotor), verseuchte Kammer,
    /// Infektion (G), keine Laken mehr, wütender Direktor, dazu Zentrifuge, Kryo, goldene Seren und Nebenzimmer.
    /// Nur mit Einführung (hintsOn) und nicht während der geführten Schritte. Jeder Hinweis einmal pro Spiel; er
    /// verschwindet, sobald die Lage gelöst ist (Info-Hinweise nach hintInfoSec, alle spätestens nach hintMaxSec).
    /// </summary>
    public static class HintLogic
    {
        /// <summary>Was im Labor gerade los ist (aus der Szene).</summary>
        public struct Context
        {
            public bool unstable;
        }

        /// <summary>Tritt die Lage gerade ein?</summary>
        public static bool Triggered(string id, GameState s, Context c, GameConfig cfg) => id switch
        {
            "inspectionSoon" => s.inspectionAnnounced && s.inspection == null,
            "inspection" => s.inspection != null,
            "unstable" => c.unstable,
            "belt" => s.beltBroken,
            "contaminated" => s.contaminated,
            "infected" => s.infected,
            "noCloth" => s.cloths <= 0 && s.inspection == null && !PendingCloth(s),
            "anger" => s.anger >= cfg.anger.tiers[1],
            "centrifuge" => s.planIdx >= cfg.machinesFixed.centrifuge.unlockPlanIdx && s.dayNum >= 2,
            "cryo" => s.planIdx >= cfg.machinesFixed.cryo.unlockPlanIdx,
            "ultimate" => s.planIdx >= cfg.machinesFixed.ultimate.unlockPlanIdx,
            "rooms" => s.dayNum >= 2,
            _ => false
        };

        /// <summary>Ist die Lage gelöst (Hinweis darf weg)?</summary>
        public static bool Resolved(string id, GameState s, Context c, GameConfig cfg) => id switch
        {
            "inspectionSoon" => s.inspection != null || !s.inspectionAnnounced,
            "inspection" => s.inspection == null || s.chamberCovered,
            "unstable" => !c.unstable,
            "belt" => !s.beltBroken,
            "contaminated" => !s.contaminated,
            "infected" => !s.infected,
            "noCloth" => s.cloths > 0 || PendingCloth(s),
            "anger" => s.anger < cfg.anger.tiers[0],
            _ => false
        };

        static bool PendingCloth(GameState s) => s.pendingOrder != null && s.pendingOrder.Length > 6 && s.pendingOrder[6] > 0;

        public static TutorialHintConfig Find(string id, GameConfig cfg)
        {
            foreach (var h in cfg.tutorial.hints) if (h.id == id) return h;
            return null;
        }

        /// <summary>
        /// Zeitschritt. Gibt die Id eines neu erschienenen Hinweises zurück (für Ton und Anzeige), sonst null.
        /// </summary>
        public static string Tick(GameState s, GameConfig cfg, Context c, float dt)
        {
            if (!s.hintsOn || TutorialLogic.Active(s, cfg))
            {
                s.hint = null;
                return null;
            }
            var t = cfg.tutorial;
            if (s.hint != null)
            {
                s.hintTime += dt;
                var cur = Find(s.hint, cfg);
                bool done = cur == null || s.hintTime >= t.hintMaxSec
                            || (cur.info ? s.hintTime >= t.hintInfoSec : Resolved(s.hint, s, c, cfg));
                // ein dringender Hinweis (Kontrolle, Not-Stopp …) verdrängt einen Info-Hinweis
                if (!done && !(cur.info && Next(s, c, cfg, true) != null)) return null;
                s.hint = null;
            }
            var next = Next(s, c, cfg, false);
            if (next == null) return null;
            s.hint = next.id;
            s.hintTime = 0f;
            s.hintsSeen.Add(next.id);
            return next.id;
        }

        /// <summary>Nächster fälliger, noch nicht gezeigter Hinweis (optional nur dringende).</summary>
        static TutorialHintConfig Next(GameState s, Context c, GameConfig cfg, bool urgentOnly)
        {
            foreach (var h in cfg.tutorial.hints)
            {
                if (urgentOnly && h.info) continue;
                if (!s.hintsSeen.Contains(h.id) && Triggered(h.id, s, c, cfg)) return h;
            }
            return null;
        }
    }
}
