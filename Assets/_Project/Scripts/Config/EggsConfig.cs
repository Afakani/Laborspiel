using System;

namespace PlanErfuellt
{
    /// <summary>Sichtbare Eier in der Kammer (D161): Ablage, Flug, Liegenbleiben bis Schichtende.</summary>
    [Serializable]
    public class EggsConfig
    {
        /// <summary>Höchstzahl Eier auf dem Boden (darüber verschwindet das älteste)</summary>
        public int maxOnFloor;
        /// <summary>Eilänge in m</summary>
        public float length;
        /// <summary>Eibreite in m</summary>
        public float width;
        /// <summary>Eierschale (Hex)</summary>
        public string color;
        /// <summary>Austritt über dem Boden (× Tiergröße)</summary>
        public float dropHeight;
        /// <summary>Austritt hinter der Mitte (× Tiergröße)</summary>
        public float backOffset;
        /// <summary>Wurf nach hinten in m/s</summary>
        public float scatterSpeed;
        /// <summary>Streuung zur Seite 0..1</summary>
        public float sideScatter;
        /// <summary>Wurf nach oben in m/s</summary>
        public float upSpeed;
        public float gravity;
        /// <summary>Aufprall: Anteil der Geschwindigkeit nach oben</summary>
        public float bounce;
        /// <summary>Aufprall: Anteil waagerecht</summary>
        public float bounceKeep;
        /// <summary>Darunter springt es nicht mehr</summary>
        public float settleSpeed;
        /// <summary>Bremsen beim Rollen pro s</summary>
        public float rollFriction;
        /// <summary>Darunter liegt es still</summary>
        public float stopSpeed;
        /// <summary>Umfallen auf die Seite pro s</summary>
        public float restTurnSpeed;
        /// <summary>Drehung im Flug Grad/s</summary>
        public float spinDegMin;
        public float spinDegMax;
        /// <summary>Abstand zur Glaswand in m</summary>
        public float wallMargin;
        /// <summary>Huhn geht beim Legen in die Hocke (Anteil)</summary>
        public float squatStrength;
        public float squatSec;
    }
}
