using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Karte im Logbuch: Skizze (Kuh oder Huhn, Größe nach A, Euter nach B, gestrichelter Rahmen bei C), Name,
    /// Dosiszeile, Vor- und Nachteile. Ultimative golden, Kuriositäten rosa, Rekorde ohne Skizze.
    /// </summary>
    public class LogbookCard : MonoBehaviour
    {
        [SerializeField] Image paper;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text sub;
        [SerializeField] TMP_Text body;
        [SerializeField] RectTransform sketchRoot;
        [SerializeField] Image animal;
        [SerializeField] Image udder;
        [SerializeField] Image frame;
        [SerializeField] Sprite cowSprite;
        [SerializeField] Sprite henSprite;
        [SerializeField] Color paperNormal = new Color(0.95f, 0.91f, 0.8f);
        [SerializeField] Color paperSecret = new Color(1f, 0.88f, 0.55f);
        [SerializeField] Color paperFun = new Color(1f, 0.85f, 0.93f);
        [SerializeField] Color paperRecords = new Color(0.9f, 0.93f, 0.85f);

        public void Setup(Image paperImg, TMP_Text titleText, TMP_Text subText, TMP_Text bodyText, RectTransform sketch,
            Image animalImg, Image udderImg, Image frameImg, Sprite cow, Sprite hen)
        {
            paper = paperImg;
            title = titleText;
            sub = subText;
            body = bodyText;
            sketchRoot = sketch;
            animal = animalImg;
            udder = udderImg;
            frame = frameImg;
            cowSprite = cow;
            henSprite = hen;
        }

        public void Fill(LogbookEntry e, string lists, GameConfig cfg)
        {
            title.text = e.title;
            sub.text = e.sub;
            body.text = lists;
            paper.color = e.section switch
            {
                LogbookSection.Secret => paperSecret,
                LogbookSection.Curiosity => paperFun,
                LogbookSection.Records => paperRecords,
                _ => paperNormal
            };
            bool sketch = e.section != LogbookSection.Records;
            sketchRoot.gameObject.SetActive(sketch);
            if (!sketch) return;
            LogbookLogic.SketchShape(e.sketch, cfg, out float scale, out int udders, out bool dashed);
            animal.sprite = e.kind == AnimalKind.Hen ? henSprite : cowSprite;
            animal.rectTransform.localScale = Vector3.one * scale / (1f + cfg.animalVisuals.growPerA * cfg.animalVisuals.growCapA);
            udder.gameObject.SetActive(e.kind == AnimalKind.Cow && udders > 0);
            udder.rectTransform.localScale = Vector3.one * (0.6f + 0.25f * udders);
            frame.gameObject.SetActive(dashed);
        }
    }
}
