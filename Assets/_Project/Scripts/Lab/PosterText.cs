using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Wandplakat bzw. Warnschild mit Text aus der JSON (posters[i]). Schreibweise der JSON:
    /// „|“ und „ · “ = Zeilenumbruch der Parole, „ – “ trennt Schlagzeile und kleine Unterzeile
    /// („MEHR|MILCH – für die Republik“). Beim Warnschild ist die erste Zeile die Schlagzeile, der Rest die Unterzeile.
    /// </summary>
    public class PosterText : MonoBehaviour
    {
        public const string SubSeparator = " – ";
        public const string LineSeparator = " · ";

        [SerializeField] string textKey;
        [SerializeField] TMP_Text head;
        [SerializeField] TMP_Text sub;
        // Warnschild: erste Zeile groß, weitere Zeilen klein
        [SerializeField] bool firstLineOnly;

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.Texts != null && !string.IsNullOrEmpty(textKey)) Apply(gm.Texts.Get(textKey));
        }

        /// <summary>Für Editor-Werkzeuge: Schlüssel und Textfelder setzen, Text sofort anzeigen.</summary>
        public void Setup(string key, TMP_Text headText, TMP_Text subText, string raw, bool warnSign)
        {
            textKey = key;
            head = headText;
            sub = subText;
            firstLineOnly = warnSign;
            Apply(raw);
        }

        void Apply(string raw)
        {
            Split(raw, firstLineOnly, out var h, out var s);
            head.text = h;
            if (sub != null) sub.text = s;
        }

        /// <summary>Zerlegt den JSON-Text in Schlagzeile (mit Zeilenumbrüchen) und Unterzeile.</summary>
        public static void Split(string raw, bool firstLineOnly, out string headLine, out string subLine)
        {
            raw ??= "";
            int i = raw.IndexOf(SubSeparator, System.StringComparison.Ordinal);
            headLine = (i < 0 ? raw : raw.Substring(0, i)).Replace(LineSeparator, "\n").Replace('|', '\n');
            subLine = i < 0 ? "" : raw.Substring(i + SubSeparator.Length);
            if (!firstLineOnly) return;
            int nl = headLine.IndexOf('\n');
            if (nl < 0) return;
            subLine = headLine.Substring(nl + 1) + (subLine.Length > 0 ? "\n" + subLine : "");
            headLine = headLine.Substring(0, nl);
        }
    }
}
