using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>Kurzmeldungen (GDD 7.6: max. ui.toastMax gleichzeitig, rechts). Hört auf EventBus.Toast.</summary>
    public class ToastUI : MonoBehaviour
    {
        [SerializeField] RectTransform container;
        [SerializeField] GameObject template;
        [SerializeField] Color info = new Color(0.96f, 0.93f, 0.82f);
        [SerializeField] Color good = new Color(0.25f, 1f, 0.48f);
        [SerializeField] Color warn = new Color(1f, 0.69f, 0.13f);
        [SerializeField] Color bad = new Color(1f, 0.42f, 0.42f);

        class Entry
        {
            public GameObject go;
            public TMP_Text text;
            public Image frame;
            public CanvasGroup group;
            public float age;
        }

        readonly List<Entry> active = new List<Entry>();
        readonly Stack<Entry> pool = new Stack<Entry>();

        void OnEnable() => EventBus.Toast += Show;
        void OnDisable() => EventBus.Toast -= Show;

        void Show(string text, ToastKind kind)
        {
            var cfg = GameManager.Instance.Config.ui;
            while (active.Count >= cfg.toastMax) Recycle(active[0]);

            var e = pool.Count > 0 ? pool.Pop() : Create();
            e.go.SetActive(true);
            e.go.transform.SetAsLastSibling();
            e.text.text = text;
            var c = kind == ToastKind.Good ? good : kind == ToastKind.Warn ? warn : kind == ToastKind.Bad ? bad : info;
            e.text.color = c;
            e.frame.color = c;
            e.age = 0f;
            e.group.alpha = 1f;
            active.Add(e);
        }

        void Update()
        {
            if (active.Count == 0) return;
            var cfg = GameManager.Instance.Config.ui;
            float dt = Time.unscaledDeltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var e = active[i];
                e.age += dt;
                float fadeStart = cfg.toastSec * cfg.toastFadeFrom;
                e.group.alpha = e.age < fadeStart ? 1f : Mathf.Clamp01(1f - (e.age - fadeStart) / (cfg.toastSec - fadeStart));
                if (e.age >= cfg.toastSec) Recycle(e);
            }
        }

        Entry Create()
        {
            var go = Instantiate(template, container);
            return new Entry
            {
                go = go,
                text = go.GetComponentInChildren<TMP_Text>(true),
                frame = go.GetComponent<Image>(),
                group = go.GetComponent<CanvasGroup>()
            };
        }

        void Recycle(Entry e)
        {
            active.Remove(e);
            e.go.SetActive(false);
            pool.Push(e);
        }
    }
}
