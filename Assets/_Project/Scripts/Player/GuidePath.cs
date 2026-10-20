using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Weg für die Einführungspfeile in Prototyp-Koordinaten (x, z): geradeaus, bei Nebenzimmern durch die Tür
    /// (Punkt vor der Tür in der Halle und Punkt dahinter im Zimmer). Dazu Punkte im festen Abstand entlang des Wegs.
    /// </summary>
    public static class GuidePath
    {
        /// <summary>Abstand der Türpunkte zur Wand (Halle bzw. Zimmer).</summary>
        public const float DoorOffset = 1.0f;

        public static List<Vector2> Waypoints(LayoutConfig L, Vector2 from, Vector2 to)
        {
            var pts = new List<Vector2> { from };
            var a = WalkArea.RoomAt(L, from);
            var b = WalkArea.RoomAt(L, to);
            if (a != b)
            {
                float hallZ = L.room.backWallZ - DoorOffset, roomZ = L.WingZMin + DoorOffset;
                if (a != null)
                {
                    pts.Add(new Vector2(a.doorX, roomZ));
                    pts.Add(new Vector2(a.doorX, hallZ));
                }
                if (b != null)
                {
                    pts.Add(new Vector2(b.doorX, hallZ));
                    pts.Add(new Vector2(b.doorX, roomZ));
                }
            }
            pts.Add(to);
            return pts;
        }

        /// <summary>
        /// Punkte im Abstand step entlang des Wegs, beginnend bei start (ab dem ersten Punkt), höchstens bis stopBefore
        /// vor dem Ziel und höchstens max Stück. Liefert Lage und Richtung (normiert) je Punkt.
        /// </summary>
        public static List<(Vector2 pos, Vector2 dir)> Samples(List<Vector2> pts, float start, float step, float stopBefore, int max)
        {
            var res = new List<(Vector2, Vector2)>();
            float total = 0f;
            for (int i = 1; i < pts.Count; i++) total += Vector2.Distance(pts[i - 1], pts[i]);
            float end = total - stopBefore;
            float d = start, acc = 0f;
            int seg = 1;
            while (d <= end && res.Count < max && seg < pts.Count)
            {
                float len = Vector2.Distance(pts[seg - 1], pts[seg]);
                if (d > acc + len)
                {
                    acc += len;
                    seg++;
                    continue;
                }
                var dir = len > 0.001f ? (pts[seg] - pts[seg - 1]) / len : Vector2.up;
                res.Add((pts[seg - 1] + dir * (d - acc), dir));
                d += step;
            }
            return res;
        }
    }
}
