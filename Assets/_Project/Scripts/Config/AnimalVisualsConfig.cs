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
        /// <summary>Weiches Wachsen: Annäherung pro s</summary>
        public float growSmoothing;
        /// <summary>Zusatz-Euter/Wucherungen: Ein-/Ausblenden pro s</summary>
        public float extraGrowSpeed;
        /// <summary>Atmen: Kreisfrequenz</summary>
        public float breatheFreq;
        /// <summary>Atmen: Stärke 0..1 (Blendshape)</summary>
        public float breatheAmp;
        /// <summary>Atmen schneller bei INSTABIL</summary>
        public float breatheUnstableMult;
        /// <summary>Kopf: Umschauen in Grad</summary>
        public float headLookDeg;
        /// <summary>Kopf: Umschau-Frequenz</summary>
        public float headLookFreq;
        /// <summary>Kopf: Nicken in Grad</summary>
        public float headNodDeg;
        /// <summary>Kopf: Nicken beim Laufen</summary>
        public float headWalkBobDeg;
        /// <summary>Huhn: Kopf vor/zurück beim Laufen in m</summary>
        public float henHeadBob;
        /// <summary>Ohrenzucken in Grad</summary>
        public float earFlickDeg;
        /// <summary>Ohrenzucken alle n s</summary>
        public float earFlickEverySec;
        /// <summary>Dauer eines Zuckens</summary>
        public float earFlickSec;
        /// <summary>Schwanz: Pendeln in Grad</summary>
        public float tailSwingDeg;
        /// <summary>Schwanz: Frequenz</summary>
        public float tailFreq;
        /// <summary>Knie beim Laufen in Grad</summary>
        public float kneeBendDeg;
        /// <summary>Zittern des Rumpfs bei INSTABIL in Grad</summary>
        public float shiverDeg;
        /// <summary>Zittern: Frequenz</summary>
        public float shiverFreq;
    }
}
