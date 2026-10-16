using System;

namespace PlanErfuellt
{
    /// <summary>Ein Serum im Regal (GDD 4.2).</summary>
    [Serializable]
    public class Serum
    {
        public Dose dose;
        public SerumStage stage;
        /// <summary>Konzentration aus der Zentrifuge; 0 = nicht konzentriert.</summary>
        public float conc;
        public bool frozen;
        /// <summary>Id des ultimativen Serums (omega/koloss) oder leer.</summary>
        public string ultimateId;

        public Serum(Dose dose, SerumStage stage)
        {
            this.dose = dose;
            this.stage = stage;
        }

        public bool IsUltimate => !string.IsNullOrEmpty(ultimateId);
        public bool IsConcentrated => conc > 0f;
    }
}
