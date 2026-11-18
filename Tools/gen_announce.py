# Erzeugt die Hallen-Durchsagen der Partei als WAV: Gong (Ding-Dong) und gesprochener Text aus Texte_de.json "announce".
# Stimme: Piper (MIT) mit der deutschen Stimme "Thorsten" (Datensatz CC0), danach tiefer und wie ein alter
# Hallen-Lautsprecher (Bandpass, leichte Verzerrung, Echo). Piper liegt nicht im Projekt.
# Aufruf: python Tools/gen_announce.py <piper.exe> <de_DE-thorsten-medium.onnx> <Zielordner>
# Hinweis: Die aktuelle Stimmdatei (.onnx.json) enthält in phoneme_id_map Einträge aus mehreren Zeichen, die Piper
# 2023.11.14-2 ablehnt („is not a single codepoint“). Diese Einträge vorher aus der .onnx.json löschen.
import io, json, os, subprocess, sys, tempfile, wave

import numpy as np

PIPER, MODEL, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
TEXTS = os.path.join(os.path.dirname(__file__), '..', 'Assets', '_Project', 'Data', 'Json', 'Texte_de.json')
SR = 22050
# Aussprache für die Stimme (der Untertitel bleibt wie im Text)
SPOKEN = {'B-12': 'Be zwölf'}


def speak(text):
    for k, v in SPOKEN.items():
        text = text.replace(k, v)
    tmp = os.path.join(tempfile.gettempdir(), 'announce_tmp.wav')
    subprocess.run([PIPER, '--model', MODEL, '--output_file', tmp, '--length_scale', '1.12', '--sentence_silence', '0.35'],
                   input=text.encode('utf-8'), check=True, capture_output=True)
    with wave.open(tmp) as w:
        assert w.getframerate() == SR
        x = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16).astype(np.float64) / 32768.0
    return x


def deepen(x, factor=0.9):
    """Tiefer und etwas langsamer: einfaches Umtasten (Tonhöhe × factor)."""
    n = int(len(x) / factor)
    return np.interp(np.arange(n) * factor, np.arange(len(x)), x)


def bandpass(x, lo=320.0, hi=3400.0):
    spec = np.fft.rfft(x)
    f = np.fft.rfftfreq(len(x), 1.0 / SR)
    mask = 1.0 / (1.0 + (lo / np.maximum(f, 1.0)) ** 4) / (1.0 + (f / hi) ** 4)
    return np.fft.irfft(spec * mask, len(x))


def loudspeaker(x):
    x = bandpass(x)
    x = x / (np.max(np.abs(x)) + 1e-9)
    x = np.tanh(x * 2.2) / np.tanh(2.2)
    # Halle: frühe Reflexionen
    y = np.concatenate([x, np.zeros(int(SR * 0.9))])
    for delay, gain in ((0.11, 0.32), (0.23, 0.2), (0.41, 0.11), (0.62, 0.06)):
        d = int(SR * delay)
        y[d:d + len(x)] += x * gain
    rng = np.random.default_rng(3)
    y += bandpass(rng.normal(0, 0.012, len(y)), 500, 5000)
    return y


def gong():
    """Ding-Dong wie im Bahnhof: zwei Glockentöne mit Obertönen und abklingender Hüllkurve."""
    out = np.zeros(int(SR * 1.6))
    for start, f in ((0.0, 659.25), (0.55, 523.25)):
        t = np.arange(int(SR * 1.05)) / SR
        env = np.exp(-t * 3.2) * np.minimum(1.0, t / 0.005)
        s = (np.sin(2 * np.pi * f * t) + 0.35 * np.sin(2 * np.pi * f * 2.0 * t) + 0.12 * np.sin(2 * np.pi * f * 3.01 * t)) * env
        i = int(SR * start)
        out[i:i + len(s)] += s[:len(out) - i]
    return out / np.max(np.abs(out)) * 0.6


def save(path, x):
    x = x / (np.max(np.abs(x)) + 1e-9) * 0.89
    with wave.open(path, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())


texts = json.load(io.open(TEXTS, encoding='utf-8'))['announce']
os.makedirs(OUT, exist_ok=True)
for key, line in texts.items():
    if key.startswith('_'):
        continue
    voice = loudspeaker(deepen(speak(line)))
    clip = np.concatenate([gong(), np.zeros(int(SR * 0.15)), voice])
    save(os.path.join(OUT, 'announce_' + key + '.wav'), clip)
    print(key, round(len(clip) / SR, 1), 's')
