using System;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Einstellungen des Spielers (GDD 14, Phase 12): Lautstärke Gesamt/Musik/Effekte, Ton aus (M), Mausempfindlichkeit, Kamerawackeln,
    /// Grafik Hoch/Niedrig (umgesetzt von GraphicsQuality). Gespeichert in den PlayerPrefs, Vorgaben und Grenzen aus der Balancing-JSON.
    /// </summary>
    public static class GameSettings
    {
        const string KeyMaster = "pe.master", KeyMusic = "pe.music", KeySfx = "pe.sfx", KeyMuted = "pe.muted",
            KeyMouse = "pe.mouse", KeyHigh = "pe.highQuality", KeyShake = "pe.shake";

        static GameConfig cfg;

        /// <summary>Eine Einstellung hat sich geändert.</summary>
        public static event Action Changed;

        public static void Init(GameConfig config) => cfg = config;

        public static float Master { get => PlayerPrefs.GetFloat(KeyMaster, 1f); set => SetFloat(KeyMaster, Mathf.Clamp01(value)); }
        public static float Music { get => PlayerPrefs.GetFloat(KeyMusic, cfg != null ? cfg.audio.music : 1f); set => SetFloat(KeyMusic, Mathf.Clamp01(value)); }
        public static float Sfx { get => PlayerPrefs.GetFloat(KeySfx, cfg != null ? cfg.audio.sfx : 1f); set => SetFloat(KeySfx, Mathf.Clamp01(value)); }

        public static bool Muted
        {
            get => PlayerPrefs.GetInt(KeyMuted, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KeyMuted, value ? 1 : 0);
                Changed?.Invoke();
            }
        }

        /// <summary>Faktor auf die Mausempfindlichkeit aus der Spieler-Config.</summary>
        public static float Mouse
        {
            get => PlayerPrefs.GetFloat(KeyMouse, cfg != null ? cfg.settings.mouseDefault : 1f);
            set => SetFloat(KeyMouse, cfg != null ? Mathf.Clamp(value, cfg.settings.mouseMin, cfg.settings.mouseMax) : value);
        }

        /// <summary>Faktor auf das Kamerawackeln 0..1 (0 = aus).</summary>
        public static float Shake { get => PlayerPrefs.GetFloat(KeyShake, 1f); set => SetFloat(KeyShake, Mathf.Clamp01(value)); }

        public static bool HighQuality
        {
            get => PlayerPrefs.GetInt(KeyHigh, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KeyHigh, value ? 1 : 0);
                Changed?.Invoke();
            }
        }

        static void SetFloat(string key, float v)
        {
            PlayerPrefs.SetFloat(key, v);
            Changed?.Invoke();
        }

        /// <summary>Statisches Ereignis leeren (Play-Start ohne Domain-Reload).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Changed = null;
    }
}
