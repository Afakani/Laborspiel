using System;

namespace PlanErfuellt
{
    /// <summary>Weg und Darstellung des Kommissars (Prototyp inspG, Prototyp-Koordinaten).</summary>
    [Serializable]
    public class InspectorConfig
    {
        /// <summary>Start rechts</summary>
        public float startX;
        /// <summary>Ende links (Kontrolle bestanden)</summary>
        public float endX;
        /// <summary>Weg vor der Kammer</summary>
        public float pathZ;
        /// <summary>Abstand für Wodka anbieten</summary>
        public float vodkaReach;
        /// <summary>Vorrang vor anderen Zielen (Wertung = Abstand − Wert)</summary>
        public float vodkaScoreOffset;
        /// <summary>Größe des Oberkommissars</summary>
        public float bossScale;
        /// <summary>Schwanken beim Trinken</summary>
        public float swayDeg;
        public float swayFreq;
        /// <summary>Schritte pro s beim Gehen</summary>
        public float stepFreq;
        public float legSwingDeg;
        /// <summary>Kamerawackeln beim Eintreffen</summary>
        public float startShake;
    }
}
