using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Werte des Tiers in der Kammer (Prototyp subj). Reine Daten plus Berechnungen über Formula;
    /// die Kammer verändert sie.
    /// </summary>
    public class AnimalStats
    {
        public AnimalKind kind;
        /// <summary>Summe aller gesprühten Dosen.</summary>
        public Dose dose;
        public UltimateSerum ultimate;
        /// <summary>Höchste Konzentration der gesprühten Seren (0 = keine).</summary>
        public float conc;
        /// <summary>Stabilitätsbonus aus tiefgekühlten Seren.</summary>
        public float stabBonus;
        public FunRecipe fun;
        public bool stamped;
        public bool acid;
        public float heat;
        public float floatHeight;
        public float acidTimer;
        public float stability = 100f;
        public float minStability = 100f;
        public float unstableTimer;
        public CrisisKind crisis;
        public float currentRate;
        public float maxRate;
        public float eggTimer;
        public bool brave;
        /// <summary>Schon gezeigte Warnzeichen.</summary>
        public RiskWarnings warned;

        /// <summary>Alles außer der Tierart zurücksetzen (Prototyp resetSubj).</summary>
        public void Reset(float eggInterval)
        {
            dose = Dose.Zero;
            ultimate = null;
            conc = 0f;
            stabBonus = 0f;
            fun = null;
            stamped = false;
            acid = false;
            heat = 0f;
            floatHeight = 0f;
            acidTimer = 0f;
            stability = 100f;
            minStability = 100f;
            unstableTimer = 0f;
            crisis = CrisisKind.None;
            currentRate = 0f;
            maxRate = 0f;
            eggTimer = eggInterval;
            brave = false;
            warned = RiskWarnings.None;
        }

        public bool IsUltimate => ultimate != null;
        public bool IsMutated => dose.Total > 0 || IsUltimate;

        /// <summary>Basis-Stabilität S0 (Formel/Ultimativ + Kryo + Musik).</summary>
        public float BaseStability(bool music, GameConfig cfg) => Formula.AnimalStability(dose, ultimate, stabBonus, music, cfg);

        public float Quality(GameConfig cfg) => Formula.AnimalQuality(dose, ultimate, stamped, fun, acid, cfg);

        /// <summary>Ertrag in t/s inklusive Säure, Stromausfall und Milchhuhn.</summary>
        public float Rate(bool blackout, GameConfig cfg)
        {
            float r = Formula.AnimalRate(kind, dose, ultimate, conc, cfg);
            if (acid) r *= cfg.formulas.acid.rate;
            if (blackout) r *= cfg.risks.blackoutProductionFactor;
            if (fun != null) r *= fun.rateMult;
            return r;
        }

        /// <summary>Fleisch bei Verarbeitung inklusive Säure.</summary>
        public float Meat(GameConfig cfg)
        {
            float m = Formula.AnimalMeat(kind, dose, ultimate, conc, fun, cfg);
            return acid ? m * cfg.formulas.acid.meat : m;
        }

        /// <summary>Welche Ware das Tier liefert: Milch bei Kuh und Milchhuhn, sonst Eier.</summary>
        public char Produces(GameConfig cfg)
        {
            if (kind == AnimalKind.Cow) return cfg.animals.cow.produces[0];
            return fun != null && fun.id == UltimateIds.MilkHen ? cfg.animals.cow.produces[0] : cfg.animals.hen.produces[0];
        }

        /// <summary>Wachstums-Kennzahl für die Optik (Koloss zählt als volles A).</summary>
        public int VisualA(GameConfig cfg) => ultimate != null && ultimate.id == UltimateIds.Koloss ? Mathf.RoundToInt(cfg.animalVisuals.kolossA) : dose.A;
        /// <summary>Laktations-Kennzahl für die Optik (Ω zählt als B 3).</summary>
        public int VisualB(GameConfig cfg) => ultimate != null && ultimate.id == UltimateIds.Omega ? Mathf.RoundToInt(cfg.animalVisuals.omegaB) : dose.B;
    }
}
