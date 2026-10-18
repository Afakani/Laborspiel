using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Spielwerte rechts (GDD 7.6/7.7): Nahrung, Volk, Umwelt, Direktor-Wut, Parteikontrolle als Balken mit Zahl.
    /// Die Infektion zeigt InfectionHud darunter.
    /// </summary>
    public class WorldPanel : MonoBehaviour
    {
        [System.Serializable]
        public class Bar
        {
            public TMP_Text label;
            public TMP_Text value;
            public Image fill;
        }

        [SerializeField] TMP_Text title;
        [SerializeField] Bar food;
        [SerializeField] Bar pop;
        [SerializeField] Bar env;
        [SerializeField] Bar anger;
        [SerializeField] Bar authority;
        [SerializeField] Color good = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color mid = new Color(1f, 0.82f, 0.29f);
        [SerializeField] Color bad = new Color(0.9f, 0.2f, 0.12f);

        float timer;

        void Start()
        {
            var t = GameManager.Instance.Texts;
            title.text = t.Get("ui.hud.worldTitle");
            food.label.text = t.Get("ui.hud.food");
            pop.label.text = t.Get("ui.hud.pop");
            env.label.text = t.Get("ui.hud.env");
            anger.label.text = t.Get("ui.hud.anger");
            authority.label.text = t.Get("ui.hud.authority");
        }

        void Update()
        {
            timer -= Time.unscaledDeltaTime;
            if (timer > 0f) return;
            timer = 0.25f;
            var gm = GameManager.Instance;
            if (gm == null) return;
            var s = gm.State;
            var cfg = gm.Config;
            var w = WorldValues.Compute(s, cfg);
            Set(food, w.food, cfg.world.foodMax, false);
            Set(pop, w.pop, cfg.world.popMax, false);
            Set(env, w.env, cfg.world.envMax, false);
            Set(anger, s.anger, cfg.anger.max, true);
            Set(authority, s.authority, cfg.authority.max, true);
        }

        void Set(Bar b, float value, float max, bool higherIsWorse)
        {
            float k = max > 0f ? Mathf.Clamp01(value / max) : 0f;
            b.fill.fillAmount = k;
            float goodness = higherIsWorse ? 1f - k : k;
            b.fill.color = goodness > 0.66f ? good : goodness > 0.33f ? mid : bad;
            b.value.text = Mathf.RoundToInt(value).ToString();
        }
    }
}
