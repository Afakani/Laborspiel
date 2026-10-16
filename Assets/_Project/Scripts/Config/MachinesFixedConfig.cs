using System;

namespace PlanErfuellt
{
    /// <summary>Feste Stationen im Labor.</summary>
    [Serializable]
    public class MachinesFixedConfig
    {
        public CentrifugeConfig centrifuge = new CentrifugeConfig();
        public CryoConfig cryo = new CryoConfig();
        public UnlockConfig ultimate = new UnlockConfig();
        public TastingConfig tasting = new TastingConfig();
        public GramophoneConfig gramophone = new GramophoneConfig();
        public RohrpostConfig rohrpost = new RohrpostConfig();
        public MicroscopeConfig microscope = new MicroscopeConfig();
    }
}
