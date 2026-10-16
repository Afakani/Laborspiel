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
        }
    }
}
