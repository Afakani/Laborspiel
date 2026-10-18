using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using L = PlanErfuellt.EditorTools.LookDevMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Effekte der Kammer für Phase 5: Rundum-Warnleuchte auf dem Kopfring, Krisen-Emitter am Tier (Dampf, Flammen,
    /// Blasen, Säure, Schleim), Explosionswolken, Schleimflecken am Boden und Brandfleck.
    /// </summary>
    public static class LookDevChamberFx
    {
        public class Parts
        {
            public Transform alarmRotor;
            public Light alarmLight;
            public Renderer alarmDome;
            public ParticleSystem steam, heatFire, bubbles, acidSpray, slime;
            public Transform crisisRoot;
            public ParticleSystem slimeBurst, fireBurst;
            public Renderer[] splats;
            public Renderer scorch;
        }

        public static Parts Build(Transform root, ChamberLayout c, FxConfig fx)
        {
            var parts = new Parts();
            BuildAlarm(root, c, parts);
            BuildCrisis(root, parts);
            BuildExplosion(root, c, fx, parts);
            return parts;
        }

        /// <summary>Warnleuchte vorn auf dem Kopfring: Sockel, rote Glaskuppel, drehender Spot.</summary>
        static void BuildAlarm(Transform root, ChamberLayout c, Parts parts)
        {
            float topY = c.baseY + c.height + 0.35f + 0.4f;
            var g = BlockoutKit.Group(root, "Alarm", new Vector3(0f, topY, -(c.radius + 0.1f)));
            LookDevKit.Cyl(g, "Base", new Vector3(0f, 0.04f, 0f), 0.15f, 0.08f, L.SteelDark, null, true, 20);
            LookDevKit.BoltCircle(g, 0.12f, 0.085f, 6, L.Brass, true, 0.012f);
            var dome = LookDevKit.Obj(g, "Dome", LookDevKit.Lathe("AlarmDome", new[] { new Vector2(0.12f, 0f), new Vector2(0.12f, 0.1f), new Vector2(0.1f, 0.18f), new Vector2(0.05f, 0.23f), new Vector2(0f, 0.24f) }, 24),
                new Vector3(0f, 0.08f, 0f), AlarmMat(), null, false, false);
            parts.alarmDome = dome.GetComponent<Renderer>();
            var rotor = BlockoutKit.Group(g, "Rotor", new Vector3(0f, 0.17f, 0f));
            LookDevKit.Box(rotor, "Mirror", new Vector3(0f, 0f, -0.03f), new Vector3(0.12f, 0.1f, 0.01f), L.Chrome, null, false);
            var lightGo = new GameObject("Beam");
            lightGo.transform.SetParent(rotor, false);
            lightGo.transform.localEulerAngles = new Vector3(12f, 0f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Spot;
            light.spotAngle = 45f;
            light.range = 14f;
            light.intensity = 0f;
            light.color = new Color(1f, 0.12f, 0.08f);
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.enabled = false;
            lightGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
            parts.alarmRotor = rotor;
            parts.alarmLight = light;
            foreach (var tr in g.GetComponentsInChildren<Transform>(true)) BlockoutKit.SetStatic(tr.gameObject, false);
        }

        static Material AlarmMat() => BlockoutKit.Emissive("LD_AlarmDome", LabMaterials.Hex("#E0201A"), 0.2f, false);

        /// <summary>Emitter, die dem Tier folgen (Position setzt AnimalCrisisFx).</summary>
        static void BuildCrisis(Transform root, Parts parts)
        {
            var g = BlockoutKit.Group(root, "CrisisFx", Vector3.zero);
            parts.crisisRoot = g;
            parts.steam = Emitter(g, "Steam", new Vector3(0f, 1.6f, 0f), L.Steam, new Color(1f, 1f, 1f, 0.5f),
                1.2f, 1.8f, 0.6f, 1.2f, 0.4f, 1.0f, 0f, ParticleSystemShapeType.Box, new Vector3(1.4f, 0.2f, 0.6f), 2.2f);
            parts.heatFire = Emitter(g, "HeatFire", new Vector3(0f, 1.5f, 0f), L.FireAdd, Color.white,
                0.4f, 0.7f, 1.2f, 2.2f, 0.2f, 0.5f, -0.2f, ParticleSystemShapeType.Box, new Vector3(1.6f, 0.2f, 0.7f), 1.6f);
            parts.bubbles = Emitter(g, "Bubbles", new Vector3(-0.3f, 0.55f, 0f), L.Slime, new Color(0.6f, 1f, 0.4f, 0.8f),
                0.8f, 1.2f, 0.2f, 0.5f, 0.06f, 0.16f, -0.1f, ParticleSystemShapeType.Sphere, new Vector3(0.35f, 0.35f, 0.35f), 1.3f);
            parts.acidSpray = Emitter(g, "AcidSpray", new Vector3(0f, 0.9f, 0f), L.Slime, new Color(0.75f, 1f, 0.2f, 0.9f),
                0.7f, 1.1f, 2f, 4f, 0.06f, 0.14f, 1f, ParticleSystemShapeType.Sphere, new Vector3(0.5f, 0.5f, 0.5f), 1f);
            parts.slime = Emitter(g, "Slime", new Vector3(0f, 1.2f, 0f), L.Slime, new Color(0.35f, 1f, 0.25f, 0.95f),
                0.8f, 1.2f, 2f, 4f, 0.15f, 0.35f, 1f, ParticleSystemShapeType.Sphere, new Vector3(0.8f, 0.8f, 0.8f), 1f);
        }

        static void BuildExplosion(Transform root, ChamberLayout c, FxConfig fx, Parts parts)
        {
            var g = BlockoutKit.Group(root, "ExplosionFx", Vector3.zero);
            parts.slimeBurst = Emitter(g, "SlimeBurst", Vector3.zero, L.Slime, new Color(0.35f, 1f, 0.25f, 0.95f),
                1.2f, 2f, 4f, 12f, 0.2f, 0.6f, 1.2f, ParticleSystemShapeType.Sphere, Vector3.one * 0.6f, 1f);
            parts.fireBurst = Emitter(g, "FireBurst", Vector3.zero, L.FireAdd, Color.white,
                0.6f, 1.2f, 2f, 8f, 0.6f, 1.6f, -0.2f, ParticleSystemShapeType.Sphere, Vector3.one * 0.8f, 1.8f);
            foreach (var ps in new[] { parts.slimeBurst, parts.fireBurst })
            {
                var em = ps.emission;
                em.enabled = false;
            }

            var splats = new List<Renderer>();
            for (int i = 0; i < fx.slimeSplats; i++)
            {
                var q = LookDevKit.Plane(g, "Splat", Vector3.zero, new Vector2(1.6f, 1.6f), L.SplatSlime, new Vector3(90f, 0f, 0f), false);
                q.GetComponent<Renderer>().enabled = false;
                splats.Add(q.GetComponent<Renderer>());
            }
            parts.splats = splats.ToArray();
            var scorch = LookDevKit.Plane(g, "Scorch", new Vector3(0f, 0.305f, 0f), new Vector2(c.radius * 1.8f, c.radius * 1.8f), L.Scorch, new Vector3(90f, 0f, 0f), false);
            scorch.GetComponent<Renderer>().enabled = false;
            parts.scorch = scorch.GetComponent<Renderer>();
        }

        /// <summary>Partikel-Emitter (Weltraum, Rate 0 – die Laufzeit-Komponente steuert die Menge).</summary>
        internal static ParticleSystem Emitter(Transform parent, string name, Vector3 pos, Material mat, Color color,
            float life0, float life1, float speed0, float speed1, float size0, float size1, float gravity,
            ParticleSystemShapeType shapeType, Vector3 shapeSize, float growTo)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = shapeType == ParticleSystemShapeType.Box ? new Vector3(-90f, 0f, 0f) : Vector3.zero;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life0, life1);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed0, speed1);
            main.startSize = new ParticleSystem.MinMaxCurve(size0, size1);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = color;
            main.gravityModifier = gravity;
            main.maxParticles = 200;
            var em = ps.emission;
            em.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = shapeType;
            if (shapeType == ParticleSystemShapeType.Box) shape.scale = shapeSize;
            else shape.radius = shapeSize.x;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, growTo));
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.sortingFudge = 1f;
            return ps;
        }
    }
}
