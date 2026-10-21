using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Reine Spielformeln (GDD 4.3, 4.4, 6.2). Ohne Zustand, alle Zahlen aus der GameConfig.
    /// </summary>
    public static class Formula
    {
        // ---------- Grundformeln (GDD 4.3) ----------

        /// <summary>Dämpfung durch C: e = 1 / (1 + dampPerC·C).</summary>
        public static float Dampening(Dose d, GameConfig cfg) => 1f / (1f + cfg.formulas.dampPerC * d.C);

        /// <summary>Stabilität einer Dosis (ohne Boni), 0..statMax.</summary>
        public static float Stability(Dose d, GameConfig cfg)
        {
            var k = cfg.formulas.stability;
            float s = k.@base - k.perA * d.A - k.perB * d.B - k.perAB * d.A * d.B + k.perC * d.C
                      - k.overloadPer * Mathf.Max(0, d.Total - k.overloadFrom);
            return Mathf.Clamp(s, 0f, cfg.formulas.statMax);
        }

        /// <summary>Qualität einer Dosis (ohne Boni), 0..statMax.</summary>
        public static float Quality(Dose d, GameConfig cfg)
        {
            var k = cfg.formulas.quality;
            return Mathf.Clamp(k.@base - k.perA * d.A - k.perB * d.B + k.perC * d.C, 0f, cfg.formulas.statMax);
        }

        /// <summary>Ertragsfaktor Milch (Kuh): 1 + perB·B·e + perA·A·e.</summary>
        public static float RateFactor(Dose d, GameConfig cfg) => RateFactor(d, cfg, AnimalKind.Cow);

        /// <summary>Ertragsfaktor je Tierart: beim Huhn wirkt B mit ratePerBHen (Laktation treibt die Eier stärker).</summary>
        public static float RateFactor(Dose d, GameConfig cfg, AnimalKind kind)
        {
            float e = Dampening(d, cfg);
            var k = cfg.formulas.rate;
            float perB = kind == AnimalKind.Hen ? cfg.formulas.ratePerBHen : k.perB;
            return 1f + perB * d.B * e + k.perA * d.A * e;
        }

        /// <summary>Fleischfaktor: 1 + perA·A·e + perB·B·e.</summary>
        public static float MeatFactor(Dose d, GameConfig cfg)
        {
            float e = Dampening(d, cfg);
            var k = cfg.formulas.meat;
            return 1f + k.perA * d.A * e + k.perB * d.B * e;
        }

        /// <summary>Ertrag in t/s nach Formel.</summary>
        public static float Rate(AnimalKind kind, Dose d, GameConfig cfg) => cfg.Animal(kind).baseRate * RateFactor(d, cfg, kind);

        /// <summary>Fleisch in t nach Formel.</summary>
        public static float Meat(AnimalKind kind, Dose d, GameConfig cfg) => cfg.Animal(kind).baseMeat * MeatFactor(d, cfg);

        /// <summary>Konzentriert: basis + (formel − basis)·conc. conc ≤ 0 gilt als 1.</summary>
        public static float Concentrated(float baseValue, float formulaValue, float conc)
        {
            float k = conc > 0f ? conc : 1f;
            return baseValue + (formulaValue - baseValue) * k;
        }

        // ---------- Werte am Tier mit Modifikatoren (GDD 4.3, 4.5, 4.6) ----------

        /// <summary>
        /// Basis-Stabilität S0 am Tier: Formel bzw. Ultimativ-Wert + Kryo-Bonus + Musik, max. statMax.
        /// S0 steuert auch die Risiken (Entscheidung: wie Prototyp stabOf).
        /// </summary>
        public static float AnimalStability(Dose d, UltimateSerum ult, float frozenBonus, bool music, GameConfig cfg)
        {
            float s = ult != null ? ult.stability : Stability(d, cfg);
            s += frozenBonus + (music ? cfg.formulas.musicStabBonus : 0f);
            return Mathf.Min(cfg.formulas.statMax, s);
        }

        /// <summary>Qualität am Tier: Formel bzw. Ultimativ + Siegel − Münzhuhn, bei Säure ×acid.quality.</summary>
        public static float AnimalQuality(Dose d, UltimateSerum ult, bool stamped, FunRecipe fun, bool acid, GameConfig cfg)
        {
            float q = ult != null ? ult.quality : Quality(d, cfg);
            if (stamped) q += cfg.formulas.stampQualityBonus;
            if (fun != null) q -= fun.qualityMalus;
            q = Mathf.Clamp(q, 0f, cfg.formulas.statMax);
            return acid ? q * cfg.formulas.acid.quality : q;
        }

        /// <summary>
        /// Ertrag am Tier in t/s ohne Säure, Stromausfall und Milchhuhn-Faktor (die wendet das Tier selbst an).
        /// </summary>
        public static float AnimalRate(AnimalKind kind, Dose d, UltimateSerum ult, float conc, GameConfig cfg)
        {
            float b = cfg.Animal(kind).baseRate;
            return ult != null ? b * ult.rateMult : Concentrated(b, Rate(kind, d, cfg), conc);
        }

        /// <summary>Fleisch am Tier in t ohne Säure-Faktor.</summary>
        public static float AnimalMeat(AnimalKind kind, Dose d, UltimateSerum ult, float conc, FunRecipe fun, GameConfig cfg)
        {
            float b = cfg.Animal(kind).baseMeat;
            float m = ult != null ? b * ult.meatMult : Concentrated(b, Meat(kind, d, cfg), conc);
            return fun != null ? m * fun.meatMult : m;
        }

        // ---------- Kombinationen (GDD 4.4) ----------

        /// <summary>Name einer Kombination: fester Name aus der Config oder generischer Name aus den Texten.</summary>
        public static string ComboName(Dose d, GameConfig cfg, TextDatabase texts)
        {
            var fixedName = cfg.FixedComboName(d.Key);
            if (fixedName != null) return fixedName;

            var c = cfg.combo;
            string baseKey = d.AB == 0 ? "baseStabilizer" : d.A > d.B ? "baseFatten" : d.B > d.A ? "baseMilk" : "baseCombi";
            string name = texts.Get("ui.combo." + baseKey);
            if (d.AB > 0 && d.C >= Mathf.Max(c.stabilizedMinC, d.AB * c.stabilizedRatio))
                name += texts.Get("ui.combo.suffixStabilized");
            else if (d.AB >= c.extremeFromAB)
                name += texts.Get("ui.combo.suffixExtreme");
            return name;
        }

        /// <summary>Vor- und Nachteile einer Kombination auf Kuh-Basis.</summary>
        public static ComboInfo GetComboInfo(Dose d, GameConfig cfg)
        {
            var c = cfg.combo;
            var info = new ComboInfo
            {
                dose = d,
                rate = Rate(AnimalKind.Cow, d, cfg),
                meat = Meat(AnimalKind.Cow, d, cfg),
                quality = Quality(d, cfg),
                stability = Stability(d, cfg)
            };
            float mf = MeatFactor(d, cfg), rf = RateFactor(d, cfg), qBase = cfg.formulas.quality.@base;

            if (mf > c.showEffectAboveFactor) info.pros.Add(new ComboEffect(ComboEffectType.Meat, true, RoundHalfUp((mf - 1f) * 100f)));
            if (rf > c.showEffectAboveFactor) info.pros.Add(new ComboEffect(ComboEffectType.Rate, true, RoundHalfUp((rf - 1f) * 100f)));
            float ef = RateFactor(d, cfg, AnimalKind.Hen);
            if (ef > c.showEffectAboveFactor) info.pros.Add(new ComboEffect(ComboEffectType.Eggs, true, RoundHalfUp((ef - 1f) * 100f)));
            if (info.quality > qBase) info.pros.Add(new ComboEffect(ComboEffectType.QualityUp, true, RoundHalfUp(info.quality - qBase)));
            else if (info.quality < qBase) info.cons.Add(new ComboEffect(ComboEffectType.QualityDown, false, RoundHalfUp(qBase - info.quality)));
            if (d.C > 0) info.pros.Add(new ComboEffect(ComboEffectType.Stabilized, true, RoundHalfUp(d.C * cfg.formulas.stability.perC)));
            if (d.C > 0 && d.AB > 0) info.cons.Add(new ComboEffect(ComboEffectType.Dampened, false, RoundHalfUp((1f - Dampening(d, cfg)) * 100f)));
            if (info.stability < cfg.risks.activeBelowStab) info.cons.Add(new ComboEffect(ComboEffectType.LowStability, false, RoundHalfUp(info.stability)));
            if (d.A >= c.heatWarnMinA) info.cons.Add(new ComboEffect(ComboEffectType.Heat, false));
            if (d.B >= c.acidWarnMinB) info.cons.Add(new ComboEffect(ComboEffectType.Acid, false));
            if (d.B >= c.floatWarnMinB) info.cons.Add(new ComboEffect(ComboEffectType.Float, false));
            if (d.A >= c.beltWarnMinA) info.cons.Add(new ComboEffect(ComboEffectType.Belt, false));
            if (d.AB == 0) info.cons.Add(new ComboEffect(ComboEffectType.NoGain, false));
            if (info.pros.Count == 0) info.pros.Add(new ComboEffect(ComboEffectType.FallbackPro, true));
            if (info.cons.Count == 0) info.cons.Add(new ComboEffect(ComboEffectType.FallbackCon, false));
            return info;
        }

        /// <summary>Tendenzen für das Probenmikroskop (GDD 6.2).</summary>
        public static ProbeResult Probe(Dose d, GameConfig cfg)
        {
            var m = cfg.machinesFixed.microscope;
            float q = Quality(d, cfg), s = Stability(d, cfg);
            int meat = CountAbove(MeatFactor(d, cfg), m.factorLevels);
            int milk = CountAbove(RateFactor(d, cfg), m.factorLevels);
            int taste = CountAtLeast(q, m.qualityLevels) - 2;
            int danger = m.stabLevels.Length - CountAtLeast(s, m.stabLevels);
            return new ProbeResult(meat, milk, taste, danger);
        }

        /// <summary>Kaufmännisches Runden wie im Prototyp (Math.round).</summary>
        public static int RoundHalfUp(float v) => Mathf.FloorToInt(v + 0.5f);

        static int CountAbove(float v, float[] limits)
        {
            int n = 0;
            foreach (var t in limits) if (v > t) n++;
            return n;
        }

        static int CountAtLeast(float v, float[] limits)
        {
            int n = 0;
            foreach (var t in limits) if (v >= t) n++;
            return n;
        }
    }
}
