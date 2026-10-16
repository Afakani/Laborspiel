using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Überträgt Data/Json/Balancing.json in GameConfig.asset und die Einzel-Assets
    /// (UltimateSerum_*, FunRecipe_*, BuildMachineType_*). Läuft automatisch, wenn sich die JSON ändert.
    /// </summary>
    public static class BalancingImporter
    {
        public const string JsonPath = "Assets/_Project/Data/Json/Balancing.json";
        public const string ConfigPath = "Assets/_Project/Data/Config/GameConfig.asset";
        const string SerumDir = "Assets/_Project/Data/Serums";
        const string RecipeDir = "Assets/_Project/Data/Recipes";
        const string MachineDir = "Assets/_Project/Data/Config";

        static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new FieldsOnlyContractResolver(),
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Ignore
        });

        [MenuItem("Plan erfüllt/Balancing importieren")]
        public static GameConfig Import()
        {
            if (!File.Exists(JsonPath))
            {
                Debug.LogError($"BalancingImporter: {JsonPath} nicht gefunden.");
                return null;
            }

            var root = JObject.Parse(File.ReadAllText(JsonPath));
            EnsureFolder(Path.GetDirectoryName(ConfigPath));
            EnsureFolder(SerumDir);
            EnsureFolder(RecipeDir);

            var cfg = LoadOrCreate<GameConfig>(ConfigPath);
            ResetToDefaults(cfg);
            Populate(root, cfg);

            cfg.ultimates.Clear();
            var keepUlt = new HashSet<string>();
            foreach (var prop in Children(root, "ultimates"))
            {
                string path = $"{SerumDir}/UltimateSerum_{prop.Name}.asset";
                var u = LoadOrCreate<UltimateSerum>(path);
                ResetToDefaults(u);
                Populate(prop.Value, u);
                u.id = prop.Name;
                EditorUtility.SetDirty(u);
                cfg.ultimates.Add(u);
                keepUlt.Add(path);
            }
            DeleteStale<UltimateSerum>(SerumDir, keepUlt);

            cfg.funRecipes.Clear();
            var keepFun = new HashSet<string>();
            foreach (var prop in Children(root, "funRecipes"))
            {
                string path = $"{RecipeDir}/FunRecipe_{prop.Name}.asset";
                var f = LoadOrCreate<FunRecipe>(path);
                ResetToDefaults(f);
                Populate(prop.Value, f);
                f.id = prop.Name;
                EditorUtility.SetDirty(f);
                cfg.funRecipes.Add(f);
                keepFun.Add(path);
            }
            DeleteStale<FunRecipe>(RecipeDir, keepFun);

            cfg.buildMachineTypes.Clear();
            var keepMach = new HashSet<string>();
            if (root.SelectToken("buildMachines.types") is JArray types)
            {
                foreach (var t in types)
                {
                    string id = (string)t["id"];
                    string path = $"{MachineDir}/BuildMachineType_{id}.asset";
                    var m = LoadOrCreate<BuildMachineType>(path);
                    ResetToDefaults(m);
                    Populate(t, m);
                    EditorUtility.SetDirty(m);
                    cfg.buildMachineTypes.Add(m);
                    keepMach.Add(path);
                }
            }
            DeleteStale<BuildMachineType>(MachineDir, keepMach);

            cfg.comboNames.Clear();
            foreach (var prop in Children(root, "comboNames"))
                cfg.comboNames.Add(new ComboNameEntry { key = prop.Name, displayName = (string)prop.Value });

            Validate(cfg);
            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            Debug.Log($"BalancingImporter: GameConfig aktualisiert ({cfg.ultimates.Count} Ultimative, {cfg.funRecipes.Count} Kuriositäten, " +
                      $"{cfg.buildMachineTypes.Count} Maschinen, {cfg.comboNames.Count} Kombinationsnamen).");
            return cfg;
        }

        static void Populate(JToken source, object target)
        {
            using (var reader = source.CreateReader()) Serializer.Populate(reader, target);
        }

        static IEnumerable<JProperty> Children(JObject root, string key) =>
            root[key] is JObject obj ? obj.Properties() : new JProperty[0];

        /// <summary>Setzt ein Asset auf die Feld-Vorgaben zurück, damit entfernte JSON-Felder keine Altwerte behalten.</summary>
        static void ResetToDefaults<T>(T asset) where T : ScriptableObject
        {
            var fresh = ScriptableObject.CreateInstance<T>();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(fresh), asset);
            Object.DestroyImmediate(fresh);
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void DeleteStale<T>(string dir, HashSet<string> keep) where T : ScriptableObject
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { dir }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (!keep.Contains(p)) AssetDatabase.DeleteAsset(p);
            }
        }

        static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Grobe Plausibilitätsprüfung, damit Tippfehler in der JSON früh auffallen.</summary>
        static void Validate(GameConfig c)
        {
            void Check(bool ok, string msg)
            {
                if (!ok) Debug.LogError("BalancingImporter: " + msg);
            }

            Check(c.day.targets.Length == c.day.plans, "day.targets muss so viele Einträge haben wie day.plans.");
            Check(c.day.prizes.Length == c.day.plans, "day.prizes muss so viele Einträge haben wie day.plans.");
            Check(c.substances.Length == 3, "substances braucht genau 3 Einträge (A, B, C).");
            Check(c.anger.tiers.Length == 2, "anger.tiers braucht 2 Grenzen.");
            Check(c.machinesFixed.microscope.factorLevels.Length == 3, "microscope.factorLevels braucht 3 Werte.");
            Check(c.machinesFixed.microscope.qualityLevels.Length == 3, "microscope.qualityLevels braucht 3 Werte.");
            Check(c.machinesFixed.microscope.stabLevels.Length == 2, "microscope.stabLevels braucht 2 Werte.");
            Check(c.ultimates.Count > 0, "Keine ultimativen Seren gefunden.");
            Check(c.formulas.statMax > 0, "formulas.statMax fehlt.");
        }
    }
}
