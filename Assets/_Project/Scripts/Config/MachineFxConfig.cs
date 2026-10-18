using System;

namespace PlanErfuellt
{
    /// <summary>Darstellung von Zentrifuge und Kryokammer (Prototyp updatePlay/kryWin).</summary>
    [Serializable]
    public class MachineFxConfig
    {
        /// <summary>Rotordrehung bei voller Drehzahl</summary>
        public float centrifugeRotorRadPerSec;
        /// <summary>Wackeln des Gehäuses in m bei voller Drehzahl</summary>
        public float centrifugeShakeAmp;
        /// <summary>Kamerawackeln beim Zerspringen</summary>
        public float centrifugeBreakShake;
        public float cryoGlowIdle;
        public float cryoGlowFreezing;
        public float cryoGlowPulse;
        /// <summary>Eisblock im Fenster sichtbar</summary>
        public float iceBlockShowSec;
        /// <summary>Grammophon-Platte</summary>
        public float recordRadPerSec;
        public float crankRadPerSec;
        /// <summary>Noten am Grammophon</summary>
        public float notesPerSec;
        /// <summary>Noten über der Singenden Kuh</summary>
        public float singNotesPerSec;
        /// <summary>Milchtropfen beim Milchhuhn</summary>
        public float milkDropsPerRate;
        /// <summary>Stempelbewegung des Verkostungsautomaten</summary>
        public float stampSec;
        public float probeGlowPulse;
    }
}
