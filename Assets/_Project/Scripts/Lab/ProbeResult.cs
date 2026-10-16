namespace PlanErfuellt
{
    /// <summary>Vage Tendenzen des Probenmikroskops (GDD 6.2).</summary>
    public readonly struct ProbeResult
    {
        /// <summary>0 = –, 1 = ↑, 2 = ↑↑, 3 = ↑↑↑</summary>
        public readonly int meatLevel;
        /// <summary>0 = –, 1 = ↑, 2 = ↑↑, 3 = ↑↑↑</summary>
        public readonly int milkLevel;
        /// <summary>1 = ↑, 0 = –, -1 = ↓, -2 = ↓↓</summary>
        public readonly int taste;
        /// <summary>0 = ruhig, 1 = nervös, 2 = brodelt gefährlich</summary>
        public readonly int danger;

        public ProbeResult(int meatLevel, int milkLevel, int taste, int danger)
        {
            this.meatLevel = meatLevel;
            this.milkLevel = milkLevel;
            this.taste = taste;
            this.danger = danger;
        }
    }
}
