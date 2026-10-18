using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Rechenregeln des Tons (Prototyp SFX/updateAudio), rein und testbar: Tonhöhen, Silbenzahl des Direktors,
    /// Pegel der Kanäle und Klang zu einer Meldung.
    /// </summary>
    public static class AudioLogic
    {
        /// <summary>Besondere Klang-Id: Laut des Tiers in der Kammer (Muhen bzw. Gackern).</summary>
        public const string Voice = "voice";

        /// <summary>Muhen: Grundton 150 − 12·A (Prototyp moo) als Tonhöhenfaktor der Datei.</summary>
        public static float MooPitch(int a, AudioConfig c) => Mathf.Max(0.2f, (c.mooBaseHz - a * c.mooHzPerA) / c.mooBaseHz);

        /// <summary>Wobbeliges Muhen ab B ≥ 2 oder bei Ultimativ.</summary>
        public static bool MooWobble(int b, bool ultimate, AudioConfig c) => ultimate || b >= c.mooWobbleFromB;

        /// <summary>Silben des Grummelns: min(12, round(Länge / 6)).</summary>
        public static int GrumbleCount(string text, AudioConfig c) =>
            string.IsNullOrEmpty(text) ? 0 : Mathf.Min(c.grumbleMax, Mathf.RoundToInt(text.Length / (float)c.grumbleCharsPerSyllable));

        /// <summary>Tonhöhe einer Silbe: (140 − Wut·0,5 + Zufall·80) / 140; tiefer bei Wut.</summary>
        public static float GrumblePitch(float anger, float rnd01, AudioConfig c) =>
            (c.grumbleBaseHz - anger * c.grumbleHzPerAnger + rnd01 * c.grumbleRandHz) / c.grumbleBaseHz;

        /// <summary>Lautstärke einer Silbe relativ zur Datei: lauter bei Wut.</summary>
        public static float GrumbleVolume(float anger, AudioConfig c) => 1f + anger * c.grumbleVolPerAnger;

        /// <summary>Zentrifuge: 60 + rpm·700 Hz als Faktor der Datei whir_loop.</summary>
        public static float WhirPitch(float rpm, AudioConfig c) => (c.whirBaseHz + rpm * c.whirHzPerRpm) / c.whirClipHz;

        /// <summary>Marsch: Tonhöhenfaktor der n-ten Note.</summary>
        public static float MarchPitch(int beat, AudioConfig c) =>
            c.marchNotes.Length == 0 ? 1f : c.marchNotes[beat % c.marchNotes.Length] / c.marchBaseHz;

        /// <summary>Pegel eines Kanals: Gesamtpegel (JSON) × Einstellung Gesamt × Kanal; 0 bei Ton aus.</summary>
        public static float Channel(bool music, AudioConfig c, float master, float musicVol, float sfxVol, bool muted) =>
            muted ? 0f : c.master * master * (music ? musicVol : sfxVol);

        /// <summary>Klang-Id zu einer Meldung oder null.</summary>
        public static string SoundForToast(string key, AudioConfig c)
        {
            foreach (var t in c.toastSounds)
                if (t.key == key) return t.sound;
            return null;
        }
    }
}
