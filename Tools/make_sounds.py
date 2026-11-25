"""Neon Blade sound generator.
Synthesizes every SFX and both music loops from scratch (no samples),
so all audio is original work. Usage: python make_sounds.py OUT_DIR
"""
import sys, os
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, sosfilt

SR = 44100
rng = np.random.default_rng(12)
OUT = sys.argv[1]
SFX = os.path.join(OUT, "SFX")
MUS = os.path.join(OUT, "Music")
os.makedirs(SFX, exist_ok=True); os.makedirs(MUS, exist_ok=True)


def t_(dur): return np.arange(int(SR * dur)) / SR

def phase(freq):            # freq may be an array (sweep)
    f = np.broadcast_to(freq, freq.shape if np.ndim(freq) else (1,))
    return 2 * np.pi * np.cumsum(f) / SR

def sq(freq, duty=0.5):
    p = (phase(freq) / (2 * np.pi)) % 1.0
    return np.where(p < duty, 1.0, -1.0)

def saw(freq):
    p = (phase(freq) / (2 * np.pi)) % 1.0
    return 2 * p - 1

def tri(freq):
    return 2 * np.abs(saw(freq)) - 1

def sine(freq): return np.sin(phase(freq))

def noise(n): return rng.uniform(-1, 1, n)

def env(n, a=0.005, d=None, curve=4.0):
    """attack then exponential-ish decay over the whole length"""
    x = np.arange(n) / SR
    att = np.clip(x / max(a, 1e-4), 0, 1)
    dec = np.exp(-curve * x / (n / SR))
    out = att * dec
    rel = min(n, int(0.004 * SR))                   # 4 ms fade-out: no click
    out[-rel:] *= np.linspace(1, 0, rel)
    return out

def lp(x, f, order=2):
    return sosfilt(butter(order, f, 'low', fs=SR, output='sos'), x)

def hp(x, f, order=2):
    return sosfilt(butter(order, f, 'high', fs=SR, output='sos'), x)

def bp(x, lo, hi, order=2):
    return sosfilt(butter(order, [lo, hi], 'band', fs=SR, output='sos'), x)

def save(path, x, peak_db=-1.0, stereo=False):
    x = np.asarray(x, dtype=np.float64)
    x = x / (np.max(np.abs(x)) + 1e-9) * 10 ** (peak_db / 20)
    data = (x * 32767).astype(np.int16)
    wavfile.write(path, SR, data.T if stereo else data)

def note(n):                # MIDI note -> Hz
    return 440.0 * 2 ** ((n - 69) / 12)


# ----------------------------------------------------------------- SFX
def jump(v):
    d = 0.16; t = t_(d)
    f0 = [330, 350, 315][v]
    f = f0 * 2 ** (t / d * 1.2)                 # rises ~1.2 octaves
    x = sq(f, 0.25) * 0.6 + sine(f * 2) * 0.3
    return lp(x, 6000) * env(len(t), 0.002, curve=3)

def land():
    d = 0.12; t = t_(d)
    x = sine(110 * 2 ** (-t / d)) + lp(noise(len(t)), 900) * 0.6
    return x * env(len(t), 0.001, curve=8)

def dash(v):                                    # whoosh of the blade dash
    d = 0.24; n = int(SR * d)
    lo = [700, 800][v]
    x = noise(n)
    out = np.zeros(n); seg = n // 8
    for i in range(8):                          # sweep a band upwards
        c = lo * 2 ** (i / 3)
        part = bp(x, c * 0.7, min(c * 1.4, 20000))
        out[i*seg:(i+1)*seg] = part[i*seg:(i+1)*seg]
    w = np.sin(np.linspace(0, np.pi, n)) ** 1.5
    return lp(out, 9000) * w

def hit(v):                                     # heavy, low
    d = 0.28; t = t_(d); n = len(t)
    f0 = [85, 75, 95][v]
    body = sine(f0 * 2 ** (-t * 6)) * env(n, 0.001, curve=6)
    crunch = bp(noise(n), 1200 + 300 * v, 5000) * env(n, 0.0005, curve=22)
    zap = sq(note(52 + 2 * v) * 2 ** (-t * 8), 0.4) * env(n, 0.001, curve=14) * 0.4
    return body * 1.0 + crunch * 0.8 + lp(zap, 4000)

