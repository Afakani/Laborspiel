using System;

namespace PlanErfuellt
{
    /// <summary>Haltezeiten der Interaktionen in Sekunden.</summary>
    [Serializable]
    public class HoldTimesConfig
    {
        public float pult;
        public float mixing;
        public float rackHide;
        public float cloth;
        public float vodka;
        public float tasting;
        public float rohrpost;
        public float beltRepair;
        public float centrifugeLoad;
        public float centrifugeBrake;
        public float cryoLoad;
        public float cryoTake;
    }
}
