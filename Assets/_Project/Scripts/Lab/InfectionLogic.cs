using System;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Infektion des Spielers (GDD 8, Prototyp infect/selfInject/updateInfection): Ansteckung bei Explosion,
    /// Frist bis Ende des nächsten Tages, langsam steigender Infektionswert, zufällige Symptome, Heilung mit Gegenmittel.
    /// Reine Logik; die Wirkung auf Kamera und Steuerung macht InfectionSystem.
    /// </summary>
    public static class InfectionLogic
    {
        public enum InfectResult { NotYet, Missed, Infected, More }

        /// <summary>Laufendes Symptom und Zeitplan (nicht gespeichert).</summary>
        public class Symptoms
        {
            /// <summary>Aktuelles Symptom (Id aus infection.symptoms) oder null.</summary>
            public string current;
            public float timer;
            public float next;
        }

        /// <summary>Explosion: Ansteckung ab infectionFromDay, wenn nah dran oder per Zufall.</summary>
        public static InfectResult OnExplosion(GameState s, GameConfig cfg, float distance, float roll, Symptoms sym)
        {
            var r = cfg.risks;
            if (s.dayNum < r.infectionFromDay) return InfectResult.NotYet;
            if (distance >= r.infectionDistance && roll >= r.infectionChance) return InfectResult.Missed;
            if (s.infected)
            {
                s.infectionLevel = Mathf.Min(1f, s.infectionLevel + cfg.infection.levelMore);
                return InfectResult.More;
            }
            s.infected = true;
            s.newsFacts.Add("newsFacts.infection");
            s.infectedUntilDay = s.dayNum + r.infectionDays - 1;
            s.infectionLevel = cfg.infection.levelStart;
            sym.current = null;
            sym.timer = 0f;
            sym.next = cfg.infection.firstSymptomSec;
            return InfectResult.Infected;
        }

        /// <summary>
        /// Zeitschritt. Gibt das neu begonnene Symptom zurück (sonst null). rnd liefert Zufallszahlen in [0, 1).
        /// </summary>
        public static string Tick(GameState s, GameConfig cfg, Symptoms sym, float dt, Func<float> rnd)
        {
            if (!s.infected) return null;
            var c = cfg.infection;
            s.infectionLevel = Mathf.Min(1f, s.infectionLevel + dt * c.levelPerSec);
            if (sym.current != null)
            {
                sym.timer -= dt;
                if (sym.timer > 0f) return null;
                sym.current = null;
                sym.next = Mathf.Lerp(c.symptomIntervalSec[0], c.symptomIntervalSec[1], rnd());
                return null;
            }
            sym.next -= dt;
            if (sym.next > 0f) return null;
            int i = Mathf.Min(c.symptoms.Length - 1, Mathf.FloorToInt(rnd() * c.symptoms.Length));
            sym.current = c.symptoms[i];
            sym.timer = Mathf.Lerp(c.symptomDurationSec[0], c.symptomDurationSec[1], rnd());
            return sym.current;
        }

        /// <summary>Gegenmittel selbst spritzen (Taste G). Fehler als Feedback (Textpfad).</summary>
        public static Feedback SelfInject(GameState s, Symptoms sym)
        {
            if (!s.infected) return Feedback.Error("ui.errors.notInfected");
            if (s.antidote <= 0) return Feedback.Error("ui.errors.noAntidoteBuy");
            s.antidote--;
            Clear(s, sym);
            return Feedback.Ok;
        }

        /// <summary>Tagesende: Frist abgelaufen → Zusammenbruch (Wut steigt, Infektion endet). Ruft der Tagesablauf (Phase 6) auf.</summary>
        public static bool TryCollapse(GameState s, GameConfig cfg, Symptoms sym)
        {
            if (!s.infected || s.dayNum < s.infectedUntilDay) return false;
            Clear(s, sym);
            s.AddAnger(cfg.anger.collapse, cfg);
            return true;
        }

        static void Clear(GameState s, Symptoms sym)
        {
            s.infected = false;
            s.infectionLevel = 0f;
            sym.current = null;
            sym.timer = 0f;
        }
    }
}
