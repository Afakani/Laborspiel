using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Lampe am Bandmotor: grün im Betrieb, rot blinkend bei gerissenem Förderband (Prototyp motorLamp).</summary>
    public class BeltMotorFx : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Renderer lamp;
        [SerializeField] Color okColor = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color brokenColor = new Color(1f, 0.16f, 0.1f);
        [SerializeField] float blinkFreq = 1.6f;

        MaterialPropertyBlock mpb;

        public void Setup(Renderer lampRenderer) => lamp = lampRenderer;

        void Awake() => mpb = new MaterialPropertyBlock();

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || lamp == null) return;
            bool broken = gm.State.beltBroken;
            bool on = !broken || Mathf.Repeat(gm.Clock.Time * blinkFreq, 1f) < 0.5f;
            var c = broken ? brokenColor : okColor;
            lamp.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            mpb.SetColor(EmissionId, c * (on ? 1.6f : 0.1f));
            lamp.SetPropertyBlock(mpb);
        }
    }
}
