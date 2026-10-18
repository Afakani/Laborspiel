using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Plantafel an der Wand (GDD 7.6): je Ware Ist/Soll als Zahl und Fortschrittsbalken, aufgefrischt alle
    /// ui.boardRefreshSec Sekunden (wie eine träge Anzeigetafel).
    /// </summary>
    public class PlanBoardFx : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        static readonly char[] Keys = { 'm', 'e', 'f' };

        [SerializeField] TMP_Text[] values = new TMP_Text[3];
        [SerializeField] Transform[] fills = new Transform[3];
        [SerializeField] Color open = new Color(1f, 0.8f, 0.16f);
        [SerializeField] Color done = new Color(0.27f, 0.82f, 0.31f);

        MaterialPropertyBlock mpb;
        float timer;

        public void Setup(TMP_Text[] valueTexts, Transform[] fillPivots)
        {
            values = valueTexts;
            fills = fillPivots;
        }

        void Awake() => mpb = new MaterialPropertyBlock();

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            timer -= Time.unscaledDeltaTime;
            if (timer > 0f) return;
            timer = gm.Config.ui.boardRefreshSec;
            Refresh(gm);
        }

        /// <summary>Sofort auffrischen (z. B. bei Schichtbeginn).</summary>
        public void Refresh(GameManager gm)
        {
            var q = gm.State.quota;
            for (int i = 0; i < 3; i++)
            {
                float v = q != null ? Quota.Get(q.prod, Keys[i]) : 0f;
                float target = q != null ? Quota.Get(q.target, Keys[i]) : 0f;
                float k = target > 0f ? Mathf.Clamp01(v / target) : 0f;
                values[i].text = q != null ? $"{Mathf.FloorToInt(v)} / {target:0} t" : "–";
                var s = fills[i].localScale;
                fills[i].localScale = new Vector3(Mathf.Max(0.001f, k), s.y, s.z);
                var r = fills[i].GetComponentInChildren<Renderer>();
                var c = k >= 1f ? done : open;
                r.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, c);
                mpb.SetColor(EmissionId, c * 1.5f);
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
