using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Begehbare Fläche als Vereinigung von Rechtecken in Prototyp-Koordinaten (x, z): Halle (layout.playerArea),
    /// je Nebenzimmer ein Durchgang in der Rückwand und das Zimmer selbst. Die Wände haben keine Kollider,
    /// deshalb hält diese Klemmung den Spieler im Gebäude.
    /// </summary>
    public static class WalkArea
    {
        /// <summary>Alle begehbaren Rechtecke als (xMin, zMin, xMax, zMax).</summary>
        public static List<Vector4> Rects(LayoutConfig L)
        {
            var list = new List<Vector4>();
            var a = L.playerArea;
            list.Add(new Vector4(a.xMin, a.zMin, a.xMax, a.zMax));
            var w = L.wing;
            if (L.rooms == null || w.depth <= 0f) return list;
            float m = w.margin, halfDoor = Mathf.Max(0.05f, w.doorWidth * 0.5f - m), halfWall = w.partition * 0.5f;
            float z0 = L.WingZMin, z1 = L.WingZMax;
            foreach (var r in L.rooms)
            {
                // Durchgang: überlappt die Halle und das Zimmer, damit man ohne Haken hindurchkommt
                list.Add(new Vector4(r.doorX - halfDoor, a.zMax - 0.1f, r.doorX + halfDoor, z0 + m + 0.1f));
                list.Add(new Vector4(r.xMin + halfWall + m, z0 + m, r.xMax - halfWall - m, z1 - m));
            }
            return list;
        }

        /// <summary>Punkt (x, Proto-z) auf die nächste begehbare Stelle setzen; liegt er schon drin, bleibt er.</summary>
        public static Vector2 Clamp(LayoutConfig L, Vector2 p) => Clamp(Rects(L), p);

        public static Vector2 Clamp(List<Vector4> rects, Vector2 p)
        {
            var best = p;
            float bestD = float.MaxValue;
            foreach (var r in rects)
            {
                var c = new Vector2(Mathf.Clamp(p.x, r.x, r.z), Mathf.Clamp(p.y, r.y, r.w));
                float d = (c - p).sqrMagnitude;
                if (d <= 0f) return p;
                if (d < bestD) { bestD = d; best = c; }
            }
            return best;
        }

        /// <summary>In welchem Nebenzimmer liegt der Punkt? Sonst null (Halle).</summary>
        public static SideRoomLayout RoomAt(LayoutConfig L, Vector2 p)
        {
            if (L.rooms == null || p.y < L.WingZMin) return null;
            foreach (var r in L.rooms)
                if (p.x >= r.xMin && p.x <= r.xMax) return r;
            return null;
        }
    }
}
