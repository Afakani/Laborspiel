using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Leuchtende Pfeile auf dem Boden vom Spieler zum Ziel des aktuellen Einführungsschritts (GuidePath, auch durch
    /// die Türen der Nebenzimmer). Die Pfeile laufen langsam in Wegrichtung und blenden zum Ende hin aus.
    /// </summary>
    [RequireComponent(typeof(TutorialUI))]
    public class TutorialPath : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] Material material;
        [SerializeField] int count = 12;
        [SerializeField] float spacing = 1.6f;
        [SerializeField] float startDist = 1.4f;
        [SerializeField] float stopBefore = 1.6f;
        [SerializeField] float size = 0.75f;
        [SerializeField] float height = 0.04f;
        [SerializeField] float flowSpeed = 1.2f;
        [SerializeField] Color color = new Color(0.35f, 1f, 0.4f, 0.9f);

        TutorialUI ui;
        Transform root;
        readonly List<Renderer> arrows = new List<Renderer>();
        MaterialPropertyBlock mpb;
        Transform player;

        void Awake()
        {
            ui = GetComponent<TutorialUI>();
            mpb = new MaterialPropertyBlock();
            root = new GameObject("TutorialPathArrows").transform;
            for (int i = 0; i < count; i++)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(q.GetComponent<Collider>());
                q.name = "GuideArrow";
                q.transform.SetParent(root, false);
                q.transform.localScale = Vector3.one * size;
                var r = q.GetComponent<Renderer>();
                r.sharedMaterial = material;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                q.SetActive(false);
                arrows.Add(r);
            }
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (player == null)
            {
                var fpc = FindAnyObjectByType<FirstPersonController>();
                if (fpc != null) player = fpc.transform;
            }
            if (gm == null || player == null || !ui.Guiding || ui.Target == null)
            {
                Hide(0);
                return;
            }
            var L = gm.Config.layout;
            var from = new Vector2(player.position.x, -player.position.z);
            var to = new Vector2(ui.Target.position.x, -ui.Target.position.z);
            var pts = GuidePath.Waypoints(L, from, to);
            float flow = Mathf.Repeat(Time.unscaledTime * flowSpeed, spacing);
            var samples = GuidePath.Samples(pts, startDist + flow, spacing, stopBefore, arrows.Count);
            for (int i = 0; i < samples.Count; i++)
            {
                var (p, dir) = samples[i];
                var r = arrows[i];
                if (!r.gameObject.activeSelf) r.gameObject.SetActive(true);
                // Proto-z → Unity-z gespiegelt; Pfeilspitze (Textur oben) zeigt in Wegrichtung
                var d3 = new Vector3(dir.x, 0f, -dir.y);
                r.transform.SetPositionAndRotation(new Vector3(p.x, height, -p.y), Quaternion.LookRotation(Vector3.down, d3));
                float fadeIn = Mathf.Clamp01((i + 1f - flow / spacing) / 1.5f);
                float fadeOut = 1f - i / (float)arrows.Count;
                var c = color;
                c.a *= fadeIn * fadeOut;
                r.GetPropertyBlock(mpb);
                mpb.SetColor(ColorId, c);
                r.SetPropertyBlock(mpb);
            }
            Hide(samples.Count);
        }

        void Hide(int from)
        {
            for (int i = from; i < arrows.Count; i++)
                if (arrows[i].gameObject.activeSelf) arrows[i].gameObject.SetActive(false);
        }
    }
}
