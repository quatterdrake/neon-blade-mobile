"""Neon Blade sprite generator: draws every game sprite from code (own work).
100 px = 1 Unity unit at the default import (Pixels Per Unit 100), so sprites
match the existing 1x1 colliders. Usage: python make_sprites.py OUT_DIR
"""
import sys, os, math, random
from PIL import Image, ImageDraw, ImageFilter, ImageChops

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)
SS = 4  # supersampling for smooth edges

CYAN = (5, 217, 232); PINK = (255, 42, 109); PURPLE = (130, 40, 220)
DARK = (13, 2, 33); WHITE = (255, 255, 255); YELLOW = (255, 214, 10)


def canvas(w, h):
    return Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0))

def S(v):  # scale coordinates
    return [x * SS for x in v] if isinstance(v, (list, tuple)) else v * SS

def glow(img, radius, strength=1.0):
    """add a soft coloured halo behind the drawing"""
    blur = img.filter(ImageFilter.GaussianBlur(radius * SS))
    if strength != 1.0:
        a = blur.getchannel("A").point(lambda p: min(255, int(p * strength)))
        blur.putalpha(a)
    return Image.alpha_composite(blur, img)

def finish(img, w, h, name):
    img = img.resize((w, h), Image.LANCZOS)
    img.save(os.path.join(OUT, name))
    print("saved", name, img.size)


# ------------------------------------------------------------------ player
def player():
    W = H = 100
    im = canvas(W, H); d = ImageDraw.Draw(im)
    # scarf trailing behind (left)
    d.polygon(S([30, 38, 8, 30, 12, 40, 4, 48, 30, 46]), fill=PINK + (255,))
    # body: rounded suit
    d.rounded_rectangle(S([28, 30, 72, 88]), radius=S(14), fill=(20, 40, 70, 255), outline=CYAN + (255,), width=S(4))
    # head / helmet
    d.rounded_rectangle(S([30, 8, 70, 42]), radius=S(14), fill=(20, 40, 70, 255), outline=CYAN + (255,), width=S(4))
    # visor
    d.rounded_rectangle(S([42, 18, 68, 30]), radius=S(5), fill=CYAN + (255,))
    d.rectangle(S([58, 20, 64, 23]), fill=WHITE + (255,))
    # belt + chest light
    d.rectangle(S([30, 58, 70, 63]), fill=PINK + (255,))
    d.ellipse(S([46, 42, 54, 50]), fill=CYAN + (255,))
    # legs
    d.rounded_rectangle(S([33, 84, 47, 98]), radius=S(4), fill=CYAN + (255,))
    d.rounded_rectangle(S([53, 84, 67, 98]), radius=S(4), fill=CYAN + (255,))
    # blade on the back (diagonal neon line)
    d.line(S([40, 56, 96, 6]), fill=WHITE + (255,), width=S(5))
    d.line(S([40, 56, 96, 6]), fill=PINK + (255,), width=S(2))
    d.line(S([36, 62, 46, 52]), fill=YELLOW + (255,), width=S(6))   # hilt
    finish(glow(im, 2.5, 1.2), W, H, "player.png")


# ------------------------------------------------------------------ enemy (white: EnemyAI tints it)
def enemy():
    W = H = 100
    im = canvas(W, H); d = ImageDraw.Draw(im)
    g1 = (235, 235, 235, 255); g2 = (150, 150, 150, 255); g3 = (70, 70, 70, 255)
    # spikes / horns
    d.polygon(S([22, 26, 14, 4, 36, 20]), fill=g1)
    d.polygon(S([78, 26, 86, 4, 64, 20]), fill=g1)
    # body
    d.rounded_rectangle(S([10, 18, 90, 84]), radius=S(18), fill=g3, outline=g1, width=S(5))
    # angry eye bar
    d.rounded_rectangle(S([20, 34, 80, 56]), radius=S(8), fill=(25, 25, 25, 255))
    d.polygon(S([24, 38, 46, 44, 46, 52, 24, 52]), fill=WHITE + (255,))
    d.polygon(S([76, 38, 54, 44, 54, 52, 76, 52]), fill=WHITE + (255,))
    # teeth vent
    for x in range(26, 76, 10):
        d.rectangle(S([x, 64, x + 5, 74]), fill=g2)
    # feet
    d.rounded_rectangle(S([18, 82, 40, 98]), radius=S(5), fill=g1)
    d.rounded_rectangle(S([60, 82, 82, 98]), radius=S(5), fill=g1)
    finish(glow(im, 2.5, 1.0), W, H, "enemy.png")


