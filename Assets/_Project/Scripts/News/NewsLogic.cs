using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Inhalt der Staats-TV-Sendung (GDD 11, Prototyp newsCopy): Stufe nach Hungersnot, Schlagzeile und Laufband aus
    /// news.* mit Platzhaltern, davor die verdrehten Taten (newsFacts) und das letzte Tagesergebnis. Sondersendung
    /// nach dem ersten Ultimativ. Reine Regeln, Texte über TextDatabase.
    /// </summary>
    public static class NewsLogic
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        public static NewsLevel Level(float famine, GameConfig cfg)
        {
            var n = cfg.news;
            return famine >= n.levelDead ? NewsLevel.Dead : famine >= n.levelCrisis ? NewsLevel.Crisis : famine >= n.levelWarn ? NewsLevel.Warn : NewsLevel.Boom;
        }

        public static string LevelKey(NewsLevel l) => l switch
        {
            NewsLevel.Dead => "dead",
            NewsLevel.Crisis => "crisis",
            NewsLevel.Warn => "warn",
            _ => "boom"
        };

        /// <summary>Sprecher verschwunden (Automatikbetrieb)?</summary>
        public static bool AnchorGone(float famine, GameConfig cfg) => famine >= cfg.news.anchorGoneFrom;

        /// <summary>
        /// Stimmung des Sprechers aus dem letzten Tagesergebnis (lastDayResult: 1 erfüllt, −1 verfehlt, 0 noch keins)
        /// und der Lage: erfüllt → Jubel (in der Krise nur gefasst); verfehlt → Schwitzen, ab zwei verfehlten Tagen
        /// oder in der Krise Weinen; ohne Ergebnis nach Lage.
        /// </summary>
        public static NewsMood Mood(GameState s, NewsLevel level)
        {
            bool crisis = level == NewsLevel.Crisis || level == NewsLevel.Dead;
            if (s.lastDayResult > 0) return crisis ? NewsMood.Calm : NewsMood.Joy;
            if (s.lastDayResult < 0) return crisis || s.failedDays >= 2 ? NewsMood.Cry : NewsMood.Sweat;
            return level == NewsLevel.Boom ? NewsMood.Calm : crisis ? NewsMood.Cry : NewsMood.Sweat;
        }

        /// <summary>Zahl mit Vorzeichen wie im Prototyp (fmtS): +12 bzw. −5.</summary>
        public static string Signed(float v) => (v < 0f ? "−" : "+") + Mathf.Abs(Mathf.Round(v)).ToString("0", German);

        /// <summary>Sendung zusammenstellen. Leert die Taten-Liste und die vorgemerkte Sondersendung.</summary>
        public static NewsCopy Compose(GameState s, GameConfig cfg, TextDatabase t, Func<float> rnd)
        {
            var w = WorldValues.Compute(s, cfg);
            var copy = new NewsCopy
            {
                level = Level(w.famine, cfg),
                anchorGone = AnchorGone(w.famine, cfg),
                world = w,
                mood = Mood(s, Level(w.famine, cfg)),
                year = t.Format("ui.news.time", ("n", Mathf.Max(1, s.dayNum)),
                    ("year", Mathf.RoundToInt(Mathf.Max(1, s.planIdx) * cfg.news.prognosisYearsPerPlan)))
            };
            var values = new (string, object)[]
            {
                ("meat", Signed(w.meat)), ("quality", Signed(w.quality)), ("pop", Mathf.RoundToInt(w.pop)),
                ("year", Mathf.Max(1, s.dayNum)), ("earned", Mathf.RoundToInt(s.earned).ToString("N0", German)), ("target", "")
            };

            // Taten (neueste zuerst) und Tagesergebnis vor die Standardzeilen
            var facts = new List<string>();
            int from = Mathf.Max(0, s.newsFacts.Count - cfg.news.factsMax);
            for (int i = s.newsFacts.Count - 1; i >= from; i--) facts.Add(Resolve(s.newsFacts[i], t));
            s.newsFacts.Clear();

            if (!string.IsNullOrEmpty(s.specialNewsUltimate))
            {
                var u = cfg.UltimateById(s.specialNewsUltimate);
                s.specialNewsUltimate = null;
                copy.special = true;
                copy.tag = t.Get("ui.news.specialTag");
                copy.head = t.Format("newsFacts.specialHead", ("name", u != null ? u.displayName : ""));
                copy.ticker.AddRange(t.GetList("newsFacts.specialTicker"));
                if (!string.IsNullOrEmpty(s.lastPlanLine)) copy.ticker.Add(s.lastPlanLine);
                copy.ticker.AddRange(facts);
                return copy;
            }

            string path = "news." + LevelKey(copy.level);
            int count = t.Count(path);
            int pick = count > 1 ? Mathf.Min(count - 1, Mathf.FloorToInt(rnd() * count)) : 0;
            string item = $"{path}[{pick}]";
            copy.tag = TextDatabase.Fill(t.Get(item + ".tag"), values);
            copy.head = TextDatabase.Fill(t.Get(item + ".head"), values);
            copy.ticker.AddRange(facts);
            if (!string.IsNullOrEmpty(s.lastPlanLine)) copy.ticker.Add(s.lastPlanLine);
            foreach (var line in t.GetList(item + ".ticker")) copy.ticker.Add(TextDatabase.Fill(line, values));
            return copy;
        }

        /// <summary>Tat auflösen: Pfad unter newsFacts (Liste → Zufallseintrag) oder fertiger Text.</summary>
        static string Resolve(string fact, TextDatabase t)
        {
            if (!fact.StartsWith("newsFacts.") || !t.Has(fact)) return fact;
            var list = t.GetList(fact);
            return list.Count > 0 ? t.Pick(fact) : t.Get(fact);
        }
    }
}
