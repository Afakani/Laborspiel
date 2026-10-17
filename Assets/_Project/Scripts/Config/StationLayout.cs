using System;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Position, Ausrichtung, Kollider und Schild einer Station (Prototyp-Koordinaten, GDD 3).</summary>
    [Serializable]
    public class StationLayout
    {
        public string id;
        public float x;
        /// <summary>Höhe des Ursprungs (meist 0 = Boden).</summary>
        public float y;
        /// <summary>z im Prototyp; in Unity gilt -z.</summary>
        public float z;
        /// <summary>Drehung um Y in Unity-Grad; 0 = Vorderseite zeigt zum Spieler (-Z).</summary>
        public float facing;
        /// <summary>Kollider [Breite, Höhe, Tiefe].</summary>
        public float[] size = new float[3];
        /// <summary>Textpfad des Schilds (leer = kein Schild).</summary>
        public string signKey;
        /// <summary>Höhe des Schilds über dem Boden.</summary>
        public float signHeight;
        /// <summary>Reichweite für Interaktion um den Stationsmittelpunkt (0 = eigene Zone oder keine Interaktion).</summary>
        public float reach;

        /// <summary>Position in Prototyp-Koordinaten (x, z) für Abstandsrechnungen.</summary>
        public Vector2 ProtoXZ => new Vector2(x, z);

        public Vector3 UnityPosition => LayoutConfig.ToUnity(x, y, z);
        public Quaternion UnityRotation => Quaternion.Euler(0f, facing, 0f);
        public Vector3 Size => size != null && size.Length >= 3 ? new Vector3(size[0], size[1], size[2]) : Vector3.one;
    }
}
