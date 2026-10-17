using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Name, Farbe und Bekanntheit eines Serums (Prototyp serumName/serumCol/serumKnown).</summary>
    public static class SerumTexts
    {
        /// <summary>Schlüssel im Entdeckungs-Verzeichnis: "A-B-C" bzw. "U:id" für Ultimative.</summary>
        public static string RecipeKey(Serum s) => s.IsUltimate ? "U:" + s.ultimateId : s.dose.Key;

        /// <summary>Wurde diese Kombination schon an einem Tier getestet?</summary>
        public static bool IsKnown(Serum s, GameState state) => state.discoveredRecipes.Contains(RecipeKey(s));

        /// <summary>Anzeigename mit Zusätzen (zentrifugiert, konzentriert, tiefgekühlt).</summary>
        public static string Name(Serum s, GameConfig cfg, TextDatabase t, GameState state)
        {
            if (s.IsUltimate)
            {
                var u = cfg.UltimateById(s.ultimateId);
                return u != null ? u.displayName : s.ultimateId;
            }
            var d = s.dose;
            string name = IsKnown(s, state)
                ? Formula.ComboName(d, cfg, t)
                : t.Format("ui.serum.unknownName", ("a", d.A), ("b", d.B), ("c", d.C));
            if (s.stage == SerumStage.Centrifuged) name += t.Get("ui.serum.suffixCentrifuged");
            if (s.IsConcentrated) name += t.Format("ui.serum.suffixConc", ("n", s.conc));
            if (s.frozen) name += t.Get("ui.serum.suffixFrozen");
            return name;
        }

        /// <summary>Farbe: Ultimativ-Farbe, Zentrifugen-Farbe oder Farbe des stärksten Stoffs.</summary>
        public static Color Color(Serum s, GameConfig cfg)
        {
            if (s.IsUltimate)
            {
                var u = cfg.UltimateById(s.ultimateId);
                if (u != null) return u.Color;
            }
            if (s.stage == SerumStage.Centrifuged) return Hex(cfg.mixing.centrifugedColor);
            return DoseColor(s.dose, cfg);
        }

        /// <summary>Farbe des am höchsten dosierten Stoffs (bei Gleichstand der erste).</summary>
        public static Color DoseColor(Dose d, GameConfig cfg)
        {
            int best = 0;
            for (int i = 1; i < 3; i++) if (d[i] > d[best]) best = i;
            return Hex(cfg.substances[best].color);
        }

        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : UnityEngine.Color.white;
    }
}
