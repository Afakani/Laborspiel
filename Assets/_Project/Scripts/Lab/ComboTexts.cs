using System.Collections.Generic;

namespace PlanErfuellt
{
    /// <summary>Macht aus ComboInfo und ProbeResult lesbare Texte (aus ui.combo bzw. ui.probe).</summary>
    public static class ComboTexts
    {
        /// <summary>Anzeigetext eines Vor- oder Nachteils.</summary>
        public static string Effect(ComboEffect e, TextDatabase t)
        {
            switch (e.type)
            {
                case ComboEffectType.Meat: return t.Format("ui.combo.proMeat", ("n", e.value));
                case ComboEffectType.Rate: return t.Format("ui.combo.proRate", ("n", e.value));
                case ComboEffectType.Eggs: return t.Format("ui.combo.proEggs", ("n", e.value));
                case ComboEffectType.QualityUp: return t.Format("ui.combo.proQuality", ("n", e.value));
                case ComboEffectType.QualityDown: return t.Format("ui.combo.conQuality", ("n", e.value));
                case ComboEffectType.Stabilized: return t.Format("ui.combo.proStabilized", ("n", e.value));
                case ComboEffectType.Dampened: return t.Format("ui.combo.conDampened", ("n", e.value));
                case ComboEffectType.LowStability: return t.Format("ui.combo.conLowStab", ("n", e.value));
                case ComboEffectType.Heat: return t.Get("ui.combo.conHeat");
                case ComboEffectType.Acid: return t.Get("ui.combo.conAcid");
                case ComboEffectType.Float: return t.Get("ui.combo.conFloat");
                case ComboEffectType.Belt: return t.Get("ui.combo.conBelt");
                case ComboEffectType.NoGain: return t.Get("ui.combo.conNoGain");
                case ComboEffectType.FallbackPro: return t.Get("ui.combo.proFallback");
                default: return t.Get("ui.combo.conFallback");
            }
        }

        /// <summary>Alle Vor- bzw. Nachteile als Texte.</summary>
        public static List<string> Effects(IEnumerable<ComboEffect> effects, TextDatabase t)
        {
            var list = new List<string>();
            foreach (var e in effects) list.Add(Effect(e, t));
            return list;
        }

        /// <summary>Mikroskop-Ergebnis als eine Zeile.</summary>
        public static string Probe(ProbeResult p, TextDatabase t)
        {
            string taste = p.taste >= 1 ? "tasteUp" : p.taste == 0 ? "tasteSame" : p.taste == -1 ? "tasteDown" : "tasteBad";
            string state = p.danger == 0 ? "calm" : p.danger == 1 ? "nervous" : "dangerous";
            return t.Format("ui.probe.format",
                ("meat", t.Get("ui.probe.level" + p.meatLevel)),
                ("milk", t.Get("ui.probe.level" + p.milkLevel)),
                ("taste", t.Get("ui.probe." + taste)),
                ("state", t.Get("ui.probe." + state)));
        }
    }
}
