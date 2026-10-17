using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt
{
    /// <summary>
    /// Stromausfall (GDD 5.2, Prototyp tearCable/repairCable): Das Hauptkabel unter der Decke reißt an einer
    /// zufälligen Stelle, beide Enden hängen herunter und sprühen Funken; das Bild wird auf risks.blackoutLightFactor
    /// abgedunkelt (Post-Processing), bis der Strom wieder da ist.
    /// </summary>
    public class BlackoutFx : MonoBehaviour
    {
        [SerializeField] Renderer cableMain;
        [SerializeField] Transform halfLeft;
        [SerializeField] Transform halfRight;
        [SerializeField] Transform dangleLeft;
        [SerializeField] Transform dangleRight;
        [SerializeField] ParticleSystem sparks;
        [SerializeField] Volume darkVolume;
        [SerializeField] float cableXMin = -17f;
        [SerializeField] float cableXMax = 17f;
        [SerializeField] float tearRange = 3f;
        [SerializeField] float fadeSpeed = 3f;

        float cableY, cableZ;

        public void Setup(Renderer main, Transform left, Transform right, Transform dLeft, Transform dRight, ParticleSystem sparkFx, Volume dark, float xMin, float xMax)
        {
            cableMain = main;
            halfLeft = left;
            halfRight = right;
            dangleLeft = dLeft;
            dangleRight = dRight;
            sparks = sparkFx;
            darkVolume = dark;
            cableXMin = xMin;
            cableXMax = xMax;
        }

        void Awake()
        {
            var p = cableMain.transform.position;
            cableY = p.y;
            cableZ = p.z;
            Repair();
            darkVolume.weight = 0f;
        }

        void OnEnable() => EventBus.Risk += OnRisk;
        void OnDisable() => EventBus.Risk -= OnRisk;

        void OnRisk(RiskEvent e)
        {
            if (e == RiskEvent.CableTorn) Tear(Random.Range(-tearRange, tearRange));
            else if (e == RiskEvent.PowerBack) Repair();
        }

        void Tear(float x)
        {
            cableMain.enabled = false;
            const float gap = 0.35f;
            Span(halfLeft, cableXMin, x - gap);
            Span(halfRight, x + gap, cableXMax);
            Hang(dangleLeft, x - gap - 0.1f, 0.25f);
            Hang(dangleRight, x + gap + 0.1f, -0.25f);
            sparks.transform.position = new Vector3(x, cableY - 0.8f, cableZ);
            sparks.Emit(24);
            sparks.Play();
        }

        void Repair()
        {
            cableMain.enabled = true;
            foreach (var t in new[] { halfLeft, halfRight, dangleLeft, dangleRight }) t.gameObject.SetActive(false);
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        /// <summary>Kabelstück (Einheitszylinder, Höhe 2) waagerecht zwischen x0 und x1.</summary>
        void Span(Transform t, float x0, float x1)
        {
            t.gameObject.SetActive(true);
            t.position = new Vector3((x0 + x1) * 0.5f, cableY, cableZ);
            var s = t.localScale;
            t.localScale = new Vector3(s.x, Mathf.Max(0.01f, (x1 - x0) * 0.5f), s.z);
        }

        void Hang(Transform t, float x, float tilt)
        {
            t.gameObject.SetActive(true);
            t.position = new Vector3(x, cableY - 0.4f, cableZ);
            t.rotation = Quaternion.Euler(0f, 0f, tilt * Mathf.Rad2Deg);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            bool dark = gm.State.blackoutTimer > 0f;
            darkVolume.weight = Mathf.MoveTowards(darkVolume.weight, dark ? 1f : 0f, Time.unscaledDeltaTime * fadeSpeed);
            // Abgerissene Enden sprühen ab und zu Funken
            if (dark && gm.Clock.DeltaTime > 0f && Random.value < gm.Clock.DeltaTime * 2f) sparks.Emit(4);
        }
    }
}
