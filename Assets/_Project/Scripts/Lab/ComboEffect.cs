namespace PlanErfuellt
{
    /// <summary>Ein Vor- oder Nachteil mit Zahlenwert (z. B. Fleisch +70 %).</summary>
    public readonly struct ComboEffect
    {
        public readonly ComboEffectType type;
        public readonly bool isPro;
        /// <summary>Gerundeter Anzeigewert (Prozent oder Punkte), 0 wenn ohne Zahl.</summary>
        public readonly int value;

        public ComboEffect(ComboEffectType type, bool isPro, int value = 0)
        {
            this.type = type;
            this.isPro = isPro;
            this.value = value;
        }
    }
}
