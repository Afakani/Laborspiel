using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using L = PlanErfuellt.EditorTools.LookDevMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Glaskammer B-12 nach Bild 08: schwerer Sockel und Kopfring aus Stahl mit Flanschen und Schraubenkränzen,
    /// Messingbänder, kräftige Streben (vorn offen), doppelwandiges Glas, drei hängende Düsen mit grünen
    /// Spotlichtern und Lichtkegeln, Nebelpartikel innen, „B-12“ und Plakette auf dem Kopfring.
    /// Links und rechts je eine Schleuse mit Hubtür auf der Bandachse; Feuer-Rig für die Vernichtung.
    /// </summary>
    public static class LookDevChamber
    {
        /// <summary>Teile, die die Laufzeit-Komponenten brauchen.</summary>
        public class Parts
        {
            public readonly List<Renderer> nozzleRings = new List<Renderer>();
            public readonly List<Renderer> nozzleCones = new List<Renderer>();
            public readonly List<Light> nozzleLights = new List<Light>();
            public Transform leftDoor, rightDoor;
            public Renderer leftLamp, rightLamp;
            public ParticleSystem[] fire;
            public Light fireLight;
            public Renderer fog;
        }

        public static Parts Build(Transform root, ChamberLayout c, ChamberDoorsConfig d)
        {
            float R = c.radius, H = c.height, y0 = c.baseY;
            var parts = new Parts();
            var frame = BlockoutKit.Group(root, "Frame", Vector3.zero);

            // Öffnungswinkel der Schleusen (Bandachse ±X)
            float half = Mathf.Asin(Mathf.Min(0.95f, (d.width * 0.5f + 0.06f) / R)) * Mathf.Rad2Deg;
            var arcs = new[] { (half, 180f - half), (180f + half, 360f - half) };

            // Sockel: bleibt innerhalb von playerCollisionRadius minus Spielerradius (2,8 m), damit niemand auf dem Ring steht
            LookDevKit.Obj(frame, "Plinth", LookDevKit.Cylinder(R + 0.28f, 0.3f, 64), new Vector3(0f, 0.15f, 0f), L.FloorPlate);
            foreach (var (a0, a1) in arcs)
            {
                LookDevKit.Obj(frame, "BaseRing", LookDevKit.RingArc(R - 0.02f, R + 0.3f, 0.5f, a0, a1, 40), new Vector3(0f, 0.25f, 0f), L.SteelFrame);
                LookDevKit.Obj(frame, "BrassBase", LookDevKit.RingArc(R + 0.29f, R + 0.36f, 0.1f, a0, a1, 40), new Vector3(0f, 0.42f, 0f), L.Brass);
                ArcBolts(frame, R + 0.16f, 0.505f, a0, a1, 14, 0.03f);
            }

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
                LookDevKit.Cyl(frame, "SupplyPipe", new Vector3(p.x, topY + 2.4f, p.y), 0.16f, 4.4f, L.SteelDark);

            // Streben: an den Schleusen und hinten; vorn (−Z) bleibt offen
            float post = half + 3f;
            foreach (float deg in new[] { -post, post, 180f - post, 180f + post, 65f, 115f })
            {
                float a = deg * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Cos(a) * (R + 0.08f), y0 + H * 0.5f, Mathf.Sin(a) * (R + 0.08f));
                LookDevKit.Box(frame, "Post", pos, new Vector3(0.16f, H, 0.22f), L.SteelFrame, new Vector3(0f, -deg, 0f));
                LookDevKit.Bolts(frame, pos + Vector3.up * (H * 0.5f - 0.2f), pos - Vector3.up * (H * 0.5f - 0.2f), 7,
                    new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), L.Brass, 0.018f);
            }

            // Glas (doppelwandig, damit auch die Rückseite spiegelt), mit Öffnungen für die Schleusen
            foreach (var (a0, a1) in arcs)
            {
                var glass = LookDevKit.Obj(root, "Glass", LookDevKit.RingArc(R - 0.03f, R, H, a0, a1, 48), new Vector3(0f, y0 + H * 0.5f, 0f), L.GlassClear, null, true, false);
                glass.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            }

            BuildLock(root, -1f, R, y0, half, d, parts);
            BuildLock(root, 1f, R, y0, half, d, parts);

            // Beschriftung: B-12 und Plakette auf dem Kopfring
            OnRing(root, "Number", "ui.signs.chamberNumber", -112f, R + 0.47f, topY, 3.6f, LabMaterials.CreamText);
            var plaque = BlockoutKit.Group(root, "Plaque", RingPos(-62f, R + 0.47f, topY));
            plaque.localEulerAngles = new Vector3(0f, RingYaw(-62f), 0f);
            LookDevKit.Box(plaque, "Plate", new Vector3(0f, 0f, 0.01f), new Vector3(2.1f, 0.42f, 0.03f), L.RedPaint);
            LookDevKit.Plane(plaque, "Emblem", new Vector3(0.84f, 0f, -0.01f), new Vector2(0.32f, 0.32f), L.EmblemCream);
            LookDevKit.Label(plaque, "Text", "ui.signs.chamberPlate", new Vector3(-0.17f, 0f, -0.012f), 0.85f, LabMaterials.CreamText);
            LookDevKit.Bolts(plaque, new Vector3(-0.99f, 0.16f, -0.006f), new Vector3(-0.99f, -0.16f, -0.006f), 2, Vector3.back, L.Brass, 0.012f);
            LookDevKit.Bolts(plaque, new Vector3(0.99f, 0.16f, -0.006f), new Vector3(0.99f, -0.16f, -0.006f), 2, Vector3.back, L.Brass, 0.012f);

            // Düsen mit Lichtkegel und Spotlicht
            var nozzles = BlockoutKit.Group(root, "Nozzles", Vector3.zero);
            float headY = y0 + H - 0.45f;
            var heads = new List<Vector3>();
            foreach (var p in new[] { new Vector2(-1.1f, -0.6f), new Vector2(0f, 0.9f), new Vector2(1.1f, -0.6f) })
            {
                var n = BlockoutKit.Group(nozzles, "Nozzle", new Vector3(p.x, headY, p.y));
                heads.Add(new Vector3(p.x, headY - 0.2f, p.y));
                LookDevKit.Cyl(n, "Pipe", new Vector3(0f, 0.45f, 0f), 0.07f, 0.6f, L.SteelDark);
                LookDevKit.Cyl(n, "Head", new Vector3(0f, 0.12f, 0f), 0.2f, 0.24f, L.SteelPanel);
                LookDevKit.BoltCircle(n, 0.17f, 0.245f, 8, L.Brass, true, 0.012f);
                BlockoutKit.MeshObj(n, "Shroud", MeshFactory.Cone(0.27f, 0.18f), new Vector3(0f, -0.15f, 0f), L.SteelDark, false);
                parts.nozzleRings.Add(BlockoutKit.MeshObj(n, "Ring", MeshFactory.Torus(0.18f, 0.035f), new Vector3(0f, -0.16f, 0f), LabMaterials.ChamberRing, false).GetComponent<Renderer>());
                var cone = BlockoutKit.MeshObj(n, "LightCone", MeshFactory.Cone(1.05f, 3.4f), new Vector3(0f, -0.15f, 0f), L.LightConeGreen, false, new Vector3(180f, 0f, 0f));
                cone.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                cone.GetComponent<MeshRenderer>().receiveShadows = false;
                parts.nozzleCones.Add(cone.GetComponent<Renderer>());

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
                parts.nozzleLights.Add(light);
            }

            parts.fog = BuildFog(root, R, y0);
            BuildFire(root, R, y0, heads, parts);
            return parts;
        }

        /// <summary>
        /// Schleuse auf der Bandachse (side −1 links, +1 rechts): Seitenwände und Dach vom Glas bis zur Tür,
        /// Führungsschienen, Querträger mit Antrieb und Warnlampe, Hubtür mit Rippen und Warnstreifen.
        /// </summary>
        static void BuildLock(Transform root, float side, float R, float y0, float halfDeg, ChamberDoorsConfig d, Parts parts)
        {
            string tag = side < 0f ? "L" : "R";
            var g = BlockoutKit.Group(root, "Lock_" + tag, Vector3.zero);
            float doorX = side * (R + d.radialOffset);
            float innerX = side * R * Mathf.Cos(halfDeg * Mathf.Deg2Rad);
            float outerX = doorX + side * 0.12f;
            float len = Mathf.Abs(outerX - innerX), cx = (innerX + outerX) * 0.5f;
            float halfW = d.width * 0.5f, top = y0 + d.height;

            // Tunnel
            foreach (float z in new[] { -1f, 1f })
            {
                LookDevKit.Box(g, "Wall", new Vector3(cx, y0 + d.height * 0.5f, z * (halfW + 0.06f)), new Vector3(len, d.height, 0.12f), L.SteelPanel);
                LookDevKit.Bolts(g, new Vector3(innerX + side * 0.08f, top - 0.15f, z * (halfW + 0.125f)), new Vector3(innerX + side * 0.08f, y0 + 0.2f, z * (halfW + 0.125f)), 6,
                    new Vector3(0f, 0f, z), L.Brass, 0.016f);
            }
            LookDevKit.Box(g, "Roof", new Vector3(cx, top + 0.08f, 0f), new Vector3(len, 0.16f, d.width + 0.24f), L.SteelFrame);

            // Führungsschienen und Querträger oben
            float railTop = top + d.lift + 0.35f;
            foreach (float z in new[] { -1f, 1f })
            {
                LookDevKit.Box(g, "Rail", new Vector3(doorX, railTop * 0.5f, z * (halfW + 0.14f)), new Vector3(0.24f, railTop, 0.16f), L.SteelFrame);
                LookDevKit.Box(g, "RailStripe", new Vector3(doorX + side * 0.125f, 0.6f, z * (halfW + 0.14f)), new Vector3(0.01f, 1.0f, 0.16f), L.Hazard);
            }
            LookDevKit.Box(g, "TopBeam", new Vector3(doorX, railTop + 0.12f, 0f), new Vector3(0.3f, 0.24f, d.width + 0.6f), L.SteelDark);
            LookDevKit.Box(g, "Motor", new Vector3(doorX, railTop + 0.42f, 0.55f), new Vector3(0.36f, 0.36f, 0.5f), L.GreenPaint);
            LookDevKit.Cyl(g, "Shaft", new Vector3(doorX, railTop + 0.42f, -0.2f), 0.06f, 1.0f, L.SteelPanel, new Vector3(90f, 0f, 0f), true, 12);
            var lamp = BlockoutKit.Sphere(g, "WarnLamp", new Vector3(doorX, railTop + 0.34f, -0.55f), 0.11f, DoorLamp(), false);
            lamp.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

            // Tür (wird von ChamberDoors gehoben)
            var door = BlockoutKit.Group(g, "Door_" + tag, new Vector3(doorX, y0 + d.height * 0.5f, 0f));
            float dh = d.height, dw = d.width + 0.16f;
            LookDevKit.Box(door, "Slab", Vector3.zero, new Vector3(0.1f, dh, dw), L.SteelPanel, null, false);
            for (int i = 0; i < 4; i++)
            {
                float y = -dh * 0.5f + 0.55f + i * (dh - 0.9f) / 3f;
                foreach (float f in new[] { -1f, 1f })
                    LookDevKit.Box(door, "Rib", new Vector3(f * 0.07f, y, 0f), new Vector3(0.05f, 0.1f, dw - 0.2f), L.SteelDark, null, false);
            }
            foreach (float f in new[] { -1f, 1f })
            {
                LookDevKit.Box(door, "Stripe", new Vector3(f * 0.056f, -dh * 0.5f + 0.18f, 0f), new Vector3(0.012f, 0.26f, dw), L.Hazard, null, false);
                LookDevKit.Bolts(door, new Vector3(f * 0.06f, dh * 0.5f - 0.1f, -dw * 0.5f + 0.1f), new Vector3(f * 0.06f, dh * 0.5f - 0.1f, dw * 0.5f - 0.1f), 6,
                    new Vector3(f, 0f, 0f), L.Brass, 0.016f);
            }
            LookDevKit.Box(door, "Seal", new Vector3(0f, -dh * 0.5f - 0.02f, 0f), new Vector3(0.14f, 0.05f, dw), L.Rubber, null, false);
            // Die Tür bewegt sich: nichts davon darf statisch gebatcht oder gebacken werden
            foreach (var tr in door.GetComponentsInChildren<Transform>(true)) BlockoutKit.SetStatic(tr.gameObject, false);

            if (side < 0f) { parts.leftDoor = door; parts.leftLamp = lamp.GetComponent<Renderer>(); }
            else { parts.rightDoor = door; parts.rightLamp = lamp.GetComponent<Renderer>(); }
        }

        static Material DoorLamp() => BlockoutKit.Emissive("LD_DoorLamp", LabMaterials.Hex("#FF8C1A"), 0.15f, false);

        static void ArcBolts(Transform parent, float radius, float y, float a0, float a1, int count, float r)
        {
            var mesh = LookDevKit.Cylinder(r, 0.03f, 8);
            for (int i = 0; i < count; i++)
            {
                float a = Mathf.Lerp(a0, a1, (i + 0.5f) / count) * Mathf.Deg2Rad;
                LookDevKit.Obj(parent, "Bolt", mesh, new Vector3(Mathf.Cos(a) * radius, y, Mathf.Sin(a) * radius), L.Brass, null, true, false);
            }
        }

        /// <summary>Langsam aufsteigender grüner Nebel in der Kammer (weiche Partikel).</summary>
        static Renderer BuildFog(Transform root, float R, float y0)
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
            col.color = Fade(Color.white, Color.white, 0.25f, 0.7f, 0.8f);
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = 0.35f;
            Finish(go, L.FogGreen, 2f);
            return go.GetComponent<ParticleSystemRenderer>();
        }

        /// <summary>Feuer-Rig: Flammenteppich am Boden, Stichflammen aus den Düsen, dunkler Rauch, Feuerlicht.</summary>
        static void BuildFire(Transform root, float R, float y0, List<Vector3> heads, Parts parts)
        {
            var fx = BlockoutKit.Group(root, "FireFx", Vector3.zero);
            var list = new List<ParticleSystem>();

            // Flammenteppich (Kegel nach oben)
            var floor = Particles(fx, "FireFloor", new Vector3(0f, y0 + 0.1f, 0f), new Vector3(-90f, 0f, 0f));
            Flames(floor, 0.6f, 1.2f, 1.8f, 3.8f, 0.8f, 1.8f, 170f, 320, 1.5f);
            var fs = floor.shape;
            fs.shapeType = ParticleSystemShapeType.Cone;
            fs.angle = 6f;
            fs.radius = R - 0.45f;
            list.Add(floor);

            // Stichflammen aus den Düsen (nach unten)
            foreach (var h in heads)
            {
                var jet = Particles(fx, "FireJet", h, new Vector3(90f, 0f, 0f));
                Flames(jet, 0.45f, 0.7f, 4f, 6f, 0.25f, 0.6f, 70f, 120, 2.6f);
                var js = jet.shape;
                js.shapeType = ParticleSystemShapeType.Cone;
                js.angle = 14f;
                js.radius = 0.06f;
                list.Add(jet);
            }

            // Rauch
            var smoke = Particles(fx, "FireSmoke", new Vector3(0f, y0 + 1.4f, 0f), new Vector3(-90f, 0f, 0f));
            var sm = smoke.main;
            sm.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 3.5f);
            sm.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            sm.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            sm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            sm.maxParticles = 60;
            var sem = smoke.emission;
            sem.rateOverTime = 14f;
            var ss = smoke.shape;
            ss.shapeType = ParticleSystemShapeType.Cone;
            ss.angle = 20f;
            ss.radius = R - 0.8f;
            var sc = smoke.colorOverLifetime;
            sc.enabled = true;
            sc.color = Fade(new Color(0.35f, 0.3f, 0.26f), new Color(0.12f, 0.11f, 0.1f), 0.2f, 0.6f, 0.6f);
            Finish(smoke.gameObject, L.SmokeDark, 0f);
            list.Add(smoke);

            var lightGo = new GameObject("FireLight");
            lightGo.transform.SetParent(fx, false);
            lightGo.transform.localPosition = new Vector3(0f, y0 + 1.2f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 9f;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            lightGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();

            parts.fire = list.ToArray();
            parts.fireLight = light;
        }

        static ParticleSystem Particles(Transform parent, string name, Vector3 pos, Vector3 euler)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = euler;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            return ps;
        }

        /// <summary>Flammen: Farbe hell-gelb → orange → dunkelrot, wachsend, mit Rauschen.</summary>
        static void Flames(ParticleSystem ps, float life0, float life1, float speed0, float speed1, float size0, float size1, float rate, int max, float growTo)
        {
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life0, life1);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed0, speed1);
            main.startSize = new ParticleSystem.MinMaxCurve(size0, size1);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = max;
            var em = ps.emission;
            em.rateOverTime = rate;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f), new GradientColorKey(new Color(1f, 0.55f, 0.15f), 0.35f), new GradientColorKey(new Color(0.6f, 0.12f, 0.05f), 0.85f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, growTo));
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.6f;
            noise.frequency = 1.2f;
            noise.scrollSpeed = 1.5f;
            Finish(ps.gameObject, L.FireAdd, 3f);
        }

        static Gradient Fade(Color from, Color to, float inAt, float holdAt, float holdAlpha)
        {
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(from, 0f), new GradientColorKey(to, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, inAt), new GradientAlphaKey(holdAlpha, holdAt), new GradientAlphaKey(0f, 1f) });
            return grad;
        }

        static void Finish(GameObject go, Material mat, float fudge)
        {
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.sortingFudge = fudge;
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
