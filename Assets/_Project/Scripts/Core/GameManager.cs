using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Einstiegspunkt der Spiellogik in Lab bzw. TestRange. Hält Config, Texte, Zustand und Spielzeit.
    /// Andere Systeme greifen über GameManager.Instance zu (kein Find im Update).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] GameConfig config;
        [SerializeField] TextAsset textsJson;
        [Tooltip("Größter Echtzeit-Schritt pro Frame in Sekunden (gegen Sprünge nach Rucklern).")]
        [SerializeField] float maxFrameStep = 0.1f;

        public GameConfig Config => config;
        public TextDatabase Texts { get; private set; }
        public GameState State { get; private set; }
        public GameClock Clock { get; private set; }

        /// <summary>Ein Panel (Pult, Mischstation, Bestellung …) ist offen und braucht die Maus.</summary>
        public bool PanelOpen { get; set; }
        /// <summary>Darf der Spieler laufen, sich umsehen und interagieren?</summary>
        public bool PlayerControlEnabled => !Clock.Paused && !PanelOpen;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("GameManager: zweite Instanz wird entfernt.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (config == null || textsJson == null)
            {
                Debug.LogError("GameManager: GameConfig oder Texte fehlen. Menü Plan erfüllt > Balancing importieren ausführen.");
                enabled = false;
                return;
            }

            Texts = new TextDatabase(textsJson.text, config.anger.tiers);
            GameSettings.Init(config);
            Clock = new GameClock(maxFrameStep);
            NewGame();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            Clock.Tick(Time.unscaledDeltaTime);
            if (!Clock.Paused) State.playTime += Clock.DeltaTime;
        }

        /// <summary>Neues Spiel mit Startwerten.</summary>
        public void NewGame()
        {
            State = GameState.Create(config);
            EventBus.RaiseMoneyChanged(State.Money);
            EventBus.RaiseInventoryChanged();
        }

        /// <summary>Kurzmeldung aus einem Textpfad anzeigen.</summary>
        public void Toast(string path, ToastKind kind, params (string key, object value)[] values)
        {
            EventBus.RaiseToast(Texts.Format(path, values), kind);
            EventBus.RaiseToastKey(path);
        }
    }
}
