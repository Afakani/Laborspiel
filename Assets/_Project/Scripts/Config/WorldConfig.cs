using System;

namespace PlanErfuellt
{
    /// <summary>Spielwerte der Welt (GDD 7.7).</summary>
    [Serializable]
    public class WorldConfig
    {
        public float avgQStart;
        public float famineQ;
        public float famineRange;
        public float popMax;
        public GoodsValues supplyWeights = new GoodsValues();
        public float supplyPerDay;
        public float foodBase;
        public float foodPerSupply;
        public float foodMax;
        public float foodFamineFactor;
        public float envMax;
        public float envPerDose;
        public float envPerExplosion;
        public WindowHungerConfig windowHunger = new WindowHungerConfig();
        public WindowParadeConfig windowParade = new WindowParadeConfig();
    }
}