# ------------------------------------------------------------------ crystal
def crystal():
    W = H = 100
    im = canvas(W, H); d = ImageDraw.Draw(im)
    top, mid, bot = (50, 6), (50, 38), (50, 94)
    left, right = (18, 38), (82, 38)
    d.polygon(S([*top, *right, *bot, *left]), fill=CYAN + (255,))
    d.polygon(S([*top, *right, *mid]), fill=(170, 250, 255, 255))
    d.polygon(S([*top, *left, *mid]), fill=(90, 235, 250, 255))
    d.polygon(S([*left, *mid, *bot]), fill=(0, 150, 190, 255))
    d.polygon(S([*right, *mid, *bot]), fill=(0, 190, 220, 255))
    d.line(S([*top, *right, *bot, *left, *top]), fill=WHITE + (255,), width=S(3))
    d.polygon(S([40, 18, 46, 14, 36, 34, 30, 34]), fill=WHITE + (230,))   # shine
    finish(glow(im, 4, 1.6), W, H, "crystal.png")


# ------------------------------------------------------------------ ground (stretched 30x wide: horizontal bands only)
def ground():
    W = H = 100
    im = Image.new("RGBA", (W, H), DARK + (255,)); d = ImageDraw.Draw(im)
    for y in range(H):                                   # purple fade downwards
        k = y / H
        c = tuple(int(a * (1 - k) + b * k) for a, b in zip((60, 10, 90), DARK))
        d.line([(0, y), (W, y)], fill=c + (255,))
    for y in (30, 52, 76):                               # grid lines
        d.line([(0, y), (W, y)], fill=PURPLE + (160,), width=2)
    d.rectangle([0, 0, W, 6], fill=PINK + (255,))       # neon top edge
    d.rectangle([0, 6, W, 12], fill=(255, 120, 170, 140))
    d.rectangle([0, 0, W, 2], fill=(255, 210, 225, 255))
    im.save(os.path.join(OUT, "ground.png")); print("saved ground.png")


