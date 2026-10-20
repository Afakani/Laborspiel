using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using D = PlanErfuellt.EditorTools.LookDevMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Anbau hinter der Rückwand (layout.wing, layout.rooms): Rückwand der Halle mit Türöffnungen, Zimmer mit Boden,
    /// Decke, Wänden und Trennwänden, Türrahmen mit offenen Flügeln und Zimmerschild, Deckenlampen, Light Probes
    /// und Reflexionssonde. Die Einrichtung kommt aus LookDevRooms. Wände ohne Kollider (Begrenzung über WalkArea).
    /// </summary>
    public static class LabWingBuilder
    {
        /// <summary>Rückwand der Halle zwischen den Türen, Stürze über den Türen, Sockel auf beiden Seiten.</summary>
        public static void BuildHallBackWall(Transform shell, LayoutConfig L)
        {
            var r = L.room;
            float x = r.sideWallX, t = r.wallThickness, h = r.height;
            float zIn = -r.backWallZ, zc = zIn - t * 0.5f;
            float dw = L.wing.doorWidth, dh = L.wing.doorHeight;
            var doors = (L.rooms ?? new SideRoomLayout[0]).Select(d => d.doorX).OrderBy(v => v).ToList();

            float left = -x - t;
            int i = 0;
            foreach (var dx in doors.Concat(new[] { x + t + dw * 0.5f }))
            {
                float right = dx - dw * 0.5f;
                if (right - left > 0.01f)
                    WallBox(shell, "Wall_Back_" + i, new Vector3((left + right) * 0.5f, h * 0.5f, zc), new Vector3(right - left, h, t), D.WallPanel, Vector3.forward);
                if (i < doors.Count)
                    WallBox(shell, "Wall_Back_Lintel_" + i, new Vector3(dx, (dh + h) * 0.5f, zc), new Vector3(dw, h - dh, t), D.WallPanel);
                left = dx + dw * 0.5f;
                i++;
            }
        }

        public static void Build(Transform root, LayoutConfig L)
        {
            if (L.rooms == null || L.rooms.Length == 0 || L.wing.depth <= 0f) return;
            var r = L.room;
            var w = L.wing;
            float x = r.sideWallX, t = r.wallThickness, hw = w.height;
            float z0 = -L.WingZMin, z1 = -L.WingZMax;          // Unity-z: z0 an der Halle, z1 hinten
            float zMid = (z0 + z1) * 0.5f, depth = z0 - z1;
            var g = BlockoutKit.Group(root, "Wing", Vector3.zero);

            // Decke über allen Zimmern (wirft Schatten, damit das Hallenlicht nicht hereinscheint)
            var ceil = LookDevKit.Box(g, "Ceiling", new Vector3(0f, hw + 0.1f, zMid - t * 0.5f), new Vector3(2f * x + 2f * t, 0.2f, depth + t), D.Plaster);
            BlockoutKit.CastShadows(ceil, true);

            // Außenwände
            WallBoxMat(g, "Wall_WingBack", new Vector3(0f, hw * 0.5f, z1 - t * 0.5f), new Vector3(2f * x + 2f * t, hw, t), D.Plaster, null);
            WallBoxMat(g, "Wall_WingLeft", new Vector3(-x - t * 0.5f, hw * 0.5f, zMid), new Vector3(t, hw, depth), D.Plaster, null);
            WallBoxMat(g, "Wall_WingRight", new Vector3(x + t * 0.5f, hw * 0.5f, zMid), new Vector3(t, hw, depth), D.Plaster, null);

            var probes = new List<Vector3>();
            foreach (var room in L.rooms)
            {
                var rg = BlockoutKit.Group(g, "Room_" + room.id, Vector3.zero);
                float cx = (room.xMin + room.xMax) * 0.5f, width = room.xMax - room.xMin;
                var floorMat = FloorFor(room.id);
                var floor = LookDevKit.Box(rg, "Floor", new Vector3(cx, -0.1f, zMid), new Vector3(width, 0.2f, depth + 2f * t), floorMat);
                BlockoutKit.CastShadows(floor, false);

                // Trennwand rechts (außer am Rand)
                if (room.xMax < x - 0.01f)
                    WallBoxMat(rg, "Partition", new Vector3(room.xMax, hw * 0.5f, zMid), new Vector3(w.partition, hw, depth), D.Plaster, null);

                // Sockel in Zimmerfarbe an allen vier Innenseiten
                float xl = room.xMin + (room.xMin > -x + 0.01f ? w.partition * 0.5f : 0f);
                float xr = room.xMax - (room.xMax < x - 0.01f ? w.partition * 0.5f : 0f);
                var sm = SockelFor(room.id);
                Sockel(rg, new Vector3((xl + xr) * 0.5f, 0f, z0 - 0.015f), new Vector3(xr - xl, 1.2f, 0.03f), sm);
                Sockel(rg, new Vector3((xl + xr) * 0.5f, 0f, z1 + 0.015f), new Vector3(xr - xl, 1.2f, 0.03f), sm);
                Sockel(rg, new Vector3(xl + 0.015f, 0f, zMid), new Vector3(0.03f, 1.2f, depth - 0.06f), sm);
                Sockel(rg, new Vector3(xr - 0.015f, 0f, zMid), new Vector3(0.03f, 1.2f, depth - 0.06f), sm);

                Door(rg, room, L);
                int lamps = width > 6.5f ? 2 : 1;
                for (int k = 0; k < lamps; k++)
                {
                    float lx = lamps == 1 ? cx : Mathf.Lerp(room.xMin, room.xMax, (k + 1f) / 3f);
                    CeilingLamp(rg, new Vector3(lx, hw, zMid), room.id == "director" ? 16f : 20f);
                }
                for (float px = room.xMin + 1f; px < room.xMax - 0.5f; px += 2.2f)
                for (float pz = z0 - 1f; pz > z1 + 0.5f; pz -= 2.2f)
                foreach (var py in new[] { 0.6f, 2.0f, 3.5f })
                    probes.Add(new Vector3(px, py, pz));

                LookDevRooms.Build(rg, room, L);
            }

            var pg = new GameObject("WingLightProbes");
            pg.transform.SetParent(g, false);
            pg.AddComponent<LightProbeGroup>().probePositions = probes.ToArray();
            var probeGo = new GameObject("WingReflectionProbe");
            probeGo.transform.SetParent(g, false);
            probeGo.transform.position = new Vector3(0f, hw * 0.5f, zMid);
            var probe = probeGo.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Baked;
            probe.size = new Vector3(2f * x, hw, depth);
            probe.boxProjection = true;
        }

        /// <summary>Türrahmen mit Warnstreifen, zwei offene Flügel (mit Kollider) und Zimmerschild auf der Hallenseite.</summary>
        static void Door(Transform parent, SideRoomLayout room, LayoutConfig L)
        {
            float t = L.room.wallThickness, dw = L.wing.doorWidth, dh = L.wing.doorHeight;
            float zHall = -L.room.backWallZ, zRoom = zHall - t;
            var g = BlockoutKit.Group(parent, "Door", new Vector3(room.doorX, 0f, zHall - t * 0.5f));
            LookDevKit.Box(g, "Header", new Vector3(0f, dh + 0.12f, 0f), new Vector3(dw + 0.5f, 0.24f, t + 0.14f), D.SteelFrame);
            LookDevKit.Box(g, "HeaderStripe", new Vector3(0f, dh + 0.12f, t * 0.5f + 0.075f), new Vector3(dw + 0.5f, 0.2f, 0.01f), D.Hazard);
            foreach (float s in new[] { -1f, 1f })
                LookDevKit.Box(g, "Post", new Vector3(s * (dw * 0.5f + 0.11f), dh * 0.5f, 0f), new Vector3(0.22f, dh, t + 0.14f), D.SteelFrame);
            var threshold = LookDevKit.Box(g, "Threshold", new Vector3(0f, 0.01f, 0f), new Vector3(dw, 0.02f, t + 0.1f), D.FloorPlate);
            BlockoutKit.CastShadows(threshold, false);

            // Flügel: weit in den Raum geöffnet (150°), 30° vor der Wand
            float leaf = dw * 0.5f - 0.04f;
            foreach (float s in new[] { -1f, 1f })
            {
                var hinge = BlockoutKit.Group(parent, "DoorLeafHinge", new Vector3(room.doorX + s * dw * 0.5f, 0f, zRoom - 0.04f), s < 0f ? 150f : -150f);
                var leafGo = LookDevKit.Box(hinge, "Leaf", new Vector3(-s * leaf * 0.5f, (dh - 0.04f) * 0.5f, 0f), new Vector3(leaf, dh - 0.04f, 0.06f), D.GreenPaint);
                LookDevKit.Box(hinge, "Window", new Vector3(-s * leaf * 0.5f, dh * 0.68f, 0f), new Vector3(leaf * 0.5f, 0.6f, 0.07f), D.GlassBlue);
                LookDevKit.Box(hinge, "Handle", new Vector3(-s * (leaf - 0.12f), 1.05f, -0.06f), new Vector3(0.03f, 0.18f, 0.04f), D.Chrome);
                var col = leafGo.AddComponent<BoxCollider>();
                col.size = new Vector3(leaf, dh - 0.04f, 0.06f);
            }

            // Zimmerschild über der Tür (Hallenseite)
            var sign = BlockoutKit.Group(parent, "RoomSign", new Vector3(room.doorX, dh + 0.68f, zHall + 0.04f), 180f);
            LookDevKit.Box(sign, "Plate", new Vector3(0f, 0f, 0.01f), new Vector3(2.4f, 0.55f, 0.04f), D.RedPaint);
            LookDevKit.Label(sign, "Text", "ui.rooms." + room.id, new Vector3(0f, 0f, -0.02f), 3.2f, LabMaterials.CreamText);
            LookDevKit.Bolts(sign, new Vector3(-1.1f, 0f, -0.01f), new Vector3(1.1f, 0f, -0.01f), 2, Vector3.back, D.Brass, 0.02f);
        }

        /// <summary>Deckenlampe (Schirm, Kolben) mit gebackenem warmem Licht.</summary>
        static void CeilingLamp(Transform parent, Vector3 ceilingPos, float intensity)
        {
            var g = BlockoutKit.Group(parent, "CeilingLamp", ceilingPos);
            LookDevKit.Cyl(g, "Rod", new Vector3(0f, -0.2f, 0f), 0.015f, 0.4f, D.SteelDark, null, true, 8);
            BlockoutKit.MeshObj(g, "Shade", MeshFactory.Cone(0.4f, 0.3f), new Vector3(0f, -0.5f, 0f), D.GreenPaint);
            var bulb = BlockoutKit.Sphere(g, "Bulb", new Vector3(0f, -0.52f, 0f), 0.09f, D.LampBulb);
            bulb.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            var lightGo = new GameObject("RoomLamp_Baked");
            lightGo.transform.SetParent(g, false);
            lightGo.transform.localPosition = new Vector3(0f, -0.95f, 0f);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.lightmapBakeType = LightmapBakeType.Baked;
            l.color = LabMaterials.Hex("#FFC98A");
            l.intensity = intensity;
            l.range = 10f;
            l.shadows = LightShadows.Soft;
            lightGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
        }

        /// <summary>Wand aus Beton mit 1,2 m hohem Sockel auf den angegebenen Seiten (Normalen).</summary>
        static void WallBox(Transform parent, string name, Vector3 center, Vector3 size, Material sockel, params Vector3[] sockelSides) =>
            WallBoxMat(parent, name, center, size, D.WallConcrete, sockel, sockelSides);

        static void WallBoxMat(Transform parent, string name, Vector3 center, Vector3 size, Material wall, Material sockel, params Vector3[] sockelSides)
        {
            var w = LookDevKit.Box(parent, name, center, size, wall);
            BlockoutKit.CastShadows(w, true);
            foreach (var n in sockelSides)
            {
                var half = Vector3.Scale(size * 0.5f, new Vector3(Mathf.Abs(n.x), 0f, Mathf.Abs(n.z)));
                var c = new Vector3(center.x, 0f, center.z) + Vector3.Scale(n, half) + n * 0.015f;
                var s = new Vector3(Mathf.Abs(n.x) > 0f ? 0.03f : size.x, size.y < 2.2f ? size.y : 2.2f, Mathf.Abs(n.z) > 0f ? 0.03f : size.z);
                Sockel(parent, c, s, sockel);
            }
        }

        static void Sockel(Transform parent, Vector3 floorCenter, Vector3 size, Material m)
        {
            LookDevKit.Box(parent, "Sockel", floorCenter + Vector3.up * size.y * 0.5f, size, m);
        }

        static Material FloorFor(string id)
        {
            switch (id)
            {
                case "bedroom":
                case "director": return D.Wood;
                case "locker": return D.FloorPlate;
                default: return D.FloorConcrete;
            }
        }

        static Material SockelFor(string id)
        {
            switch (id)
            {
                case "locker": return D.GreenPaint;
                case "canteen": return D.CreamPlate;
                case "director": return D.Wood;
                default: return D.WallPanel;
            }
        }
    }
}
