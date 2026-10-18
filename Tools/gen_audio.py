# Erzeugt alle Klänge als WAV nach dem Vorbild des Prototyps (WebAudio-Synthese in SFX/sirenSet/whirSet/updateAudio):
# Oszillatoren mit exponentiellen Frequenz- und Lautstärkerampen und gefiltertes Rauschen. Keine Fremdquellen.
# Lautstärken wie im Prototyp; der Gesamtpegel (Prototyp master 0.45) kommt zur Laufzeit aus der Balancing-JSON.
# Aufruf: python Tools/gen_audio.py <Zielordner>
import math, os, random, struct, sys, wave

SR = 44100
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)
random.seed(7)


def buf(seconds):
    return [0.0] * int(SR * seconds)


def osc(kind, phase):
    p = phase % 1.0
    if kind == "sine":
        return math.sin(2 * math.pi * p)
    if kind == "square":
        return 1.0 if p < 0.5 else -1.0
    if kind == "sawtooth":
        return 2.0 * p - 1.0
    return 4.0 * abs(p - 0.5) - 1.0  # triangle


def tone(b, f, dur, kind="sine", vol=0.2, f2=None, delay=0.0):
    """Wie tone() im Prototyp: Frequenz exponentiell f → f2, Hüllkurve 0,0001 → vol (20 ms) → 0,0001."""
    start = int(delay * SR)
    n = int((dur + 0.05) * SR)
    phase = 0.0
    for i in range(n):
        t = i / SR
        if start + i >= len(b):
            break
        k = min(1.0, t / dur)
        freq = f * (max(20.0, f2) / f) ** k if f2 else f
        phase += freq / SR
        if t < 0.02:
            g = 0.0001 * (vol / 0.0001) ** (t / 0.02)
        elif t < dur:
            g = vol * (0.0001 / vol) ** ((t - 0.02) / max(1e-4, dur - 0.02))
        else:
            g = 0.0
        b[start + i] += osc(kind, phase) * g


def biquad(x, kind, freq, q=0.7071):
    """RBJ-Filter wie BiquadFilterNode (Tiefpass, Hochpass, Bandpass)."""
    w = 2 * math.pi * freq / SR
    cw, sw = math.cos(w), math.sin(w)
    a = sw / (2 * q)
    if kind == "lowpass":
        b0, b1, b2 = (1 - cw) / 2, 1 - cw, (1 - cw) / 2
    elif kind == "highpass":
        b0, b1, b2 = (1 + cw) / 2, -(1 + cw), (1 + cw) / 2
    else:
        b0, b1, b2 = a, 0.0, -a
    a0, a1, a2 = 1 + a, -2 * cw, 1 - a
    y = [0.0] * len(x)
    x1 = x2 = y1 = y2 = 0.0
    for i, v in enumerate(x):
        o = (b0 * v + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2) / a0
        x2, x1, y2, y1 = x1, v, y1, o
        y[i] = o
    return y


def noise(b, dur, vol=0.3, freq=1000, kind="lowpass", delay=0.0):
    """Wie noiseBurst(): weißes Rauschen, linear ausklingend, gefiltert."""
    n = int(SR * dur)
    raw = [(random.random() * 2 - 1) * (1 - i / n) for i in range(n)]
    out = biquad(raw, kind, freq, 1.0 if kind == "bandpass" else 0.7071)
    start = int(delay * SR)
    for i, v in enumerate(out):
        if start + i < len(b):
            b[start + i] += v * vol


def save(b, name):
    peak = max(1e-9, max(abs(v) for v in b))
    scale = 1.0 if peak <= 0.99 else 0.99 / peak
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, v * scale)) * 32767)) for v in b))
    print("geschrieben:", name)


