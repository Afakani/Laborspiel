using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Gemeinsame Logik der Panels (Pult, Mischstation): öffnen auf Anfrage, Maus frei, Spieler steht;
    /// schließen mit Leertaste, P/Esc oder Knopf. Es ist immer nur ein Panel offen. Inhalt wird im Takt
    /// pult.panelRefreshSec aufgefrischt (wie im Prototyp).
    /// </summary>
    public abstract class PanelBase : MonoBehaviour
    {
        [SerializeField] string panelId;
        [SerializeField] GameObject root;

        static PanelBase open;
        float refreshTimer;
        int openedFrame;

        public bool IsOpen => open == this;
        /// <summary>Id des gerade offenen Panels oder null (z. B. für die Einführung).</summary>
        public static string OpenId => open != null ? open.panelId : null;
        protected GameManager GM => GameManager.Instance;

        protected virtual void Awake() => root.SetActive(false);

        protected virtual void OnEnable()
        {
            EventBus.PanelRequested += OnRequested;
            EventBus.PanelCloseRequested += OnCloseRequested;
        }

        protected virtual void OnDisable()
        {
            EventBus.PanelRequested -= OnRequested;
            EventBus.PanelCloseRequested -= OnCloseRequested;
            if (IsOpen && GM != null) Close();
        }

        void OnRequested(string id)
        {
            if (id == panelId) Open();
        }

        void OnCloseRequested()
        {
            if (IsOpen && GM != null) Close();
        }

        public void Open()
        {
            if (open != null && open != this) open.Close();
            open = this;
            openedFrame = Time.frameCount;
            root.SetActive(true);
            GM.PanelOpen = true;
            OnOpened();
            Refresh(true);
        }

        public void Close()
        {
            if (!IsOpen) return;
            open = null;
            root.SetActive(false);
            if (GM != null) GM.PanelOpen = false;
        }

        protected virtual void Update()
        {
            if (!IsOpen) return;
            // F oder Leertaste schließt (nicht im selben Frame, in dem das Panel aufging)
            if (Time.frameCount > openedFrame && (InputReader.InteractPressed || InputReader.JumpPressed))
            {
                Close();
                return;
            }
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = GM.Config.pult.panelRefreshSec;
                Refresh(false);
            }
        }

        /// <summary>Wird beim Öffnen einmal aufgerufen (statische Texte setzen).</summary>
        protected virtual void OnOpened() { }

        /// <summary>Inhalt auffrischen; force = alles neu aufbauen.</summary>
        public abstract void Refresh(bool force);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic() => open = null;
    }
}
