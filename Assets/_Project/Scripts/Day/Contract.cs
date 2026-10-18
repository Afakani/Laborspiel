namespace PlanErfuellt
{
    /// <summary>Sonderauftrag des Tages (GDD 7.4): zahlt mehrfach für eine Ware ab Mindestqualität.</summary>
    public class Contract
    {
        public ContractConfig data;
        public char Good => data.k[0];
        /// <summary>Gelieferte Menge zum Sonderpreis.</summary>
        public float got;
    }
}
