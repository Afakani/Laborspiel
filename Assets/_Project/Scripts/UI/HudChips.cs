using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>Lager-Chips links oben (GDD 7.6): A, B, C, Gegenmittel, Seren im Regal.</summary>
    public class HudChips : MonoBehaviour
    {
        [System.Serializable]
        public class Chip
        {
            public TMP_Text symbol;
            public TMP_Text name;
            public TMP_Text count;
            public Image frame;
        }

        [SerializeField] Chip[] chips = new Chip[5];

        readonly string[] last = new string[5];

        void Start()
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            for (int i = 0; i < 3; i++)
            {
                var sub = gm.Config.substances[i];
                chips[i].symbol.text = sub.id;
                chips[i].name.text = t.Get("ui.hud.chip" + sub.id);
                if (ColorUtility.TryParseHtmlString(sub.color, out var c)) chips[i].symbol.color = c;
            }
            chips[3].symbol.text = t.Get("ui.hud.chipAntidoteSym");
            chips[3].name.text = t.Get("ui.hud.chipAntidote");
            chips[4].symbol.text = t.Get("ui.hud.chipSerumSym");
            chips[4].name.text = t.Get("ui.hud.chipSerums");
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var s = gm.State;
            var t = gm.Texts;
            for (int i = 0; i < 3; i++) Set(i, t.Format("ui.hud.chipCount", ("n", s.substances[i])));
            Set(3, t.Format("ui.hud.chipCount", ("n", s.antidote)));
            Set(4, t.Format("ui.hud.chipRack", ("n", s.serums.Count), ("max", gm.Config.mixing.rackSize)));
        }

        void Set(int i, string value)
        {
            if (last[i] == value) return;
            last[i] = value;
            chips[i].count.text = value;
        }
    }
}
