using System.Text;
using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Infobox mit Name, Vorteilen (grün) und Nachteilen (rot); unbekannte Mischungen zeigen keine Werte (GDD 4.4).
    /// Baut den Text nur neu, wenn sich der Inhalt ändert.
    /// </summary>
    public class SerumInfoBox : MonoBehaviour
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text body;
        [SerializeField] Color proColor = new Color(0.55f, 1f, 0.6f);
        [SerializeField] Color conColor = new Color(1f, 0.6f, 0.54f);

        string shownKey;

        public void Clear(string note = null)
        {
            string key = "clear|" + note;
            if (key == shownKey) return;
            shownKey = key;
            title.text = "";
            body.text = note ?? "";
        }

        /// <summary>Unbekannte Mischung.</summary>
        public void ShowUnknown(TextDatabase t)
        {
            if (shownKey == "unknown") return;
            shownKey = "unknown";
            title.text = t.Get("ui.mixing.unknownTitle");
            body.text = t.Get("ui.mixing.unknownText");
        }

        /// <summary>Bekannte Kombination mit Vor- und Nachteilen.</summary>
        public void ShowCombo(Dose d, GameConfig cfg, TextDatabase t)
        {
            string key = "combo|" + d.Key;
            if (key == shownKey) return;
            shownKey = key;
            var info = Formula.GetComboInfo(d, cfg);
            title.text = Formula.ComboName(d, cfg, t);
            body.text = Lists(ComboTexts.Effects(info.pros, t), ComboTexts.Effects(info.cons, t));
        }

        /// <summary>Ultimatives Serum (Vor-/Nachteile aus der Config).</summary>
        public void ShowUltimate(UltimateSerum u)
        {
            string key = "ult|" + u.id;
            if (key == shownKey) return;
            shownKey = key;
            title.text = u.displayName;
            body.text = Lists(u.pros, u.cons);
        }

        string Lists(System.Collections.Generic.IEnumerable<string> pros, System.Collections.Generic.IEnumerable<string> cons)
        {
            var sb = new StringBuilder();
            string pc = ColorUtility.ToHtmlStringRGB(proColor), cc = ColorUtility.ToHtmlStringRGB(conColor);
            foreach (var p in pros) sb.Append("<color=#").Append(pc).Append(">+ ").Append(p).Append("</color>\n");
            foreach (var c in cons) sb.Append("<color=#").Append(cc).Append(">− ").Append(c).Append("</color>\n");
            return sb.ToString();
        }
    }
}
