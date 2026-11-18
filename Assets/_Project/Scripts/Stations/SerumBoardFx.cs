using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Whiteboard im Lager (D157, statt Chips im HUD): Bestand an A-17, B-08, C-04, Gegenmittel und Seren im Regal.
    /// Farbpunkte in den Serumfarben; aufgefrischt bei jeder Bestandsänderung.
    /// </summary>
    public class SerumBoardFx : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] TMP_Text[] lines = new TMP_Text[5];
        [SerializeField] Renderer[] dots = new Renderer[3];

        float timer;

        public void Setup(TMP_Text[] lineTexts, Renderer[] dotRenderers)
        {
            lines = lineTexts;
            dots = dotRenderers;
        }

        void OnEnable() => EventBus.InventoryChanged += Refresh;
        void OnDisable() => EventBus.InventoryChanged -= Refresh;

        void Start()
        {
            var subs = GameManager.Instance.Config.substances;
            var mpb = new MaterialPropertyBlock();
            for (int i = 0; i < dots.Length && i < subs.Length; i++)
            {
                if (!ColorUtility.TryParseHtmlString(subs[i].color, out var c)) continue;
                dots[i].GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, c);
                dots[i].SetPropertyBlock(mpb);
            }
            Refresh();
        }

        // Rückfall: auch ohne Ereignis (z. B. nach dem Laden) regelmäßig auffrischen
        void Update()
        {
            timer -= Time.unscaledDeltaTime;
            if (timer > 0f) return;
            timer = GameManager.Instance.Config.ui.boardRefreshSec;
            Refresh();
        }

        void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var s = gm.State;
            var t = gm.Texts;
            var subs = gm.Config.substances;
            for (int i = 0; i < 3; i++)
                lines[i].text = t.Format("ui.signs.serumLine", ("name", t.Get("ui.hud.chip" + subs[i].id)), ("n", s.substances[i]));
            lines[3].text = t.Format("ui.signs.serumAntidote", ("n", s.antidote));
            lines[4].text = t.Format("ui.signs.serumRack", ("n", s.serums.Count), ("max", gm.Config.mixing.rackSize));
        }
    }
}
