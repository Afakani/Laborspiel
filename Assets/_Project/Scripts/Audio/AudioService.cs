using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Ton des Spiels (GDD 14, Prototyp SFX/updateAudio): Effektquellen im Ring, Kanäle Musik/Effekte mit Gesamtpegel,
    /// Ton aus mit M. Klänge zu Meldungen kommen aus audio.toastSounds, dazu Mischen, Sprühen, Hebel, Direktor-Grummeln,
    /// Tierlaute, Sirene bei INSTABIL, Zentrifugensurren (Tonhöhe nach Drehzahl) und Marschmusik vom Grammophon.
    /// Klänge selbst erzeugt (Tools/gen_audio.py), Dateiname = Klang-Id, Varianten als name_0, name_1 …
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [SerializeField] AudioClip[] clips = new AudioClip[0];

        readonly Dictionary<string, List<AudioClip>> bank = new Dictionary<string, List<AudioClip>>();
        AudioSource[] voices;
        int nextVoice;
        AudioSource siren, whir;
        Chamber chamber;
        float mooTimer, beatTimer;
        int beat;

        GameManager GM => GameManager.Instance;
        AudioConfig C => GM.Config.audio;

        public void Setup(AudioClip[] all) => clips = all;

        void Awake()
        {
            Instance = this;
            foreach (var c in clips)
            {
                if (c == null) continue;
                string id = c.name;
                int us = id.LastIndexOf('_');
                if (us > 0 && int.TryParse(id.Substring(us + 1), out _)) id = id.Substring(0, us);
                if (!bank.TryGetValue(id, out var list)) bank[id] = list = new List<AudioClip>();
                list.Add(c);
            }
        }

        void Start()
        {
            voices = new AudioSource[Mathf.Max(1, C.voices)];
            for (int i = 0; i < voices.Length; i++) voices[i] = NewSource("Voice" + i, null, false);
            siren = NewSource("Siren", Clip("siren_loop"), true);
            whir = NewSource("Whir", Clip("whir_loop"), true);
            chamber = FindAnyObjectByType<Chamber>();
            mooTimer = C.mooEveryMin;
        }

        void OnEnable()
        {
            EventBus.ToastKey += OnToastKey;
            EventBus.SerumMixed += OnMixed;
            EventBus.SerumSprayed += OnSprayed;
            EventBus.LeverPulled += OnLever;
            EventBus.UltimateSprayed += OnUltimate;
            EventBus.DirectorSpeak += Grumble;
            EventBus.Station += OnStation;
            EventBus.PauseChanged += OnPause;
        }

        void OnDisable()
        {
            EventBus.ToastKey -= OnToastKey;
            EventBus.SerumMixed -= OnMixed;
            EventBus.SerumSprayed -= OnSprayed;
            EventBus.LeverPulled -= OnLever;
            EventBus.UltimateSprayed -= OnUltimate;
            EventBus.DirectorSpeak -= Grumble;
            EventBus.Station -= OnStation;
            EventBus.PauseChanged -= OnPause;
            AudioListener.pause = false;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        AudioSource NewSource(string name, AudioClip clip, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.loop = loop;
            s.clip = clip;
            return s;
        }

        AudioClip Clip(string id) =>
            bank.TryGetValue(id, out var list) && list.Count > 0 ? list[Random.Range(0, list.Count)] : null;

        float Level(bool music) =>
            AudioLogic.Channel(music, C, GameSettings.Master, GameSettings.Music, GameSettings.Sfx, GameSettings.Muted);

        /// <summary>Klang abspielen (Id = Dateiname). pitch = Tonhöhenfaktor, volume relativ, delay in Sekunden.</summary>
        public void Play(string id, float pitch = 1f, float volume = 1f, float delay = 0f, bool music = false)
        {
            if (string.IsNullOrEmpty(id) || voices == null) return;
            if (id == AudioLogic.Voice)
            {
                AnimalVoice(delay);
                return;
            }
            var clip = Clip(id);
            float level = Level(music) * volume;
            if (clip == null || level <= 0f) return;
            var s = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            s.Stop();
            s.clip = clip;
            s.pitch = pitch;
            s.volume = level;
            if (delay > 0f) s.PlayDelayed(delay);
            else s.Play();
        }

        /// <summary>Laut des Tiers in der Kammer: Muhen (tiefer mit A, wobbelig bei B ≥ 2 / Ultimativ) bzw. Gackern.</summary>
        void AnimalVoice(float delay)
        {
            if (chamber == null || !chamber.HasLivingAnimal && chamber.State != AnimalState.Enter) return;
            var s = chamber.Stats;
            if (s.kind == AnimalKind.Hen)
            {
                Play("cluck", 1f, 1f, delay);
                return;
            }
            float p = AudioLogic.MooPitch(s.dose.A, C);
            Play("moo", p, 1f, delay);
            if (AudioLogic.MooWobble(s.dose.B, s.IsUltimate, C)) Play("moo_wobble", p, 1f, delay);
        }

        void Grumble(string text)
        {
            float anger = GM.State.anger;
            int n = AudioLogic.GrumbleCount(text, C);
            for (int i = 0; i < n; i++)
                Play("grumble", AudioLogic.GrumblePitch(anger, Random.value, C), AudioLogic.GrumbleVolume(anger, C), i * C.grumbleGapSec);
        }

        void OnToastKey(string key)
        {
            var id = AudioLogic.SoundForToast(key, C);
            if (id != null) Play(id, 1f, 1f, id == AudioLogic.Voice ? C.cloneVoiceDelay : 0f);
        }

        void OnMixed(Serum s) => Play("bubble");
        void OnSprayed(Color c) => Play("hiss");
        void OnLever(int lever) => Play("click");
        void OnPause(bool paused) => AudioListener.pause = paused;

        void OnUltimate(UltimateSerum u, bool first)
        {
            if (first) Play("fanfare");
        }

        void OnStation(string id)
        {
            // Münzhuhn: Münzklang bei 30 % der Eier (Prototyp)
            if (id == "coinEgg" && Random.value < C.coinEggChance) Play("coin");
        }

        void Update()
        {
            var gm = GM;
            if (gm == null || voices == null) return;
            if (InputReader.MutePressed) GameSettings.Muted = !GameSettings.Muted;

            var dc = DayCycle.Instance;
            bool play = dc != null && dc.Mode == DayMode.Play && !gm.Clock.Paused;
            float dt = gm.Clock.DeltaTime;
            var s = gm.State;

            // Sirene bei INSTABIL, Surren der Zentrifuge
            Loop(siren, play && chamber != null && chamber.State == AnimalState.Unstable, 1f);
            bool spin = play && s.centrifuge != null && s.centrifuge.spinning;
            Loop(whir, spin, spin ? AudioLogic.WhirPitch(s.centrifuge.rpm, C) : 1f);

            if (!play) return;

            // Marschmusik, solange das Grammophon läuft
            if (s.musicLeft > 0f)
            {
                beatTimer -= dt;
                if (beatTimer <= 0f)
                {
                    beatTimer = C.marchBeatSec;
                    Play("march_note", AudioLogic.MarchPitch(beat++, C), 1f, 0f, true);
                }
            }

            // Tierlaute alle 9–19 s; die Singende Kuh singt
            mooTimer -= dt;
            if (mooTimer <= 0f && chamber != null && chamber.HasLivingAnimal)
            {
                mooTimer = C.mooEveryMin + Random.value * C.mooEveryRand;
                var st = chamber.Stats;
                if (st.fun != null && st.fun.id == UltimateIds.SingingCow) Play("sing", 1f, 1f, 0f, true);
                else AnimalVoice(0f);
            }
        }

        void Loop(AudioSource src, bool on, float pitch)
        {
            if (src == null || src.clip == null) return;
            float level = Level(false);
            if (on && level > 0f)
            {
                src.pitch = pitch;
                src.volume = level;
                if (!src.isPlaying) src.Play();
            }
            else if (src.isPlaying) src.Stop();
        }
    }
}
