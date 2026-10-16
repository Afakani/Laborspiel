using System;

namespace PlanErfuellt
{
    /// <summary>Sonderaufträge (GDD 7.4).</summary>
    [Serializable]
    public class ContractsConfig
    {
        public int fromDay;
        public float payMult;
        public ContractConfig[] list = new ContractConfig[0];
    }
}