def pickup(v):                                  # bright two-note chime
    d = 0.42; t = t_(d); n = len(t)
    a, b = [(88, 95), (90, 97)][v]               # E6->B6 / F#6->C#7
    split = int(0.07 * SR)
    f = np.where(np.arange(n) < split, note(a), note(b))
    x = sine(f) * 0.8 + tri(f * 2) * 0.25
    e = env(n, 0.002, curve=5)
    return x * e

def windup():                                   # 0.45 s rising charge, very recognisable
    d = 0.45; t = t_(d); n = len(t)
    f = 140 * 2 ** (t / d * 2.0)                 # two octaves up
    trem = 0.6 + 0.4 * np.sign(np.sin(2 * np.pi * (12 + 30 * t / d) * t))
    x = lp(saw(f) + saw(f * 1.01) * 0.7, 3500) * trem
    rise = np.clip(t / d, 0, 1) ** 0.6 * 0.8 + 0.2
    out = x * rise
    out[-200:] *= np.linspace(1, 0, 200)
    out[:60] *= np.linspace(0, 1, 60)
    return out

def alert():                                    # enemy spotted you: two quick beeps
    d = 0.22; n = int(SR * d); out = np.zeros(n)
    for i, (st, f) in enumerate([(0.0, note(84)), (0.1, note(88))]):
        seg = t_(0.08); s = int(st * SR)
        out[s:s+len(seg)] += sq(np.full(len(seg), f), 0.5) * env(len(seg), 0.001, curve=3) * 0.6
    return lp(out, 5000)

def death():
    d = 0.7; t = t_(d); n = len(t)
    f = 420 * 2 ** (-t / d * 3)
    x = saw(f) * 0.6 + sq(f * 0.5, 0.3) * 0.4
    x = lp(x, 2500) * env(n, 0.002, curve=3)
    boom = sine(60 * 2 ** (-t * 3)) * env(n, 0.001, curve=7)
    return x + boom * 0.9 + bp(noise(n), 300, 3000) * env(n, 0.001, curve=15) * 0.5

def jingle(notes, step, tail=0.5):
    d = step * len(notes) + tail; n = int(SR * d); out = np.zeros(n)
    for i, m in enumerate(notes):
        seg = t_(step + tail); s = int(i * step * SR)
        f = np.full(len(seg), note(m))
        v = sq(f, 0.25) * 0.4 + tri(f) * 0.5 + sine(f * 2) * 0.15
        e = env(len(seg), 0.004, curve=5)
        k = min(len(seg), n - s)
        out[s:s+k] += (v * e)[:k]
    return lp(out, 7000)

def win():  return jingle([72, 76, 79, 84, 88], 0.11, 0.7)       # C major up
def lose(): return jingle([69, 65, 62, 57], 0.2, 0.8)            # A minor down

def click(v):
    d = 0.05; t = t_(d); n = len(t)
    f = [1800, 1500][v]
    x = sine(f * 2 ** (-t * 20)) + hp(noise(n), 3000) * 0.3
    return x * env(n, 0.0005, curve=18)


sfx = {
    "jump_1": jump(0), "jump_2": jump(1), "jump_3": jump(2),
    "land_1": land(),
    "dash_1": dash(0), "dash_2": dash(1),
    "hit_1": hit(0), "hit_2": hit(1), "hit_3": hit(2),
    "pickup_1": pickup(0), "pickup_2": pickup(1),
    "windup_1": windup(),
    "alert_1": alert(),
    "death_1": death(),
    "win_1": win(),
    "lose_1": lose(),
    "click_1": click(0), "click_2": click(1),
}
for name, x in sfx.items():
    save(os.path.join(SFX, name + ".wav"), x, -1.0)


# ----------------------------------------------------------------- MUSIC
def render_loop(length_s, events):
    """events: list of (start_s, mono_array, pan[-1..1]). Overflow past the loop end
    is wrapped to the start, so the last bar flows into the first: a seamless loop."""
    n = int(round(length_s * SR))
    L = np.zeros(n + SR * 4); R = np.zeros(n + SR * 4)
    for st, x, pan in events:
        s = int(round(st * SR)); e = s + len(x)
        gl = np.sqrt((1 - pan) / 2); gr = np.sqrt((1 + pan) / 2)
        L[s:e] += x * gl; R[s:e] += x * gr
    outL = L[:n].copy(); outR = R[:n].copy()
    tailL = L[n:]; tailR = R[n:]
    outL[:len(tailL)] += tailL[:n]; outR[:len(tailR)] += tailR[:n]
    return np.vstack([outL, outR])

def kick():
    t = t_(0.35); n = len(t)
    return sine(50 + 120 * np.exp(-t * 35)) * env(n, 0.001, curve=7)

