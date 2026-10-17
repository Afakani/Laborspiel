using System;

namespace PlanErfuellt
{
    /// <summary>Sichtbare Mutation am Tier (GDD 5.5).</summary>
    [Serializable]
    public class AnimalVisualsConfig
    {
        public float growPerA;
        public float growCapA;
        public float kolossGrowExtra;
        public float kolossA;
        public float omegaB;
        public float inflPerA;
        public float inflPerBOver2;
        public float inflBCap;
        public float inflSmoothing;
        public float udderPerB;
        public float udderBCap;
        public float udderSmoothing;
        public float henScale;
        public float glowFromInstab;
        public float glowFactor;
        public float glowOmega;
        public float glowSmoothing;
        public float emissiveFactor;
        public float lightFactor;
        public float legSwingFloatDeg;
        public float legSwingWalkDeg;
        public float legWalkFreq;
        public float legFloatFreq;
        public float wobbleUnstableDeg;
        public float wobbleLowStabDeg;
        public float lowStabWobbleBelow;
        public float floatBobAmp;
        public float floatRollDeg;
        public float floatBobFrom;
        public float wingFlapFast;
        public float wingFlapSlow;
        public float wingFlapDeg;
        public float floatRollFreq;
        public float floatBobFreq;
        public float wobbleUnstableFreq;
        public float wobbleLowStabFreq;
        public float pulseBase;
        public float pulseAmp;
        public float pulseFreq;
        public float pulseHeatFreq;
        public float pulseUnstableFreq;
        public string glowColor;
        public string heatColor;
        public string goldColor;
        public string spotColor;
        public string spotGlowColor;
        public float spotGlowFactor;
        public float unstableHeatColorMix;
        public float lightOffsetY;
        public float lightOffsetZ;
        public string funLabelColor;
        public float gasFlashAlpha;
        public float gasIdleAlpha;
    }
}