# ------------------------------------------------------------------ background (synthwave sunset)
def background():
    W, H = 1920, 1080
    im = Image.new("RGBA", (W, H)); d = ImageDraw.Draw(im)
    horizon = int(H * 0.62)
    sky = [(8, 2, 28), (40, 8, 75), (120, 20, 110), (255, 70, 120)]
    for y in range(horizon):
        k = y / horizon * (len(sky) - 1); i = min(int(k), len(sky) - 2); f = k - i
        c = tuple(int(sky[i][j] * (1 - f) + sky[i + 1][j] * f) for j in range(3))
        d.line([(0, y), (W, y)], fill=c + (255,))
    rnd = random.Random(7)
    for _ in range(220):                                  # stars
        x, y = rnd.randrange(W), rnd.randrange(int(horizon * 0.7))
        r = rnd.choice([1, 1, 1, 2]); a = rnd.randrange(120, 255)
        d.ellipse([x - r, y - r, x + r, y + r], fill=(255, 255, 255, a))
    # sun with stripes
    sun = Image.new("RGBA", (W, H), (0, 0, 0, 0)); sd = ImageDraw.Draw(sun)
    cx, cy, R = W // 2, horizon - 40, 260
    for y in range(cy - R, cy + R):
        k = (y - (cy - R)) / (2 * R)
        c = (255, int(220 - 150 * k), int(60 + 60 * k))
        half = int(math.sqrt(max(0, R * R - (y - cy) ** 2)))
        sd.line([(cx - half, y), (cx + half, y)], fill=c + (255,))
    for i, y in enumerate(range(cy - 20, cy + R, 34)):   # cut stripes in the lower half
        sd.rectangle([cx - R, y, cx + R, y + 6 + i * 2], fill=(0, 0, 0, 0))
    halo = sun.filter(ImageFilter.GaussianBlur(40))
    im = Image.alpha_composite(im, halo); im = Image.alpha_composite(im, sun)
    d = ImageDraw.Draw(im)
    # mountains
    for layer, (col, base, amp, seed) in enumerate([((60, 10, 90), horizon, 170, 3), ((25, 5, 50), horizon, 110, 9)]):
        r2 = random.Random(seed); pts = [(0, base)]; x = 0
        while x < W:
            x += r2.randrange(80, 200); pts.append((x, base - r2.randrange(20, amp)))
        pts += [(W, base), (W, H), (0, H)]
        d.polygon(pts, fill=col + (255,))
        d.line(pts[:-2], fill=(PINK if layer == 0 else PURPLE) + (200,), width=3)
    # floor + perspective grid
    d.rectangle([0, horizon, W, H], fill=(14, 2, 30, 255))
    grid = Image.new("RGBA", (W, H), (0, 0, 0, 0)); gd = ImageDraw.Draw(grid)
    for i in range(-20, 21):
        gd.line([(W // 2 + i * 40, horizon), (W // 2 + i * 260, H)], fill=PINK + (200,), width=3)
    y = horizon; step = 8
    while y < H:
        gd.line([(0, y), (W, y)], fill=PINK + (200,), width=3); y += step; step *= 1.35
    im = Image.alpha_composite(im, grid.filter(ImageFilter.GaussianBlur(4)))
    im = Image.alpha_composite(im, grid)
    d = ImageDraw.Draw(im)
    d.line([(0, horizon), (W, horizon)], fill=(255, 180, 220, 255), width=4)
    # slight darkening so gameplay sprites stand out
    im = Image.alpha_composite(im, Image.new("RGBA", (W, H), (0, 0, 0, 70)))
    im.convert("RGB").save(os.path.join(OUT, "background.png")); print("saved background.png")


player(); enemy(); crystal(); ground(); background()


# ------------------------------------------------------------------ platform (700x50 px = 7 x 0.5 units, used at scale 1)
def platform():
    W, H = 700, 50
    im = canvas(W, H); d = ImageDraw.Draw(im)
    CY = CYAN + (255,)
    # metal body with a darker lower half
    d.rounded_rectangle(S([2, 4, W - 2, H - 2]), radius=S(8), fill=(28, 22, 58, 255))
    d.rectangle(S([6, 26, W - 6, H - 6]), fill=(18, 12, 40, 255))
    # panel seams + hazard chevrons every 70 px
    for x in range(70, W, 70):
        d.line(S([x, 8, x, H - 6]), fill=(60, 50, 110, 255), width=S(2))
    for x in range(20, W - 20, 35):
        d.polygon(S([x, 30, x + 12, 30, x + 20, 38, x + 12, 46, x, 46, x + 8, 38]), fill=(5, 120, 140, 255))
    # rivets
    for x in range(14, W, 70):
        for xx in (x, x + 42):
            if xx < W - 8:
                d.ellipse(S([xx - 3, 13, xx + 3, 19]), fill=(120, 110, 180, 255))
    # neon cyan top edge + outline
    d.rounded_rectangle(S([2, 2, W - 2, 9]), radius=S(3), fill=CY)
    d.rounded_rectangle(S([2, 4, W - 2, H - 2]), radius=S(8), outline=CY, width=S(2))
    d.line(S([12, 4, W - 12, 4]), fill=(220, 255, 255, 255), width=S(1))
    finish(glow(im, 2, 0.9), W, H, "platform.png")

platform()
