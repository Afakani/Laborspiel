using System;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Zentrale Spielereignisse. Systeme lösen aus, UI und andere Systeme hören zu – keine Querverweise nötig.
    /// </summary>
    public static class EventBus
    {
        public static event Action<float> MoneyChanged;
        public static event Action InventoryChanged;
        public static event Action SerumRackChanged;
        public static event Action<Serum> SerumMixed;
        public static event Action<AnimalState> AnimalStateChanged;
        public static event Action Explosion;
        public static event Action QuotaMet;
        public static event Action DayEnded;
        public static event Action<bool> InspectionStarted;
        public static event Action<string, ToastKind> Toast;
        public static event Action<string> DirectorSpeak;
        public static event Action<bool> PauseChanged;
        public static event Action<float> TimeScaleChanged;
        public static event Action PanelCloseRequested;
        public static event Action<string> PanelRequested;
        public static event Action<int> LeverPulled;
        public static event Action<char, float> Delivered;
        public static event Action<Color> SerumSprayed;
        public static event Action<bool> ClothChanged;
        public static event Action<RiskEvent> Risk;
        public static event Action<bool> InfectionChanged;
        public static event Action<char> Overfulfilled;
        public static event Action<GameOverKind> GameOver;

        public static void RaiseMoneyChanged(float money) => MoneyChanged?.Invoke(money);
        public static void RaiseInventoryChanged() => InventoryChanged?.Invoke();
        public static void RaiseSerumRackChanged() => SerumRackChanged?.Invoke();
        public static void RaiseSerumMixed(Serum s) => SerumMixed?.Invoke(s);
        public static void RaiseAnimalStateChanged(AnimalState s) => AnimalStateChanged?.Invoke(s);
        public static void RaiseExplosion() => Explosion?.Invoke();
        public static void RaiseQuotaMet() => QuotaMet?.Invoke();
        public static void RaiseDayEnded() => DayEnded?.Invoke();
        public static void RaiseInspectionStarted(bool boss) => InspectionStarted?.Invoke(boss);
        public static void RaiseToast(string text, ToastKind kind) => Toast?.Invoke(text, kind);
        public static void RaiseDirectorSpeak(string text) => DirectorSpeak?.Invoke(text);
        public static void RaisePauseChanged(bool paused) => PauseChanged?.Invoke(paused);
        public static void RaiseTimeScaleChanged(float scale) => TimeScaleChanged?.Invoke(scale);
        public static void RaisePanelCloseRequested() => PanelCloseRequested?.Invoke();
        /// <summary>Panel öffnen lassen (Id: "mixing", "pult").</summary>
        public static void RaisePanelRequested(string id) => PanelRequested?.Invoke(id);
        /// <summary>Hebel am Pult: 0 = GAS, 1 = KLON, 2 = FÖRDERBAND.</summary>
        public static void RaiseLeverPulled(int lever) => LeverPulled?.Invoke(lever);
        /// <summary>Ware geliefert ('m', 'e', 'f') in Tonnen.</summary>
        public static void RaiseDelivered(char good, float tons) => Delivered?.Invoke(good, tons);
        /// <summary>Serum wurde in die Kammer gesprüht (Farbe für Gas und Düsen).</summary>
        public static void RaiseSerumSprayed(Color c) => SerumSprayed?.Invoke(c);
        /// <summary>Laken liegt jetzt auf (true) oder ist weg (false).</summary>
        public static void RaiseClothChanged(bool covered) => ClothChanged?.Invoke(covered);
        /// <summary>Risiko-Ereignis (Warnung, Bandriss, Kabelriss, Explosion …) für Effekte und Kamerawackeln.</summary>
        public static void RaiseRisk(RiskEvent e) => Risk?.Invoke(e);
        /// <summary>Spieler ist jetzt infiziert (true) oder geheilt (false).</summary>
        public static void RaiseInfectionChanged(bool infected) => InfectionChanged?.Invoke(infected);
        /// <summary>Ware hat das Tagessoll gerade überschritten (ab jetzt +50 %).</summary>
        public static void RaiseOverfulfilled(char good) => Overfulfilled?.Invoke(good);
        /// <summary>Spielende (Endbildschirm).</summary>
        public static void RaiseGameOver(GameOverKind kind) => GameOver?.Invoke(kind);

        /// <summary>Alle Abonnenten entfernen (bei Play-Start ohne Domain-Reload).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            MoneyChanged = null;
            InventoryChanged = null;
            SerumRackChanged = null;
            SerumMixed = null;
            AnimalStateChanged = null;
            Explosion = null;
            QuotaMet = null;
            DayEnded = null;
            InspectionStarted = null;
            Toast = null;
            DirectorSpeak = null;
            PauseChanged = null;
            TimeScaleChanged = null;
            PanelCloseRequested = null;
            PanelRequested = null;
            LeverPulled = null;
            Delivered = null;
            SerumSprayed = null;
            ClothChanged = null;
            Risk = null;
            InfectionChanged = null;
            Overfulfilled = null;
            GameOver = null;
        }
    }
}
