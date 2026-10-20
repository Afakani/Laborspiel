using System;

namespace PlanErfuellt
{
    /// <summary>Spielfigur in Ego-Sicht (GDD 2, Prototyp updatePlay).</summary>
    [Serializable]
    public class PlayerConfig
    {
        public float walkSpeed;
        /// <summary>Q/E und Pfeiltasten</summary>
        public float turnSpeedDeg;
        /// <summary>Grad pro Mauspixel</summary>
        public float mouseSensitivityDeg;
        public float lookUpMaxDeg;
        public float lookDownMaxDeg;
        public float radius;
        public float height;
        public float fov;
        public float fovMax;
        /// <summary>Unter diesem Seitenverhältnis wird das Sichtfeld weiter</summary>
        public float fovAspectRef;
        public BobConfig bob = new BobConfig();
        public HandsConfig hands = new HandsConfig();
        /// <summary>Drehfaktor, wenn die Spielfigur beschleunigt ist (Infektion)</summary>
        public float fastTurnBoost;
        /// <summary>Tempo-Faktor beim Sprinten (Shift)</summary>
        public float sprintMultiplier;
        /// <summary>Sprunghöhe in m (Leertaste ohne Ziel)</summary>
        public float jumpHeight;
        /// <summary>Fallbeschleunigung in m/s²</summary>
        public float gravity;
    }
}
