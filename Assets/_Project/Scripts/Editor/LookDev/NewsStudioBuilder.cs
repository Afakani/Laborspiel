using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Baut die Szene NewsStudio (GDD 11, Maße nach Prototyp studio): Studiowand mit Raster, Diagramm, Senderschild,
    /// Pult mit Logo und Leiste, Sprecher (lebendig/zusammengesackt), Pflanze, Tasse, Papier, Fliegen, Führungslicht,
    /// Studiokamera. Das Studio liegt tief unter dem Labor, damit sich die Lichter nicht stören. Alle x-Werte sind
    /// gegenüber dem Prototyp gespiegelt, weil die Kamera nach −z schaut (Unity linkshändig, three.js rechtshändig).
    /// </summary>
    public static class NewsStudioBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/NewsStudio.unity";
        static readonly Vector3 Origin = new Vector3(0f, -200f, 0f);

        public static void Build()
        {
            var lab = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var root = new GameObject("NewsStudio");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = Origin;
            var t = root.transform;

            var dark = BlockoutKit.Mat("LD_StudioDark", LabMaterials.Hex("#2A0A0E"), 0f, 0.3f);
            var floorMat = BlockoutKit.Mat("LD_StudioFloor", LabMaterials.Hex("#1A0A0C"), 0f, 0.4f);
            var deskMat = BlockoutKit.Mat("LD_StudioDesk", LabMaterials.Hex("#3A1A12"), 0f, 0.35f);
            var deskTop = BlockoutKit.Mat("LD_StudioDeskTop", LabMaterials.Hex("#4A2A1C"), 0f, 0.65f);
            var red = BlockoutKit.Emissive("LD_StudioRed", LabMaterials.Hex("#D01A1A"), 0.6f, false);
            var gold = BlockoutKit.Emissive("LD_StudioGold", LabMaterials.Hex("#D4A017"), 0.3f, false);
            var skin = BlockoutKit.Mat("LD_StudioSkin", LabMaterials.Hex("#F2C9A0"), 0f, 0.2f);
            var jacket = BlockoutKit.Mat("LD_StudioJacket", LabMaterials.Hex("#3B3A36"), 0f, 0.3f);
            var white = BlockoutKit.Mat("LD_StudioWhite", LabMaterials.Hex("#F4EFE3"), 0f, 0.1f);
            var black = BlockoutKit.Mat("LD_StudioBlack", LabMaterials.Hex("#0C1422"), 0f, 0.5f);
            var hairMat = BlockoutKit.Mat("LD_StudioHair", LabMaterials.Hex("#2A2018"), 0f, 0.1f);
            var pot = BlockoutKit.Mat("LD_StudioPot", LabMaterials.Hex("#B5653A"), 0f, 0.2f);
            var leafMat = BlockoutKit.Mat("LD_StudioLeaf", LabMaterials.Hex("#2FA84A"), 0f, 0.2f);
            var sweatMat = BlockoutKit.Emissive("LD_StudioSweat", LabMaterials.Hex("#5AD1FF"), 0.5f, false);
            var wallMat = LookDevKit.Textured("LD_StudioWall", "StudioWall.png", Color.white, 0f, 0.1f, 0.15f);

            // Raum
            var floor = BlockoutKit.Quad(t, "Floor", Vector3.zero, new Vector2(40f, 30f), floorMat, false, new Vector3(90f, 0f, 0f));
            var wall = BlockoutKit.Quad(t, "Wall", new Vector3(0f, 4.2f, -3.4f), new Vector2(20f, 10f), wallMat, false, new Vector3(0f, 180f, 0f));

            // Diagramm
            var chart = BlockoutKit.Group(t, "Chart", new Vector3(-3.8f, 3.5f, -3.3f));
            BlockoutKit.Box(chart, "Frame", Vector3.zero, new Vector3(4.4f, 2.85f, 0.1f), gold, false);
            BlockoutKit.Box(chart, "Face", new Vector3(0f, 0f, 0.055f), new Vector3(4.2f, 2.65f, 0.02f), dark, false);
            var chartTitle = Text(chart, "Title", new Vector3(0f, 1.12f, 0.08f), 2.2f, LabMaterials.Hex("#E0BD90"), TextAlignmentOptions.Left, new Vector2(3.9f, 0.3f));
            BlockoutKit.Box(chart, "Baseline", new Vector3(0f, -0.35f, 0.075f), new Vector3(3.9f, 0.025f, 0.01f), gold, false);
            var barL = BlockoutKit.Group(chart, "BarLeft", new Vector3(0.85f, -0.35f, 0.07f));
            var barLR = BlockoutKit.Box(barL, "Bar", new Vector3(0f, 0.5f, 0f), new Vector3(0.85f, 1f, 0.02f), gold, false).GetComponent<Renderer>();
            var barR = BlockoutKit.Group(chart, "BarRight", new Vector3(-0.85f, -0.35f, 0.07f));
            var barRR = BlockoutKit.Box(barR, "Bar", new Vector3(0f, 0.5f, 0f), new Vector3(0.85f, 1f, 0.02f), red, false).GetComponent<Renderer>();
            var valL = Text(chart, "ValueLeft", new Vector3(0.85f, 0.5f, 0.09f), 3.2f, LabMaterials.Hex("#F1E6C8"), TextAlignmentOptions.Center, new Vector2(1.6f, 0.4f));
            var valR = Text(chart, "ValueRight", new Vector3(-0.85f, 0.5f, 0.09f), 3.2f, LabMaterials.Hex("#F1E6C8"), TextAlignmentOptions.Center, new Vector2(1.6f, 0.4f));
            var labL = Text(chart, "LabelLeft", new Vector3(0.85f, -1.12f, 0.09f), 2f, LabMaterials.Hex("#F1E6C8"), TextAlignmentOptions.Center, new Vector2(1.6f, 0.3f));
            var labR = Text(chart, "LabelRight", new Vector3(-0.85f, -1.12f, 0.09f), 2f, LabMaterials.Hex("#F1E6C8"), TextAlignmentOptions.Center, new Vector2(1.6f, 0.3f));

            // Senderschild
            var sign = BlockoutKit.Group(t, "Sign", new Vector3(3.9f, 3.7f, -3.3f));
            BlockoutKit.Box(sign, "Frame", Vector3.zero, new Vector3(3.8f, 1.6f, 0.1f), gold, false);
            BlockoutKit.Box(sign, "Face", new Vector3(0f, 0f, 0.055f), new Vector3(3.6f, 1.4f, 0.02f), dark, false);
            BlockoutKit.Box(sign, "LogoBox", new Vector3(1.2f, 0.18f, 0.07f), new Vector3(0.95f, 0.55f, 0.02f), red, false);
            var logo = Text(sign, "Logo", new Vector3(1.2f, 0.18f, 0.09f), 5f, Color.white, TextAlignmentOptions.Center, new Vector2(0.95f, 0.55f));
            var signName = Text(sign, "Name", new Vector3(-0.45f, 0.2f, 0.09f), 2.6f, LabMaterials.Hex("#F4EFE3"), TextAlignmentOptions.Left, new Vector2(2.2f, 0.7f));
            var liveBox = BlockoutKit.Box(sign, "LiveBox", new Vector3(1.05f, -0.42f, 0.07f), new Vector3(1.2f, 0.26f, 0.02f), red, false).GetComponent<Renderer>();
            var live = Text(sign, "Live", new Vector3(1.05f, -0.42f, 0.09f), 2f, Color.white, TextAlignmentOptions.Center, new Vector2(1.2f, 0.26f));

            // Pult mit Logo und roter Leiste
            BlockoutKit.Box(t, "Desk", new Vector3(0f, 0.5f, 0.95f), new Vector3(5.8f, 1.0f, 1.5f), deskMat, false);
            BlockoutKit.Box(t, "DeskTop", new Vector3(0f, 1.04f, 0.95f), new Vector3(6.0f, 0.12f, 1.7f), deskTop, false);
            BlockoutKit.Box(t, "DeskLogoPlate", new Vector3(0f, 0.52f, 1.705f), new Vector3(2.4f, 0.8f, 0.01f), dark, false);
            Text(t, "DeskLogo", new Vector3(0f, 0.52f, 1.715f), 7f, LabMaterials.Hex("#D01A1A"), TextAlignmentOptions.Center, new Vector2(2.4f, 0.8f), "ui.news.logo");
            var strip = BlockoutKit.Box(t, "Strip", new Vector3(0f, 0.98f, 1.72f), new Vector3(5.8f, 0.04f, 0.04f), red, false).GetComponent<Renderer>();

            // Sprecher
            var pivot = BlockoutKit.Group(t, "Anchor", new Vector3(0f, 1.1f, -0.1f));
            var skinR = new List<Renderer>();
            BlockoutKit.Box(pivot, "Torso", new Vector3(0f, 0.55f, 0f), new Vector3(1.15f, 1.1f, 0.55f), jacket, false);
            BlockoutKit.Box(pivot, "Shirt", new Vector3(0f, 0.6f, 0.276f), new Vector3(0.34f, 0.8f, 0.02f), white, false);
            BlockoutKit.Box(pivot, "Tie", new Vector3(0f, 0.5f, 0.29f), new Vector3(0.1f, 0.55f, 0.03f), red, false);
            BlockoutKit.Box(pivot, "Knot", new Vector3(0f, 0.84f, 0.29f), new Vector3(0.13f, 0.1f, 0.04f), red, false);
            skinR.Add(BlockoutKit.Cyl(pivot, "Neck", new Vector3(0f, 1.2f, 0f), 0.13f, 0.3f, skin, false).GetComponent<Renderer>());
            Transform Arm(float sx)
            {
                var a = BlockoutKit.Group(pivot, sx < 0 ? "ArmL" : "ArmR", new Vector3(sx * 0.68f, 1.0f, 0f));
                BlockoutKit.Box(a, "Sleeve", new Vector3(0f, -0.42f, 0f), new Vector3(0.22f, 0.9f, 0.22f), jacket, false);
                skinR.Add(BlockoutKit.Sphere(a, "Hand", new Vector3(0f, -0.92f, 0f), 0.1f, skin, false).GetComponent<Renderer>());
                return a;
            }
            var armL = Arm(-1f);
            var armR = Arm(1f);
            var head = BlockoutKit.Group(pivot, "Head", new Vector3(0f, 1.62f, 0.02f));
            var skull = BlockoutKit.Sphere(head, "Skull", Vector3.zero, 0.4f, skin, false);
            skull.transform.localScale = new Vector3(0.8f, 0.86f, 0.8f);
            skinR.Add(skull.GetComponent<Renderer>());
            var hair = BlockoutKit.Sphere(head, "Hair", new Vector3(0f, 0.1f, -0.06f), 0.43f, hairMat, false);
            hair.transform.localScale = new Vector3(0.86f, 0.62f, 0.84f);
            foreach (float sx in new[] { -1f, 1f })
                skinR.Add(BlockoutKit.Sphere(head, "Ear", new Vector3(sx * 0.4f, 0f, 0f), 0.07f, skin, false).GetComponent<Renderer>());
            skinR.Add(BlockoutKit.Sphere(head, "Nose", new Vector3(0f, -0.01f, 0.4f), 0.06f, skin, false).GetComponent<Renderer>());
            var eyesA = BlockoutKit.Group(head, "EyesAlive", Vector3.zero);
            foreach (float sx in new[] { -1f, 1f })
                BlockoutKit.Sphere(eyesA, "Eye", new Vector3(sx * 0.15f, 0.07f, 0.35f), 0.05f, black, false);
            var mouthA = BlockoutKit.Group(head, "MouthAlive", new Vector3(0f, -0.13f, 0.36f));
            BlockoutKit.MeshObj(mouthA, "Smile", MeshFactory.Torus(0.1f, 0.016f), Vector3.zero, black, false, new Vector3(90f, 0f, 0f));
            mouthA.localScale = new Vector3(1f, 0.45f, 0.4f);
            var eyesD = BlockoutKit.Group(head, "EyesDead", Vector3.zero);
            foreach (float sx in new[] { -1f, 1f })
                foreach (float a in new[] { 45f, -45f })
                    BlockoutKit.Box(eyesD, "Cross", new Vector3(sx * 0.15f, 0.07f, 0.375f), new Vector3(0.17f, 0.028f, 0.02f), black, false, new Vector3(0f, 0f, a));
            var mouthD = BlockoutKit.Sphere(head, "MouthDead", new Vector3(0f, -0.15f, 0.37f), 0.07f, black, false);
            mouthD.transform.localScale = new Vector3(0.18f, 0.11f, 0.07f);
            var sweat = BlockoutKit.Sphere(head, "Sweat", new Vector3(-0.34f, 0.22f, 0.2f), 0.035f, sweatMat, false);

            // Pflanze, Tasse, Papier
            var plant = BlockoutKit.Group(t, "Plant", new Vector3(2.3f, 1.1f, 1.0f));
            BlockoutKit.Cyl(plant, "Pot", new Vector3(0f, 0.14f, 0f), 0.18f, 0.28f, pot, false);
            var leaves = new List<Transform>();
            var leafR = new List<Renderer>();
            for (int i = 0; i < 6; i++)
            {
                var spin = BlockoutKit.Group(plant, "Spin", new Vector3(0f, 0.28f, 0f), i * 60f);
                var tilt = BlockoutKit.Group(spin, "Tilt", Vector3.zero);
                tilt.localEulerAngles = new Vector3(0f, 0f, (0.3f + (i % 3) * 0.1f) * Mathf.Rad2Deg);
                var leaf = BlockoutKit.Sphere(tilt, "Leaf", new Vector3(0f, 0.34f, 0f), 1f, leafMat, false);
                leaf.transform.localScale = new Vector3(0.14f, 0.72f, 0.05f);
                leaves.Add(tilt);
                leafR.Add(leaf.GetComponent<Renderer>());
            }
            BlockoutKit.Cyl(t, "Cup", new Vector3(-2.3f, 1.2f, 1.0f), 0.09f, 0.2f, white, false);
            BlockoutKit.Box(t, "Paper", new Vector3(-0.95f, 1.106f, 1.05f), new Vector3(0.7f, 0.012f, 0.5f), white, false, new Vector3(0f, -14f, 0f));
            var flies = new List<Transform>();
            for (int i = 0; i < 4; i++)
            {
                var fly = BlockoutKit.Sphere(t, "Fly", Vector3.zero, 0.022f, black, false);
                fly.SetActive(false);
                flies.Add(fly.transform);
            }

            // Licht und Kamera
            var keyGo = new GameObject("KeyLight");
            keyGo.transform.SetParent(t, false);
            keyGo.transform.localPosition = new Vector3(-3f, 6f, 6f);
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Point;
            key.range = 30f;
            key.intensity = 40f;
            key.shadows = LightShadows.Soft;
            key.lightmapBakeType = LightmapBakeType.Realtime;
            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(t, false);
            fillGo.transform.localPosition = new Vector3(4f, 3f, 5f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.range = 20f;
            fill.intensity = 8f;
            fill.color = LabMaterials.Hex("#FFB07A");
            fill.shadows = LightShadows.None;

            var camGo = new GameObject("StudioCamera");
            camGo.transform.SetParent(t, false);
            camGo.transform.localPosition = new Vector3(0f, 2.7f, 8.6f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 38f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 60f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = LabMaterials.Hex("#120406");
            camGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            cam.enabled = false;

            foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            foreach (var tr in root.GetComponentsInChildren<Transform>(true)) tr.gameObject.isStatic = false;

            var studio = root.AddComponent<NewsStudio>();
            var so = new SerializedObject(studio);
            void Set(string f, Object v) => so.FindProperty(f).objectReferenceValue = v;
            void SetArr<T>(string f, IList<T> items) where T : Object
            {
                var p = so.FindProperty(f);
                p.arraySize = items.Count;
                for (int i = 0; i < items.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
            Set("cam", cam);
            Set("pivot", pivot);
            Set("head", head);
            Set("armL", armL);
            Set("armR", armR);
            Set("eyesAlive", eyesA.gameObject);
            Set("mouthAlive", mouthA.gameObject);
            Set("eyesDead", eyesD.gameObject);
            Set("mouthDead", mouthD);
            Set("sweat", sweat.transform);
            SetArr("skin", skinR);
            SetArr("leaves", leaves);
            SetArr("leafRenderers", leafR);
            SetArr("flies", flies);
            Set("key", key);
            Set("strip", strip);
            Set("wall", wall.GetComponent<Renderer>());
            Set("chartTitle", chartTitle);
            Set("barLeft", barL);
            Set("barRight", barR);
            Set("barLeftRenderer", barLR);
            Set("barRightRenderer", barRR);
            Set("valueLeft", valL);
            Set("valueRight", valR);
            Set("labelLeft", labL);
            Set("labelRight", labR);
            Set("signLogo", logo);
            Set("signName", signName);
            Set("liveText", live);
            Set("liveBox", liveBox);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            SceneManager.SetActiveScene(lab);

            // Beide Szenen in die Build-Einstellungen
            var list = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(lab.path, true), new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = list.ToArray();
        }

        /// <summary>Weltraum-Text ohne Billboard; key = fester Text aus der JSON, sonst leer (setzt NewsStudio).</summary>
        static TMP_Text Text(Transform parent, string name, Vector3 pos, float size, Color color, TextAlignmentOptions align, Vector2 box, string key = null)
        {
            var go = LookDevKit.Label(parent, name, key ?? "ui.news.logo", pos, size, color, new Vector3(0f, 180f, 0f), align);
            if (key == null) Object.DestroyImmediate(go.GetComponent<StationSign>());
            var tmp = go.GetComponentInChildren<TMP_Text>();
            tmp.rectTransform.sizeDelta = box;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            if (key == null) tmp.text = "";
            return tmp;
        }
    }
}
