using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Inhalt des Forschungslogbuchs (GDD 12, Prototyp renderLog): getestete Kombinationen (sortiert nach Gesamtdosis),
    /// Geheimakte der benutzten Ultimative, gefundene Kuriositäten, Rekordtafel. Nur Entdecktes.
    /// </summary>
    public static class LogbookLogic
    {
        public static List<LogbookEntry> Build(GameState s, GameConfig cfg, TextDatabase t)
        {
            var list = new List<LogbookEntry>();

            // Getestete Kombinationen
            var doses = new List<Dose>();
            foreach (var key in s.discoveredRecipes)
                if (!key.StartsWith("U:") && TryParse(key, out var d)) doses.Add(d);
            doses.Sort((a, b) => a.Total != b.Total ? a.Total.CompareTo(b.Total) : string.CompareOrdinal(a.Key, b.Key));
            foreach (var d in doses)
            {
                var info = Formula.GetComboInfo(d, cfg);
                list.Add(new LogbookEntry
                {
                    section = LogbookSection.Combo,
                    title = Formula.ComboName(d, cfg, t),
                    sub = t.Format("ui.logbook.doseLine", ("a", d.A), ("b", d.B), ("c", d.C), ("stab", Formula.RoundHalfUp(Formula.Stability(d, cfg)))),
                    pros = ComboTexts.Effects(info.pros, t),
                    cons = ComboTexts.Effects(info.cons, t),
                    sketch = d,
                    kind = AnimalKind.Cow
                });
            }

            // Geheimakte
            foreach (var u in cfg.ultimates)
            {
                if (!s.discoveredRecipes.Contains("U:" + u.id)) continue;
                var r = u.Recipe;
                list.Add(new LogbookEntry
                {
                    section = LogbookSection.Secret,
                    title = u.displayName,
                    sub = t.Format("ui.logbook.secretRecipe", ("a", r.A), ("b", r.B), ("c", r.C)),
                    pros = new List<string>(u.pros),
                    cons = new List<string>(u.cons),
                    sketch = r,
                    kind = AnimalKind.Cow,
                    ultimateId = u.id
                });
            }

            // Kuriositäten
            foreach (var f in cfg.funRecipes)
            {
                if (!s.foundFunRecipes.Contains(f.id)) continue;
                var d = new Dose(f.total[0], f.total[1], f.total[2]);
                list.Add(new LogbookEntry
                {
                    section = LogbookSection.Curiosity,
                    title = f.displayName,
                    sub = t.Format("ui.logbook.funLine", ("kind", t.Get(f.animal == AnimalKind.Cow ? "ui.common.kindCow" : "ui.common.kindHen")),
                        ("a", d.A), ("b", d.B), ("c", d.C)),
                    pros = new List<string>(f.pros),
                    cons = new List<string>(f.cons),
                    sketch = d,
                    kind = f.animal,
                    funId = f.id
                });
            }

            // Rekordtafel
            var lines = new List<string>();
            foreach (var key in RecordLogic.All)
                if (s.records.TryGetValue(key, out float v) && v > 0f)
                    lines.Add(t.Format(RecordLogic.LogbookPath(key), ("n", RecordLogic.FormatValue(key, v))));
            if (lines.Count > 0)
                list.Add(new LogbookEntry
                {
                    section = LogbookSection.Records,
                    title = t.Get("ui.logbook.recordsCard"),
                    sub = t.Get("ui.logbook.recordsCardSub"),
                    pros = lines,
                    cons = new List<string>()
                });
            return list;
        }

        /// <summary>„A-B-C“ in eine Dosis umwandeln.</summary>
        public static bool TryParse(string key, out Dose d)
        {
            d = Dose.Zero;
            var parts = key.Split('-');
            if (parts.Length != 3 || !int.TryParse(parts[0], out int a) || !int.TryParse(parts[1], out int b) || !int.TryParse(parts[2], out int c)) return false;
            d = new Dose(a, b, c);
            return true;
        }

        /// <summary>Skizzen-Maße wie im Prototyp (sketch): Größe nach A, Euter nach B, gestrichelter Rahmen bei C.</summary>
        public static void SketchShape(Dose d, GameConfig cfg, out float scale, out int udders, out bool frame)
        {
            var v = cfg.animalVisuals;
            scale = 1f + v.growPerA * Mathf.Min(d.A, v.growCapA);
            udders = Mathf.RoundToInt(Mathf.Min(d.B, v.udderBCap));
            frame = d.C > 0;
        }
    }
}
