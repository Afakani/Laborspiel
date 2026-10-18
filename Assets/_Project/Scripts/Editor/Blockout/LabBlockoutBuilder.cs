using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Baut den Labor-Blockout in der Szene Lab aus GameConfig.layout (GDD 3). Bestehende Stations-Prefabs bleiben
    /// erhalten, außer man wählt „Prefabs neu“ – so gehen spätere Handarbeiten an den Prefabs nicht verloren.
    /// </summary>
    public static class LabBlockoutBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/Lab.unity";
        const string PrefabDir = "Assets/_Project/Prefabs/Stations";

        [MenuItem("Plan erfüllt/Labor-Blockout bauen")]
        public static void BuildKeepPrefabs() => Build(false);

        [MenuItem("Plan erfüllt/Labor-Blockout bauen (Prefabs neu)")]
        public static void BuildNewPrefabs() => Build(true);

        [MenuItem("Plan erfüllt/Labor: Licht backen")]
        public static void Bake()
        {
            LabLightingSetup.EnsureLightingSettings();
            Lightmapping.BakeAsync();
        }

        public static void Build(bool rebuildPrefabs)
        {
            var cfg = BalancingImporter.Import();
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Project/Data/Json/Texte_de.json");
            var texts = new TextDatabase(textAsset.text, cfg.anger.tiers);
            var L = cfg.layout;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            BlockoutKit.Begin(texts);
            LabMaterials.Init();
            LookDevKit.Begin();
            LookDevMaterials.Init();

            var env = Root("--- Environment");
            var chamberRoot = Root("--- Chamber");
            var stationRoot = Root("--- Stations");
            var gridRoot = Root("--- BuildGrid");
            Clear(env);
            Clear(chamberRoot);
            Clear(stationRoot);
            Clear(gridRoot);
            var oldSun = GameObject.Find("Directional Light");
            if (oldSun != null) Object.DestroyImmediate(oldSun);

            LabEnvironmentBuilder.Build(env, L);
            LookDevHall.Build(env, L);
            LabLightingSetup.Build(env, L);
            var fxVolumes = LabLightingSetup.BuildFxVolumes(env, cfg);
            LabEnvironmentBuilder.BuildBlackoutFx(env, L, fxVolumes.dark);

            // Kammer
            var cowPrefab = AnimalBuilder.LoadOrBuild(AnimalBuilder.CowPath, rebuildPrefabs, AnimalBuilder.BuildCow);
            var henPrefab = AnimalBuilder.LoadOrBuild(AnimalBuilder.HenPath, rebuildPrefabs, AnimalBuilder.BuildHen);
            var chamber = InstantiatePrefab("Chamber", rebuildPrefabs, () => LabChamberBuilder.Build(L, cfg.chamberDoors, cfg.fx, cfg.player.radius, cowPrefab, henPrefab), chamberRoot);
            chamber.transform.position = LayoutConfig.ToUnity(L.chamber.x, 0f, L.chamber.z);
            BuildZones(chamberRoot, L);

            // Stationen
            foreach (var s in L.stations)
            {
                var st = s;
                var go = InstantiatePrefab("Station_" + s.id, rebuildPrefabs, () => LabStationFactory.Build(st), stationRoot);
                go.transform.SetPositionAndRotation(s.UnityPosition, s.UnityRotation);
            }

            // Baugitter (unsichtbar, nur Gizmo)
            var grid = new GameObject("BuildGrid");
            grid.transform.SetParent(gridRoot, false);
            grid.AddComponent<BuildGridGizmo>().SetConfig(cfg);

            SetupPlayerAndHud(cfg, rebuildPrefabs);
            LabLightingSetup.EnsureLightingSettings();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"LabBlockoutBuilder: Labor gebaut ({L.stations.Length} Stationen). Licht backen über Plan erfüllt > Labor: Licht backen.");
        }

        static GameObject InstantiatePrefab(string name, bool rebuild, System.Func<GameObject> create, Transform parent)
        {
            BlockoutKit.EnsureFolder(PrefabDir);
            string path = $"{PrefabDir}/{name}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || rebuild)
            {
                var temp = create();
                prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
                Object.DestroyImmediate(temp);
            }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(parent, false);
            return inst;
        }

        /// <summary>Unsichtbare Trigger für Pult- und Laken-Zone (Werte aus layout).</summary>
        static void BuildZones(Transform parent, LayoutConfig L)
        {
            var zones = BlockoutKit.Group(parent, "Zones", Vector3.zero);
            Zone(zones, "PultZone", -L.pultZone.absXMax, L.pultZone.absXMax, L.pultZone.zMin, L.pultZone.zMax);
            Zone(zones, "ClothZone_Left", -L.clothZone.absXMax, -L.clothZone.absXMin, L.clothZone.zMin, L.clothZone.zMax);
            Zone(zones, "ClothZone_Right", L.clothZone.absXMin, L.clothZone.absXMax, L.clothZone.zMin, L.clothZone.zMax);
        }

        static void Zone(Transform parent, string name, float x0, float x1, float z0Proto, float z1Proto)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            float zu0 = -z1Proto, zu1 = -z0Proto;
            go.transform.position = new Vector3((x0 + x1) * 0.5f, 1f, (zu0 + zu1) * 0.5f);
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(x1 - x0, 2f, zu1 - zu0);
        }

        /// <summary>Ersetzt die alte Szenenkamera durch das Player-Prefab und setzt das HUD ein.</summary>
        static void SetupPlayerAndHud(GameConfig cfg, bool rebuild)
        {
            var chars = Root("--- Characters");
            var ui = Root("--- UI");
            Clear(chars);
            Clear(ui);

            var player = InstantiateAt(PlayerBuilder.PrefabPath, rebuild, () => PlayerBuilder.Build(cfg), chars);
            var start = cfg.layout.playerStart;
            player.transform.position = LayoutConfig.ToUnity(start.x, 0f, start.z);

            var hud = InstantiateAt(HudBuilder.PrefabPath, rebuild, () => HudBuilder.Build(cfg.animal.meterSegments), ui);
            PlayerBuilder.Set(hud.GetComponent<PromptUI>(), "source", player.GetComponent<InteractionSystem>());
            var chamber = Object.FindAnyObjectByType<Chamber>();
            PlayerBuilder.Set(hud.GetComponentInChildren<PanelControl>(true), "chamber", chamber);
            PlayerBuilder.Set(hud.GetComponentInChildren<AnimalMeter>(true), "chamber", chamber);

            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            es.transform.SetParent(ui, false);

            // Systeme, die keine eigene Station haben
            var systems = Root("--- Systems");
            var oldMix = systems.Find("MixingSystem");
            if (oldMix != null) Object.DestroyImmediate(oldMix.gameObject);
            new GameObject("MixingSystem", typeof(MixingSystem)).transform.SetParent(systems, false);
            var oldDay = systems.Find("DayCycle");
            if (oldDay != null) Object.DestroyImmediate(oldDay.gameObject);
            new GameObject("DayCycle", typeof(Director), typeof(DayCycle)).transform.SetParent(systems, false);
            PlayerBuilder.Set(hud.GetComponentInChildren<QuotaPanel>(true), "chamber", chamber);
        }

        static GameObject InstantiateAt(string path, bool rebuild, System.Func<GameObject> create, Transform parent)
        {
            BlockoutKit.EnsureFolder(System.IO.Path.GetDirectoryName(path));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || rebuild)
            {
                var temp = create();
                prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
                Object.DestroyImmediate(temp);
            }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(parent, false);
            return inst;
        }

        static Transform Root(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) go = new GameObject(name);
            return go.transform;
        }

        static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Object.DestroyImmediate(t.GetChild(i).gameObject);
        }
    }
}
