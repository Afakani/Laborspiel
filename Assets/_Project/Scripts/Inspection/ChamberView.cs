namespace PlanErfuellt
{
    /// <summary>Was Parteikontrolle und Kommissar vom Tier in der Kammer wissen müssen (Prototyp subj).</summary>
    public struct ChamberView
    {
        /// <summary>Lebendes Tier mit Dosen oder Ultimativ (Prototyp mutated).</summary>
        public bool mutated;
        /// <summary>Tier steht (nicht instabil, nicht auf dem Band).</summary>
        public bool ok;
        public bool unstable;
        public float stability;
        public float floatHeight;
        /// <summary>Glühen 0..1 wie AnimalVisuals.Glow.</summary>
        public float glow;
        /// <summary>Id des Ultimativs oder null.</summary>
        public string ultimateId;
        /// <summary>Id der Kuriosität oder null.</summary>
        public string funId;
        /// <summary>Summe der Dosen am Tier.</summary>
        public int doseTotal;

        public bool IsOmega => ultimateId == UltimateIds.Omega;
        public bool IsKoloss => ultimateId == UltimateIds.Koloss;
        public bool Singing => funId == UltimateIds.SingingCow;

        /// <summary>Mutationsgrad: Dosensumme (begrenzt) bzw. fester Wert bei Ultimativ.</summary>
        public float Level(SuspicionConfig c) =>
            string.IsNullOrEmpty(ultimateId) ? System.Math.Min(doseTotal, c.coveredMaxDose) : c.ultLevel;
    }
}
