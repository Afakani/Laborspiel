using System;

namespace PlanErfuellt
{
    /// <summary>Ton (GDD 14, Prototyp SFX/updateAudio): Pegel, Tierlaute, Musik, Schleifen, Zuordnung zu Meldungen.</summary>
    [Serializable]
    public class AudioConfig
    {
        /// <summary>Gesamtpegel (Prototyp master 0.45)</summary>
        public float master;
        /// <summary>Standard Musik 0..1</summary>
        public float music;
        /// <summary>Standard Effekte 0..1</summary>
        public float sfx;
        /// <summary>Gleichzeitige Effektquellen</summary>
        public int voices;
        /// <summary>Tierlaut frühestens nach s</summary>
        public float mooEveryMin;
        /// <summary>plus Zufall bis s</summary>
        public float mooEveryRand;
        /// <summary>Tierlaut nach dem Klonen in s</summary>
        public float cloneVoiceDelay;
        /// <summary>Grundton der Datei moo</summary>
        public float mooBaseHz;
        /// <summary>tiefer je A</summary>
        public float mooHzPerA;
        /// <summary>wobbelig ab B</summary>
        public int mooWobbleFromB;
        /// <summary>Marsch: Notenabstand</summary>
        public float marchBeatSec;
        /// <summary>Grundton der Datei march_note</summary>
        public float marchBaseHz;
        /// <summary>Melodie in Hz</summary>
        public float[] marchNotes = new float[0];
        /// <summary>Direktor: Grundton bei Wut 0</summary>
        public float grumbleBaseHz;
        public float grumbleHzPerAnger;
        public float grumbleRandHz;
        public float grumbleGapSec;
        public int grumbleMax;
        public int grumbleCharsPerSyllable;
        /// <summary>Lautstärke +Anteil je Wutpunkt</summary>
        public float grumbleVolPerAnger;
        /// <summary>Grundton der Datei whir_loop</summary>
        public float whirClipHz;
        public float whirBaseHz;
        public float whirHzPerRpm;
        /// <summary>Münzhuhn: Münzklang je Ei</summary>
        public float coinEggChance;
        /// <summary>Meldung → Klang</summary>
        public ToastSound[] toastSounds = new ToastSound[0];
    }
}
