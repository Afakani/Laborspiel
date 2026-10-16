using System;

namespace PlanErfuellt
{
    /// <summary>Kryokammer (GDD 6.4).</summary>
    [Serializable]
    public class CryoConfig
    {
        public int unlockPlanIdx;
        public float freezeSec;
        public float takeWindowSec;
    }
}
