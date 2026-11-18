using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Tafel „Lage der Nation“ an der Wand (D157, statt Spielwerte im HUD): Nahrung, Volk, Umwelt als Zahl und Balken,
    /// grün/gelb/rot nach Anteil, aufgefrischt alle ui.boardRefreshSec Sekunden.
    /// </summary>
    public class WorldBoardFx : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] TMP_Text[] values = new TMP_Text[3];
        [SerializeField] Transform[] fills = new Transform[3];
        [SerializeField] Color good = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color mid = new Color(1f, 0.75f, 0.12f);
        [SerializeField] Color bad = new Color(0.9f, 0.2f, 0.12f);

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
            var cfg = gm.Config;
            var w = WorldValues.Compute(gm.State, cfg);
            Set(0, w.food, cfg.world.foodMax);
            Set(1, w.pop, cfg.world.popMax);
            Set(2, w.env, cfg.world.envMax);
        }

        void Set(int i, float value, float max)
        {
            float k = max > 0f ? Mathf.Clamp01(value / max) : 0f;
            values[i].text = Mathf.RoundToInt(value).ToString();
            var s = fills[i].localScale;
            fills[i].localScale = new Vector3(Mathf.Max(0.001f, k), s.y, s.z);
            var c = k > 0.66f ? good : k > 0.33f ? mid : bad;
            var r = fills[i].GetComponentInChildren<Renderer>();
            r.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            mpb.SetColor(EmissionId, c * 1.5f);
            r.SetPropertyBlock(mpb);
        }
    }
}