# ---------- Einzelklänge (SFX im Prototyp) ----------
b = buf(0.12); tone(b, 900, 0.05, "square", 0.05); save(b, "click")
# Muhen bei Grundton 150 Hz; tiefer bei viel A über die Tonhöhe der Quelle (150 − 12·A)
b = buf(1.25); tone(b, 150, 1.1, "sawtooth", 0.12, 150 * 0.75); save(b, "moo")
b = buf(1.15); tone(b, 225, 0.9, "triangle", 0.05, 330, 0.15); save(b, "moo_wobble")
for v in range(3):
    b = buf(0.45)
    for i in range(3):
        tone(b, 700 + random.random() * 200, 0.07, "square", 0.06, 450, i * 0.11)
    save(b, "cluck_%d" % v)
b = buf(1.3); noise(b, 1.2, 0.18, 3500, "highpass"); save(b, "hiss")
b = buf(1.7); noise(b, 1.6, 0.9, 400); tone(b, 70, 1.2, "sine", 0.5, 30); save(b, "boom")
b = buf(0.32); tone(b, 1320, 0.08, "square", 0.07); tone(b, 1760, 0.18, "square", 0.07, None, 0.08); save(b, "coin")
b = buf(0.8)
for i, f in enumerate([523, 659, 784, 1046]):
    tone(b, f, 0.25, "square", 0.08, None, i * 0.16)
save(b, "fanfare")
b = buf(1.35)
for i, f in enumerate([392, 370, 349, 262]):
    tone(b, f, 0.35, "triangle", 0.1, None, i * 0.3)
save(b, "sad")
b = buf(0.25); noise(b, 0.15, 0.6, 600); tone(b, 120, 0.15, "square", 0.15, 60); save(b, "stamp")
b = buf(0.95); noise(b, 0.9, 0.3, 1500, "bandpass"); save(b, "whoosh")
for v in range(2):
    b = buf(0.85)
    for i in range(6):
        tone(b, 300 + random.random() * 400, 0.08, "sine", 0.08, 700, i * 0.12)
    save(b, "bubble_%d" % v)
b = buf(0.85); noise(b, 0.8, 0.15, 6000, "highpass"); tone(b, 2400, 0.4, "sine", 0.04, 3000); save(b, "ice")
b = buf(0.5)
for i in range(8):
    tone(b, 2000 + random.random() * 3000, 0.12, "triangle", 0.06, None, i * 0.04)
save(b, "glass")
b = buf(0.62)
for i in range(5):
    noise(b, 0.05, 0.2, 2000, "bandpass", i * 0.12)
save(b, "crank")
b = buf(0.4); tone(b, 880, 0.15, "square", 0.08); tone(b, 660, 0.15, "square", 0.08, None, 0.2); save(b, "warn")
# Direktor-Grummeln: eine Silbe bei 140 Hz, Tonhöhe und Lautstärke zur Laufzeit nach Wut
b = buf(0.15); tone(b, 140, 0.09, "square", 0.03, 140 * 0.8); save(b, "grumble")

# ---------- Musik ----------
# Marsch-Note bei G4 (392 Hz) mit Unteroktave; die Melodie spielt die Laufzeit über die Tonhöhe
b = buf(0.3); tone(b, 392, 0.22, "square", 0.05); tone(b, 196, 0.22, "triangle", 0.05); save(b, "march_note")
b = buf(1.6)
for i, f in enumerate([523, 659, 784, 659, 523]):
    tone(b, f * 0.5, 0.3, "triangle", 0.1, None, i * 0.3)
save(b, "sing")

# ---------- Schleifen ----------
# Sirene: Sägezahn 650 Hz, 2,2-Hz-LFO ±220 Hz; 5 s = 11 LFO-Perioden und 3250 Grundperioden → nahtlos
n = SR * 5
b = [0.0] * n
phase = 0.0
for i in range(n):
    t = i / SR
    phase += (650 + 220 * math.sin(2 * math.pi * 2.2 * t)) / SR
    b[i] = osc("sawtooth", phase) * 0.05
save(b, "siren_loop")
# Zentrifuge: Sägezahn 400 Hz (Tonhöhe zur Laufzeit = 60 + rpm·700), 1 s = 400 Perioden
b = [osc("sawtooth", 400 * i / SR) * 0.025 for i in range(SR)]
save(b, "whir_loop")
