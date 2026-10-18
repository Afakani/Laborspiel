using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Wut-Skala neben dem Fernseher (GDD 7.2): füllt sich von unten, grün → gelb → rot nach Wut-Stufe.</summary>
    public class AngerScaleFx : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Transform fillPivot;
        [SerializeField] Renderer fill;
        [SerializeField] Color calm = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color annoyed = new Color(1f, 0.75f, 0.12f);
        [SerializeField] Color furious = new Color(1f, 0.12f, 0.08f);

        MaterialPropertyBlock mpb;
        float shown;

        public void Setup(Transform pivot, Renderer fillRenderer)
        {
            fillPivot = pivot;
            fill = fillRenderer;
        }

        void Awake() => mpb = new MaterialPropertyBlock();

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var a = gm.Config.anger;
            float anger = gm.State.anger;
            shown = Mathf.MoveTowards(shown, anger / a.max, Time.unscaledDeltaTime * 0.5f);
            var s = fillPivot.localScale;
            fillPivot.localScale = new Vector3(s.x, Mathf.Max(0.001f, shown), s.z);
            var c = anger < a.tiers[0] ? calm : anger < a.tiers[1] ? annoyed : furious;
            float pulse = anger >= a.tiers[1] ? 1f + 0.6f * Mathf.Sin(Time.unscaledTime * 6f) : 1f;
            fill.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            mpb.SetColor(EmissionId, c * 1.6f * pulse);
            fill.SetPropertyBlock(mpb);
        }
    }
}
