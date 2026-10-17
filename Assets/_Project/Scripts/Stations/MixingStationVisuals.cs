using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// 3D-Zustand der Mischstation (GDD 6.2): Rührer dreht beim Mischen, Schüssel in Farbe der Mischung,
    /// Tankfüllstände zeigen das Lager, Fläschchen im Regal in Serumfarbe.
    /// </summary>
    public class MixingStationVisuals : MonoBehaviour
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Transform stirrer;
        [SerializeField] Renderer bowlLiquid;
        [SerializeField] Transform[] tankLiquids = new Transform[3];
        [SerializeField] Renderer[] vials = new Renderer[6];
        [SerializeField] float emissionStrength = 0.5f;

        MaterialPropertyBlock mpb;
        Vector3[] tankBaseScale, tankBasePos;
        string vialKey;

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            tankBaseScale = new Vector3[tankLiquids.Length];
            tankBasePos = new Vector3[tankLiquids.Length];
            for (int i = 0; i < tankLiquids.Length; i++)
            {
                tankBaseScale[i] = tankLiquids[i].localScale;
                tankBasePos[i] = tankLiquids[i].localPosition;
            }
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var s = gm.State;
            var cfg = gm.Config;

            bool mixing = MixingLogic.IsMixing(s);
            if (mixing) stirrer.Rotate(0f, cfg.mixing.stirSpeedDeg * gm.Clock.DeltaTime, 0f, Space.Self);
            if (bowlLiquid.gameObject.activeSelf != mixing) bowlLiquid.gameObject.SetActive(mixing);
            if (mixing && s.mixPending.HasValue) Tint(bowlLiquid, SerumTexts.DoseColor(s.mixPending.Value, cfg));

            // Füllstand: voller Tank bei sliderMax Einheiten; unten bleibt der Boden stehen
            for (int i = 0; i < tankLiquids.Length; i++)
            {
                float k = Mathf.Clamp(s.substances[i] / (float)cfg.mixing.sliderMax, 0.02f, 1f);
                var sc = tankBaseScale[i];
                tankLiquids[i].localScale = new Vector3(sc.x, sc.y * k, sc.z);
                tankLiquids[i].localPosition = tankBasePos[i] - new Vector3(0f, sc.y * (1f - k), 0f);
            }

            RefreshVials(s, cfg);
        }

        void RefreshVials(GameState s, GameConfig cfg)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var serum in s.serums) sb.Append(serum.dose.Key).Append(serum.stage).Append(serum.ultimateId).Append('|');
            string key = sb.ToString();
            if (key == vialKey) return;
            vialKey = key;
            for (int i = 0; i < vials.Length; i++)
            {
                bool on = i < s.serums.Count;
                vials[i].gameObject.SetActive(on);
                if (on) Tint(vials[i], SerumTexts.Color(s.serums[i], cfg));
            }
        }

        void Tint(Renderer r, Color c)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColor, c);
            mpb.SetColor(EmissionColor, c * emissionStrength);
            r.SetPropertyBlock(mpb);
        }
    }
}
