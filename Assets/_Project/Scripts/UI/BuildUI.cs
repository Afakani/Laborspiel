using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Leiste des Baumodus (Prototyp #build): Titel, je Maschine Name, Preis bzw. „GESPERRT · ab Plan n“ und Wirkung,
    /// dazu Abriss. Gewähltes Werkzeug hervorgehoben, zu teure oder gesperrte Maschinen gedimmt.
    /// </summary>
    public class BuildUI : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        [System.Serializable]
        public class ToolButton
        {
            public Button button;
            public Image frame;
            public TMP_Text name;
            public TMP_Text price;
            public TMP_Text desc;
        }

        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] ToolButton[] tools = new ToolButton[5];
        [SerializeField] ToolButton sell;
        [SerializeField] Color selectedFrame = new Color(1f, 0.82f, 0.29f);
        [SerializeField] Color normalFrame = new Color(0.35f, 0.3f, 0.24f);
        [SerializeField] Color poorText = new Color(0.62f, 0.5f, 0.45f);
        [SerializeField] Color okText = new Color(1f, 0.82f, 0.29f);

        bool shown;

        void Awake()
        {
            for (int i = 0; i < tools.Length; i++)
            {
                int idx = i;
                tools[i].button.onClick.AddListener(() =>
                {
                    var b = BuildMode.Instance;
                    if (b != null && idx < b.Types.Count) b.Select(b.Types[idx].id);
                });
            }
            sell.button.onClick.AddListener(() =>
            {
                if (BuildMode.Instance != null) BuildMode.Instance.Select(BuildMode.SellTool);
            });
            root.SetActive(false);
        }

        void Update()
        {
            var b = BuildMode.Instance;
            bool want = b != null && b.Active;
            if (want != shown)
            {
                shown = want;
                root.SetActive(want);
                if (want) FillStatic(b);
            }
            if (!shown) return;

            var gm = GameManager.Instance;
            var s = gm.State;
            var t = gm.Texts;
            for (int i = 0; i < tools.Length && i < b.Types.Count; i++)
            {
                var type = b.Types[i];
                bool unlocked = BuildLogic.Unlocked(s, type);
                float price = BuildLogic.Price(s, gm.Config, type);
                tools[i].price.text = unlocked
                    ? t.Format("ui.common.money", ("n", Mathf.RoundToInt(price).ToString("N0", German)))
                    : t.Format("ui.build.locked", ("n", type.unlockPlanIdx + 1));
                tools[i].price.color = unlocked && s.Money >= price ? okText : poorText;
                tools[i].button.interactable = unlocked;
                tools[i].frame.color = b.Tool == type.id ? selectedFrame : normalFrame;
            }
            sell.frame.color = b.Tool == BuildMode.SellTool ? selectedFrame : normalFrame;
        }

        void FillStatic(BuildMode b)
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            title.text = t.Get("ui.build.title");
            for (int i = 0; i < tools.Length && i < b.Types.Count; i++)
            {
                var type = b.Types[i];
                tools[i].name.text = $"{i + 1} · {type.displayName}";
                tools[i].desc.text = Description(type, t);
            }
            sell.name.text = t.Get("ui.build.remove");
            sell.price.text = t.Format("ui.build.refund", ("n", Mathf.RoundToInt(gm.Config.buildMachines.sellRefund * 100f)));
            sell.desc.text = t.Get("ui.build.selectHint");
        }

        static string Description(BuildMachineType type, TextDatabase t)
        {
            switch (type.id)
            {
                case BuildLogic.Pump: return t.Format("ui.build.descPump", ("n", type.milkPerSec.ToString("0.#", German)));
                case BuildLogic.Layer: return t.Format("ui.build.descLayer", ("n", type.eggsPerSec.ToString("0.#", German)));
                case BuildLogic.Spray: return t.Format("ui.build.descSpray", ("n", Mathf.RoundToInt(type.intervalSec)));
                case BuildLogic.Cool: return t.Get("ui.build.descCool");
                default: return t.Get("ui.build.descDoc");
            }
        }
    }
}
