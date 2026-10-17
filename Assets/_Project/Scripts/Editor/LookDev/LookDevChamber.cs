using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using L = PlanErfuellt.EditorTools.LookDevMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Glaskammer B-12 nach Bild 08: schwerer Sockel und Kopfring aus Stahl mit Flanschen und Schraubenkränzen,
    /// Messingbänder, kräftige Streben (vorn offen), doppelwandiges Glas, drei hängende Düsen mit grünen
    /// Spotlights (volumetrisch) und Lichtkegeln, Nebelpartikel innen, „B-12“ und Plakette auf dem Kopfring.
    /// </summary>
    public static class LookDevChamber
    {
        /// <summary>Baut Rahmen, Glas und Düsen; gibt die Düsenring-Renderer für den Gas-Effekt zurück.</summary>
        public static List<Renderer> Build(Transform root, ChamberLayout c)
        {
            float R = c.radius, H = c.height, y0 = c.baseY;
            var frame = BlockoutKit.Group(root, "Frame", Vector3.zero);

            // Sockel
            LookDevKit.Obj(frame, "Plinth", LookDevKit.Cylinder(R + 0.45f, 0.3f, 64), new Vector3(0f, 0.15f, 0f), L.FloorPlate);
            LookDevKit.Obj(frame, "BaseRing", LookDevKit.Ring(R + 0.2f, R + 0.6f, 0.5f, 64), new Vector3(0f, 0.25f, 0f), L.SteelFrame);
            LookDevKit.BoltCircle(frame, R + 0.4f, 0.505f, 36, L.Brass, true, 0.03f);
            BlockoutKit.MeshObj(frame, "BrassBase", MeshFactory.Torus(R + 0.05f, 0.07f), new Vector3(0f, y0 + 0.18f, 0f), L.Brass);

            // Kopfring mit Flanschen
            float topY = y0 + H + 0.35f;
            LookDevKit.Obj(frame, "HeadRing", LookDevKit.Ring(R - 0.25f, R + 0.45f, 0.8f, 64), new Vector3(0f, topY, 0f), L.SteelFrame);
            LookDevKit.Obj(frame, "FlangeLow", LookDevKit.Ring(R + 0.4f, R + 0.56f, 0.1f, 64), new Vector3(0f, topY - 0.38f, 0f), L.SteelDark);
            LookDevKit.Obj(frame, "FlangeHigh", LookDevKit.Ring(R + 0.4f, R + 0.56f, 0.1f, 64), new Vector3(0f, topY + 0.38f, 0f), L.SteelDark);
            LookDevKit.BoltCircle(frame, R + 0.56f, topY - 0.38f, 40, L.SteelDark, false, 0.025f);
            LookDevKit.BoltCircle(frame, R + 0.56f, topY + 0.38f, 40, L.SteelDark, false, 0.025f);
            LookDevKit.Obj(frame, "Ceiling", LookDevKit.Cylinder(R - 0.2f, 0.12f, 48), new Vector3(0f, topY - 0.25f, 0f), L.SteelDark);
            BlockoutKit.MeshObj(frame, "BrassTop", MeshFactory.Torus(R + 0.05f, 0.07f), new Vector3(0f, y0 + H - 0.08f, 0f), L.Brass);
            BlockoutKit.MeshObj(frame, "BrassHead", MeshFactory.Torus(R + 0.47f, 0.05f), new Vector3(0f, topY + 0.02f, 0f), L.Brass);
            foreach (var p in new[] { new Vector2(-1.2f, 0.9f), new Vector2(1.3f, 1.1f) })
                LookDevKit.Cyl(frame, "SupplyPipe", new Vector3(p.x, topY + 1.3f, p.y), 0.16f, 2.2f, L.SteelDark);

            // Streben: vorn (−Z) bleibt offen
            foreach (float deg in new[] { -35f, -145f, 15f, 165f, 60f, 120f })
            {
                float a = deg * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Cos(a) * (R + 0.08f), y0 + H * 0.5f, Mathf.Sin(a) * (R + 0.08f));
                LookDevKit.Box(frame, "Post", pos, new Vector3(0.16f, H, 0.22f), L.SteelFrame, new Vector3(0f, -deg, 0f));
                LookDevKit.Bolts(frame, pos + Vector3.up * (H * 0.5f - 0.2f), pos - Vector3.up * (H * 0.5f - 0.2f), 7,
                    new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), L.Brass, 0.018f);
            }

            // Glas (doppelwandig, damit auch die Rückseite spiegelt)
            var glass = LookDevKit.Obj(root, "Glass", LookDevKit.Ring(R - 0.03f, R, H, 96), new Vector3(0f, y0 + H * 0.5f, 0f), L.GlassClear, null, true, false);
            glass.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

            // Beschriftung: B-12 und Plakette auf dem Kopfring
            OnRing(root, "Number", "ui.signs.chamberNumber", -112f, R + 0.47f, topY, 3.6f, LabMaterials.CreamText);
            var plaque = BlockoutKit.Group(root, "Plaque", RingPos(-62f, R + 0.47f, topY));
            plaque.localEulerAngles = new Vector3(0f, RingYaw(-62f), 0f);
            LookDevKit.Box(plaque, "Plate", new Vector3(0f, 0f, 0.01f), new Vector3(1.55f, 0.42f, 0.03f), L.RedPaint);
            LookDevKit.Plane(plaque, "Emblem", new Vector3(0.6f, 0f, -0.01f), new Vector2(0.32f, 0.32f), L.EmblemCream);
            LookDevKit.Label(plaque, "Text", "ui.signs.chamberPlate", new Vector3(-0.15f, 0f, -0.012f), 1.05f, LabMaterials.CreamText);
            LookDevKit.Bolts(plaque, new Vector3(-0.72f, 0.16f, -0.006f), new Vector3(-0.72f, -0.16f, -0.006f), 2, Vector3.back, L.Brass, 0.012f);
            LookDevKit.Bolts(plaque, new Vector3(0.72f, 0.16f, -0.006f), new Vector3(0.72f, -0.16f, -0.006f), 2, Vector3.back, L.Brass, 0.012f);

            // Düsen mit Lichtkegel und Spotlicht
            var rings = new List<Renderer>();
            var nozzles = BlockoutKit.Group(root, "Nozzles", Vector3.zero);
            float headY = y0 + H - 0.45f;
            foreach (var p in new[] { new Vector2(-1.1f, -0.6f), new Vector2(0f, 0.9f), new Vector2(1.1f, -0.6f) })
            {
                var n = BlockoutKit.Group(nozzles, "Nozzle", new Vector3(p.x, headY, p.y));
                LookDevKit.Cyl(n, "Pipe", new Vector3(0f, 0.45f, 0f), 0.07f, 0.6f, L.SteelDark);
                LookDevKit.Cyl(n, "Head", new Vector3(0f, 0.12f, 0f), 0.2f, 0.24f, L.SteelPanel);
                LookDevKit.BoltCircle(n, 0.17f, 0.245f, 8, L.Brass, true, 0.012f);
                BlockoutKit.MeshObj(n, "Shroud", MeshFactory.Cone(0.27f, 0.18f), new Vector3(0f, -0.15f, 0f), L.SteelDark, false);
                rings.Add(BlockoutKit.MeshObj(n, "Ring", MeshFactory.Torus(0.18f, 0.035f), new Vector3(0f, -0.16f, 0f), LabMaterials.ChamberRing, false).GetComponent<Renderer>());
                var cone = BlockoutKit.MeshObj(n, "LightCone", MeshFactory.Cone(1.05f, 3.4f), new Vector3(0f, -0.15f, 0f), L.LightConeGreen, false, new Vector3(180f, 0f, 0f));
                cone.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                cone.GetComponent<MeshRenderer>().receiveShadows = false;

                var lightGo = new GameObject("NozzleLight");
                lightGo.transform.SetParent(n, false);
                lightGo.transform.localPosition = new Vector3(0f, -0.2f, 0f);
                lightGo.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Spot;
                light.spotAngle = 42f;
                light.innerSpotAngle = 20f;
                light.range = 7.5f;
                light.intensity = 9f;
                light.color = LabMaterials.Hex("#6DFF4A");
                light.shadows = LightShadows.None;
                light.lightmapBakeType = LightmapBakeType.Realtime;
                lightGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
            }

            BuildFog(root, R, y0, H);
            return rings;
        }

        /// <summary>Langsam aufsteigender grüner Nebel in der Kammer (weiche Partikel).</summary>
        static void BuildFog(Transform root, float R, float y0, float H)
        {
            var go = new GameObject("FogParticles");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(0f, y0 + 0.4f, 0f);
            go.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 11f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.45f, 1f, 0.4f, 0.28f);
            main.maxParticles = 70;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var em = ps.emission;
            em.rateOverTime = 7f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = R - 0.5f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0.8f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = 0.35f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = L.FogGreen;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.sortingFudge = 2f;
        }

        static Vector3 RingPos(float deg, float radius, float y)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(a) * radius, y, Mathf.Sin(a) * radius);
        }

        /// <summary>Drehung, damit die Vorderseite (−Z) nach außen zeigt.</summary>
        static float RingYaw(float deg) => -(deg + 90f);

        static void OnRing(Transform root, string name, string key, float deg, float radius, float y, float size, Color color)
        {
            var go = LookDevKit.Label(root, name, key, RingPos(deg, radius + 0.01f, y), size, color);
            go.transform.localEulerAngles = new Vector3(0f, RingYaw(deg), 0f);
        }
    }
}
