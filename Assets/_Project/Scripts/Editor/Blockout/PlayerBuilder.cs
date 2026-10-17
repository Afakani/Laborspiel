using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Erzeugt das Player-Prefab: CharacterController, Ego-Steuerung, Interaktion, Kamera mit Post-Processing
    /// und Ego-Hände mit Spritze (Maße wie im Prototyp, z gespiegelt).
    /// </summary>
    public static class PlayerBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";

        public static GameObject Build(GameConfig cfg)
        {
            var P = cfg.player;
            var root = new GameObject("Player");
            var cc = root.AddComponent<CharacterController>();
            cc.radius = P.radius;
            cc.height = P.height;
            cc.center = new Vector3(0f, P.height * 0.5f, 0f);
            cc.stepOffset = 0.2f;
            cc.skinWidth = 0.02f;

            var rig = new GameObject("CameraRig").transform;
            rig.SetParent(root.transform, false);
            rig.localPosition = new Vector3(0f, cfg.layout.playerStart.eyeHeight, 0f);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(rig, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 80f;
            cam.fieldOfView = P.fov;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = LabMaterials.Hex("#1C1612");
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;

            var fps = root.AddComponent<FirstPersonController>();
            var interaction = root.AddComponent<InteractionSystem>();
            root.AddComponent<CameraShake>();
            root.AddComponent<InfectionSystem>();
            Set(fps, "cameraRig", rig);
            Set(fps, "cam", cam);

            BuildHands(camGo.transform, fps, interaction);
            return root;
        }

        static void BuildHands(Transform cam, FirstPersonController fps, InteractionSystem interaction)
        {
            var sleeve = BlockoutKit.Mat("Sleeve", LabMaterials.Hex("#4F5238"), 0f, 0.05f);
            var skin = BlockoutKit.Mat("Skin", LabMaterials.Hex("#F2C9A0"), 0f, 0.35f);
            var glass = BlockoutKit.Transparent("SyringeGlass", new Color(0.87f, 0.92f, 0.94f, 0.4f), 0.9f);
            var liquid = BlockoutKit.Emissive("SyringeLiquid", LabMaterials.Hex("#6DFF4A"), 0.55f, false);
            var white = BlockoutKit.Mat("SyringeParts", LabMaterials.Hex("#DDDDDD"), 0.2f, 0.5f);
            var needle = BlockoutKit.Mat("Needle", LabMaterials.Hex("#BBBBBB"), 0.9f, 0.8f);

            var vm = BlockoutKit.Group(cam, "ViewModel", Vector3.zero);
            var right = BlockoutKit.Group(vm, "RightHand", new Vector3(0.3f, -0.3f, 0.5f));
            right.localEulerAngles = new Vector3(-5.7f, 4.6f, 0f);
            BlockoutKit.Cyl(right, "Sleeve", new Vector3(0f, 0f, -0.3f), 0.064f, 0.75f, sleeve, false, new Vector3(90f, 0f, 0f));
            var hand = BlockoutKit.Sphere(right, "Hand", new Vector3(0f, 0.005f, 0.1f), 0.062f, skin, false);
            hand.transform.localScale = new Vector3(0.124f, 0.105f, 0.167f);
            BlockoutKit.Sphere(right, "Thumb", new Vector3(0.05f, 0.03f, 0.17f), 0.03f, skin, false);

            var syringe = BlockoutKit.Group(right, "Syringe", new Vector3(0f, 0.055f, 0.16f));
            BlockoutKit.Cyl(syringe, "Barrel", new Vector3(0f, 0f, 0.1f), 0.02f, 0.22f, glass, false, new Vector3(90f, 0f, 0f));
            BlockoutKit.Cyl(syringe, "Liquid", new Vector3(0f, 0f, 0.1f), 0.015f, 0.16f, liquid, false, new Vector3(90f, 0f, 0f));
            var plunger = BlockoutKit.Group(syringe, "Plunger", new Vector3(0f, 0f, -0.05f));
            BlockoutKit.Cyl(plunger, "Rod", Vector3.zero, 0.008f, 0.12f, white, false, new Vector3(90f, 0f, 0f));
            BlockoutKit.Box(plunger, "Grip", new Vector3(0f, 0f, -0.06f), new Vector3(0.07f, 0.012f, 0.012f), white, false);
            BlockoutKit.Cyl(syringe, "Needle", new Vector3(0f, 0f, 0.29f), 0.003f, 0.14f, needle, false, new Vector3(90f, 0f, 0f));

            var left = BlockoutKit.Group(vm, "LeftHand", new Vector3(-0.3f, -0.34f, 0.5f));
            left.localEulerAngles = new Vector3(-6.9f, -5.7f, 0f);
            BlockoutKit.Cyl(left, "Sleeve", new Vector3(0f, 0f, -0.3f), 0.064f, 0.75f, sleeve, false, new Vector3(90f, 0f, 0f));
            var hand2 = BlockoutKit.Sphere(left, "Hand", new Vector3(0f, 0.005f, 0.09f), 0.062f, skin, false);
            hand2.transform.localScale = new Vector3(0.124f, 0.105f, 0.167f);
            BlockoutKit.Cyl(left, "Armband", new Vector3(0f, 0f, -0.1f), 0.066f, 0.05f, LabMaterials.PropRedBright, false, new Vector3(90f, 0f, 0f));

            // Hände werfen und empfangen keine Schatten, damit sie nicht flackern
            foreach (var r in vm.GetComponentsInChildren<MeshRenderer>())
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
            }

            var hands = vm.gameObject.AddComponent<ViewModelHands>();
            Set(hands, "controller", fps);
            Set(hands, "interaction", interaction);
            Set(hands, "rightHand", right);
            Set(hands, "leftHand", left);
            Set(hands, "plunger", plunger);
        }

        /// <summary>Privates Serialisierungsfeld setzen.</summary>
        public static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