def snare():
    t = t_(0.22); n = len(t)
    return (bp(noise(n), 1500, 7000) * 0.8 + sine(np.full(n, 190.0)) * 0.4) * env(n, 0.001, curve=9)

def hat(open_=False):
    t = t_(0.18 if open_ else 0.05); n = len(t)
    return hp(noise(n), 7000) * env(n, 0.001, curve=6 if open_ else 12)

def pluck(m, dur, bright=3000):
    t = t_(dur); n = len(t); f = np.full(n, note(m))
    x = saw(f) * 0.5 + sq(f * 1.005, 0.5) * 0.3
    return lp(x, bright) * env(n, 0.002, curve=5)

def bass(m, dur):
    t = t_(dur); n = len(t); f = np.full(n, note(m))
    x = saw(f) * 0.7 + sine(f * 0.5) * 0.6
    e = np.clip(t / 0.004, 0, 1) * np.exp(-t * 4)
    e[-150:] *= np.linspace(1, 0, 150)
    return lp(x, 900) * e

def pad(chord, dur):
    t = t_(dur); n = len(t); x = np.zeros(n)
    for m in chord:
        for det in (-0.08, 0.0, 0.08):
            x += saw(np.full(n, note(m + det)))
    x = lp(x, 1400, 2)
    a = int(0.4 * SR); r = int(0.5 * SR)
    e = np.ones(n); e[:a] = np.linspace(0, 1, a); e[-r:] = np.linspace(1, 0, r)
    return x * e / len(chord) / 3

# chords: Am F C G (synthwave classic)
PROG = [(57, [57, 60, 64]), (53, [53, 57, 60]), (48, [55, 60, 64]), (55, [55, 59, 62])]

def menu_track():
    bpm = 88; beat = 60 / bpm; bar = beat * 4; bars = 8
    ev = []
    for b in range(bars):
        root, chord = PROG[b % 4]; t0 = b * bar
        ev.append((t0, pad([c for c in chord] + [chord[0] + 12], bar + 0.5) * 0.9, 0.0))
        ev.append((t0, bass(root - 12, bar * 0.95) * 0.5, 0.0))
        arp = chord + [chord[0] + 12]
        for i in range(8):                        # gentle 8th-note arpeggio
            m = arp[[0, 1, 2, 3, 2, 1, 2, 3][i]] + 12
            ev.append((t0 + i * beat / 2, pluck(m, 0.6, 2200) * 0.22, -0.4 if i % 2 else 0.4))
        if b % 2 == 1:
            ev.append((t0 + beat * 2, snare() * 0.12, 0.0))
        ev.append((t0, kick() * 0.35, 0.0))
    return render_loop(bars * bar, ev)

def level_track():
    bpm = 128; beat = 60 / bpm; bar = beat * 4; bars = 16
    ev = []
    lead = [76, 74, 72, 74, 76, 79, 76, 74]
    for b in range(bars):
        root, chord = PROG[b % 4]; t0 = b * bar
        ev.append((t0, pad(chord, bar + 0.3) * 0.5, 0.0))
        for i in range(8):                        # driving 8th bass
            m = root - 12 + (12 if i % 2 else 0)
            ev.append((t0 + i * beat / 2, bass(m, beat / 2 * 0.9) * 0.55, 0.0))
        for i in range(4):
            ev.append((t0 + i * beat, kick() * 0.9, 0.0))
            if i in (1, 3): ev.append((t0 + i * beat, snare() * 0.6, 0.0))
        for i in range(8):
            ev.append((t0 + i * beat / 2 + beat / 4, hat(i == 7) * 0.25, 0.3))
        arp = chord + [chord[0] + 12, chord[1] + 12]
        for i in range(16):                       # 16th arpeggio
            ev.append((t0 + i * beat / 4, pluck(arp[i % 5] + 12, 0.18, 4000) * 0.16, -0.5 if i % 2 else 0.5))
        if b >= 8:                                # second half: lead melody
            for i, m in enumerate(lead):
                ev.append((t0 + i * beat / 2, pluck(m + (0 if b % 2 == 0 else -2), beat / 2 * 1.6, 3500) * 0.28, 0.0))
    return render_loop(bars * bar, ev)

for name, fn in [("music_menu", menu_track), ("music_level", level_track)]:
    st = fn()
    save(os.path.join(MUS, name + ".wav"), st, -1.5, stereo=True)

print("done:", len(sfx), "sfx + 2 music loops ->", OUT)
