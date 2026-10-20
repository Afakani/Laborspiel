using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Licht und Stimmung nach Art Direction: warmes Hauptlicht (Mixed, Schatten), gebackene Lampen,
    /// Nebel, Post-Processing (Bloom, Vignette, Farbkorrektur warm mit grünen Lichtern), Light Probes, Reflexionssonde.
    /// </summary>
    public static class LabLightingSetup
    {
        const string ProfilePath = "Assets/_Project/Art/VFX/Lab_PostFX.asset";
        const string LightingPath = "Assets/_Project/Scenes/Lab_Lighting.lighting";

        public static void Build(Transform root, LayoutConfig L)
        {
            var lights = BlockoutKit.Group(root, "Lights", Vector3.zero);

            // Hauptlicht wie im Prototyp (Position 6, 14, 10 → z gespiegelt), fällt durch die schattenlose Decke
            var sun = NewLight(lights, "MainLight_Warm", LightType.Directional, LightmapBakeType.Mixed, LabMaterials.Hex("#FFE2B8"), 1.1f, 0f);
            sun.transform.rotation = Quaternion.LookRotation(-new Vector3(6f, 14f, -10f).normalized);
            sun.shadows = LightShadows.Soft;

            // Gebackene Lampen unter den Hängelampen
            float zBackLamps = LabEnvironmentBuilder.BackLampZ(-L.room.backWallZ);
            foreach (var p in new[] { new Vector3(-9f, 6.9f, -1.4f), new Vector3(-3f, 6.9f, -1.4f), new Vector3(3f, 6.9f, -1.4f),
                         new Vector3(9f, 6.9f, -1.4f), new Vector3(-6f, 6.9f, -8.5f), new Vector3(6f, 6.9f, -8.5f) }
                     .Concat(LabEnvironmentBuilder.BackLampXs.Select(x => new Vector3(x, 6.9f, zBackLamps))))
            {
                var l = NewLight(lights, "Lamp_Baked", LightType.Point, LightmapBakeType.Baked, LabMaterials.Hex("#FFD08A"), 22f, 16f);
                l.transform.position = p;
                l.shadows = LightShadows.Soft;
            }

            // Kühles Fensterlicht (gebacken)
            var win = L.window;
            var wl = NewLight(lights, "Window_Fill", LightType.Point, LightmapBakeType.Baked, LabMaterials.Hex("#8C9AB8"), 9f, 12f);
            wl.transform.position = new Vector3(win.x - 1.5f, win.y, -win.z);

            BuildProbes(root, L);
            BuildVolume(root);
            ApplyRenderSettings();
        }

        static Light NewLight(Transform parent, string name, LightType type, LightmapBakeType mode, Color color, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var l = go.AddComponent<Light>();
            l.type = type;
            l.lightmapBakeType = mode;
            l.color = color;
            l.intensity = intensity;
            if (range > 0f) l.range = range;
            go.AddComponent<UniversalAdditionalLightData>();
            return l;
        }

        static void BuildProbes(Transform root, LayoutConfig L)
        {
            var go = new GameObject("LightProbes");
            go.transform.SetParent(root, false);
            var group = go.AddComponent<LightProbeGroup>();
            var pts = new List<Vector3>();
            float zFront = -L.room.frontWallZ - 0.5f, zBack = -L.room.backWallZ + 0.5f;
            for (float x = -15f; x <= 15f; x += 3f)
            for (float z = zBack; z <= zFront; z += 2.5f)
            foreach (var y in new[] { 0.6f, 2.5f, 5.5f })
                pts.Add(new Vector3(x, y, z));
            group.probePositions = pts.ToArray();

            var probeGo = new GameObject("ReflectionProbe");
            probeGo.transform.SetParent(root, false);
            probeGo.transform.position = new Vector3(0f, 3f, (zFront + zBack) * 0.5f);
            var probe = probeGo.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Baked;
            probe.size = new Vector3(L.room.sideWallX * 2f, L.room.height, zFront - zBack + 1f);
            probe.boxProjection = true;
        }

        static void BuildVolume(Transform root)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                BlockoutKit.EnsureFolder(System.IO.Path.GetDirectoryName(ProfilePath));
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }
            // Komponenten jedes Mal neu setzen
            foreach (var c in profile.components.ToArray()) Object.DestroyImmediate(c, true);
            profile.components.Clear();

            var tone = Add<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.ACES);

            var bloom = Add<Bloom>(profile);
            bloom.threshold.Override(1.0f);
            bloom.intensity.Override(1.1f);
            bloom.scatter.Override(0.65f);

            var vig = Add<Vignette>(profile);
            vig.intensity.Override(0.38f);
            vig.smoothness.Override(0.45f);

            var col = Add<ColorAdjustments>(profile);
            col.postExposure.Override(0.35f);
            col.contrast.Override(12f);
            col.saturation.Override(-6f);
            col.colorFilter.Override(new Color(1f, 0.95f, 0.88f));

            var split = Add<SplitToning>(profile);
            split.shadows.Override(new Color(0.55f, 0.42f, 0.32f));
            split.highlights.Override(new Color(0.55f, 0.75f, 0.5f));
            split.balance.Override(10f);

            var grain = Add<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Medium1);
            grain.intensity.Override(0.15f);

            EditorUtility.SetDirty(profile);

            var go = new GameObject("GlobalVolume");
            go.transform.SetParent(root, false);
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = profile;
        }

        public struct FxVolumes
        {
            public Volume dark, green, blur, cinematic;
        }

        /// <summary>
        /// Zusätzliche globale Volumes für Phase 5: Stromausfall (Belichtung auf risks.blackoutLightFactor),
        /// Infektion grün (Farbfilter) und verschwommen (Gauß-Unschärfe). Alle mit Gewicht 0, die Laufzeit blendet ein.
        /// </summary>
        public static FxVolumes BuildFxVolumes(Transform root, GameConfig cfg)
        {
            var g = BlockoutKit.Group(root, "FxVolumes", Vector3.zero);

            var dark = Profile("Assets/_Project/Art/VFX/Lab_Blackout.asset");
            var exp = Add<ColorAdjustments>(dark);
            exp.postExposure.Override(Mathf.Log(Mathf.Max(0.01f, cfg.risks.blackoutLightFactor), 2f));
            exp.colorFilter.Override(new Color(0.8f, 0.85f, 1f));

            // Ultimativ-Zwischensequenz (Phase 8): Licht auf cinematic.lightFactor
            var cine = Profile("Assets/_Project/Art/VFX/Lab_Cinematic.asset");
            var cexp = Add<ColorAdjustments>(cine);
            cexp.postExposure.Override(Mathf.Log(Mathf.Max(0.01f, cfg.cinematic.lightFactor), 2f));

            var green = Profile("Assets/_Project/Art/VFX/Lab_InfectionGreen.asset");
            var tint = Add<ColorAdjustments>(green);
            tint.colorFilter.Override(new Color(0.6f, 1f, 0.55f));
            tint.hueShift.Override(25f);
            tint.saturation.Override(40f);

            var blur = Profile("Assets/_Project/Art/VFX/Lab_InfectionBlur.asset");
            var dof = Add<DepthOfField>(blur);
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(0.2f);
            dof.gaussianEnd.Override(2.5f);
            dof.gaussianMaxRadius.Override(cfg.infection.blurRadius);
            dof.highQualitySampling.Override(true);

            var v = new FxVolumes
            {
                dark = MakeVolume(g, "BlackoutVolume", dark),
                green = MakeVolume(g, "InfectionGreenVolume", green),
                blur = MakeVolume(g, "InfectionBlurVolume", blur),
                cinematic = MakeVolume(g, "CinematicVolume", cine)
            };
            g.gameObject.AddComponent<UltimateCinematic>().Setup(v.cinematic);
            g.gameObject.AddComponent<InfectionVolumes>().Setup(v.green, v.blur);
            return v;
        }

        static VolumeProfile Profile(string path)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            foreach (var c in profile.components.ToArray()) Object.DestroyImmediate(c, true);
            profile.components.Clear();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static Volume MakeVolume(Transform parent, string name, VolumeProfile profile)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 10f;
            vol.weight = 0f;
            vol.sharedProfile = profile;
            return vol;
        }

        static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var c = profile.Add<T>(true);
            c.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(c, profile);
            return c;
        }

        static void ApplyRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = LabMaterials.Hex("#2E2620");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = LabMaterials.Hex("#2A2119");
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        }

        /// <summary>Lightmapping-Einstellungen der Szene (GPU-Lightmapper, Baked Indirect).</summary>
        public static LightingSettings EnsureLightingSettings()
        {
            var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(LightingPath);
            if (ls == null)
            {
                ls = new LightingSettings { name = "Lab_Lighting" };
                AssetDatabase.CreateAsset(ls, LightingPath);
            }
            ls.bakedGI = true;
            ls.realtimeGI = false;
            ls.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            ls.mixedBakeMode = MixedLightingMode.IndirectOnly;
            ls.lightmapResolution = 8f;
            ls.lightmapPadding = 2;
            ls.lightmapMaxSize = 1024;
            ls.directionalityMode = LightmapsMode.CombinedDirectional;
            ls.maxBounces = 2;
            ls.directSampleCount = 32;
            ls.indirectSampleCount = 256;
            ls.environmentSampleCount = 128;
            ls.ao = true;
            ls.aoMaxDistance = 1f;
            ls.filteringMode = LightingSettings.FilterMode.Auto;
            EditorUtility.SetDirty(ls);
            Lightmapping.lightingSettings = ls;
            return ls;
        }
    }
}
