using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Hubtüren der Kammer: links Einlass vom Klonband, rechts Auslass zur Verarbeitung.
    /// Die Kammer setzt nur die Wünsche (offen/zu); hier fahren die Türen in Spielzeit, Warnlampen blinken beim Fahren.
    /// </summary>
    public class ChamberDoors : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Transform leftDoor;
        [SerializeField] Transform rightDoor;
        [SerializeField] Renderer leftLamp;
        [SerializeField] Renderer rightLamp;
        [SerializeField] Color lampColor = new Color(1f, 0.55f, 0.1f);

        Vector3 leftBase, rightBase;
        MaterialPropertyBlock mpb;

        /// <summary>Öffnungsgrad 0 (zu) bis 1 (ganz offen).</summary>
        public float LeftOpen { get; private set; }
        public float RightOpen { get; private set; }
        public bool WantLeft { get; set; }
        public bool WantRight { get; set; }
        public bool BothClosed => LeftOpen <= 0f && RightOpen <= 0f;

        public void Setup(Transform left, Transform right, Renderer lampLeft, Renderer lampRight)
        {
            leftDoor = left;
            rightDoor = right;
            leftLamp = lampLeft;
            rightLamp = lampRight;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            leftBase = leftDoor.localPosition;
            rightBase = rightDoor.localPosition;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var d = gm.Config.chamberDoors;
            float dt = gm.Clock.DeltaTime;
            float t = gm.Clock.Time;

            LeftOpen = Step(LeftOpen, WantLeft, d, dt);
            RightOpen = Step(RightOpen, WantRight, d, dt);
            leftDoor.localPosition = leftBase + Vector3.up * (Smooth(LeftOpen) * d.lift);
            rightDoor.localPosition = rightBase + Vector3.up * (Smooth(RightOpen) * d.lift);

            Lamp(leftLamp, Moving(LeftOpen, WantLeft), t);
            Lamp(rightLamp, Moving(RightOpen, WantRight), t);
        }

        static float Step(float open, bool want, ChamberDoorsConfig d, float dt)
        {
            if (want) return d.openSec > 0f ? Mathf.Min(1f, open + dt / d.openSec) : 1f;
            return d.closeSec > 0f ? Mathf.Max(0f, open - dt / d.closeSec) : 0f;
        }

        static bool Moving(float open, bool want) => want ? open < 1f : open > 0f;

        static float Smooth(float k) => k * k * (3f - 2f * k);

        void Lamp(Renderer r, bool moving, float t)
        {
            if (r == null) return;
            float on = moving && Mathf.Repeat(t * 3f, 1f) < 0.5f ? 4f : 0.15f;
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, lampColor * on);
            r.SetPropertyBlock(mpb);
        }
    }
}
