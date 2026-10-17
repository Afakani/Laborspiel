# Erzeugt eigene Texturen für den Look-Pass (keine Fremdquellen):
# Warnstreifen, Emblem (Ähre + Zahnrad), Direktor-Porträt (erfundene Comicfigur), Plantafel-Diagramme,
# Papier, Tank-Symbole, weiche Verläufe für Nebel und Lichtkegel.
# Aufruf: python Tools/gen_lookdev_textures.py <Zielordner>
import math, os, random, sys
from PIL import Image, ImageDraw, ImageFilter

random.seed(12)
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

CREAM = (242, 230, 201)
GOLD = (212, 160, 23)
RED = (179, 18, 27)
DARK_RED = (110, 14, 18)


def noise_layer(size, scale, alpha):
    small = Image.effect_noise((max(1, size[0] // scale), max(1, size[1] // scale)), 64).resize(size, Image.BICUBIC)
    return small.point(lambda v: int(max(0, v - 128) * alpha))


def save(img, name):
    img.save(os.path.join(OUT, name))
    print("geschrieben:", name)


# ---------- Warnstreifen ----------
def hazard():
    s = 512
    img = Image.new("RGB", (s, s), (30, 28, 24))
    d = ImageDraw.Draw(img)
    w = 64
    for i in range(-s, s * 2, w * 2):
        d.polygon([(i, 0), (i + w, 0), (i + w - s, s), (i - s, s)], fill=(232, 178, 20))
    grime = noise_layer((s, s), 8, 0.9)
    dark = Image.new("RGB", (s, s), (40, 30, 20))
    img = Image.composite(dark, img, grime)
    scratches = Image.new("L", (s, s), 0)
    sd = ImageDraw.Draw(scratches)
    for _ in range(90):
        x, y = random.randint(0, s), random.randint(0, s)
        sd.line([(x, y), (x + random.randint(-60, 60), y + random.randint(-8, 8))], fill=random.randint(60, 140), width=1)
    img = Image.composite(Image.new("RGB", (s, s), (120, 110, 95)), img, scratches)
    save(img, "Hazard.png")


# ---------- Emblem: Zahnrad + Ähre ----------
def emblem(color, name):
    s = 1024
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = s / 2
    r_out, r_in = 430, 330
    for i in range(16):
        a = i / 16 * math.tau
        pts = []
        for da, rr in ((-0.09, r_out - 40), (-0.06, r_out + 50), (0.06, r_out + 50), (0.09, r_out - 40)):
            pts.append((c + math.cos(a + da) * rr, c + math.sin(a + da) * rr))
        d.polygon(pts, fill=color)
    d.ellipse([c - r_out, c - r_out, c + r_out, c + r_out], fill=color)
    d.ellipse([c - r_in, c - r_in, c + r_in, c + r_in], fill=(0, 0, 0, 0))
    # Ähre: Halm und Körner
    d.line([(c, c + 260), (c, c - 230)], fill=color, width=26)
    for k in range(6):
        y = c + 150 - k * 70
        size = 70 - k * 6
        for side in (-1, 1):
            cx = c + side * size * 0.8
            box = [cx - size, y - size * 0.42, cx + size, y + size * 0.42]
            grain = Image.new("RGBA", (s, s), (0, 0, 0, 0))
            gd = ImageDraw.Draw(grain)
            gd.ellipse(box, fill=color)
            grain = grain.rotate(side * -32, center=(cx, y))
            img.alpha_composite(grain)
    d = ImageDraw.Draw(img)
    d.ellipse([c - 30, c - 300, c + 30, c - 200], fill=color)
    save(img, name)


# ---------- Direktor (erfundene Comicfigur, keine echte Person) ----------
def director():
    w, h = 1024, 640
    img = Image.new("RGB", (w, h), DARK_RED)
    d = ImageDraw.Draw(img)
    # Hintergrund: rote Vorhangfalten und Lichtkegel
    for x in range(0, w, 48):
        d.rectangle([x, 0, x + 22, h], fill=(150, 24, 30))
    glow = Image.new("L", (w, h), 0)
    ImageDraw.Draw(glow).ellipse([w * 0.18, -h * 0.2, w * 0.82, h * 1.1], fill=110)
    glow = glow.filter(ImageFilter.GaussianBlur(80))
    img = Image.composite(Image.new("RGB", (w, h), (215, 70, 60)), img, glow)
    d = ImageDraw.Draw(img)
    cx = w * 0.5
    # Uniform (Schultern) mit Schulterklappen und Orden
    d.polygon([(cx - 330, h), (cx - 300, 470), (cx - 120, 410), (cx + 120, 410), (cx + 300, 470), (cx + 330, h)], fill=(58, 66, 52))
    d.polygon([(cx - 70, 410), (cx, 520), (cx + 70, 410)], fill=(235, 230, 215))
    d.polygon([(cx - 22, 430), (cx, 560), (cx + 22, 430)], fill=(140, 20, 25))
    for side in (-1, 1):
        d.rectangle([cx + side * 300 - 70, 445, cx + side * 300 + 70, 478], fill=(176, 30, 30))
        d.line([(cx + side * 300 - 60, 461), (cx + side * 300 + 60, 461)], fill=GOLD, width=5)
    for i in range(3):
        x = cx + 120 + i * 48
        d.rectangle([x, 520, x + 30, 540], fill=[(200, 30, 30), GOLD, (40, 90, 160)][i])
        d.ellipse([x + 2, 545, x + 28, 573], fill=GOLD)
    # Hals und Kopf
    d.rectangle([cx - 70, 330, cx + 70, 430], fill=(222, 170, 140))
    d.ellipse([cx - 150, 120, cx + 150, 400], fill=(232, 182, 150))
    d.rectangle([cx - 135, 300, cx + 135, 360], fill=(232, 182, 150))
    d.polygon([(cx - 135, 340), (cx, 420), (cx + 135, 340)], fill=(232, 182, 150))
    # Ohren
    for side in (-1, 1):
        d.ellipse([cx + side * 150 - 25, 230, cx + side * 150 + 25, 300], fill=(222, 165, 135))
    # Augenbrauen (grimmig), Augen, Nase, Schnurrbart, Mund
    for side in (-1, 1):
        d.polygon([(cx + side * 20, 205), (cx + side * 110, 185), (cx + side * 110, 205), (cx + side * 20, 222)], fill=(60, 40, 30))
        d.ellipse([cx + side * 60 - 18, 228, cx + side * 60 + 18, 248], fill=(250, 245, 235))
        d.ellipse([cx + side * 60 - 8, 232, cx + side * 60 + 8, 246], fill=(30, 25, 20))
    d.polygon([(cx - 6, 230), (cx + 30, 300), (cx - 20, 305)], fill=(205, 140, 115))
    d.polygon([(cx - 95, 330), (cx - 10, 305), (cx, 315), (cx + 10, 305), (cx + 95, 330), (cx + 60, 342), (cx, 328), (cx - 60, 342)], fill=(70, 50, 38))
    d.line([(cx - 40, 362), (cx + 40, 362)], fill=(140, 70, 60), width=6)
    # Schirmmütze mit rotem Band und Stern
    d.polygon([(cx - 190, 150), (cx - 150, 60), (cx, 30), (cx + 150, 60), (cx + 190, 150)], fill=(58, 66, 52))
    d.rectangle([cx - 160, 140, cx + 160, 175], fill=(176, 30, 30))
    d.polygon([(cx - 175, 172), (cx + 175, 172), (cx + 120, 205), (cx - 120, 205)], fill=(30, 30, 28))
    star = []
    for i in range(10):
        a = -math.pi / 2 + i * math.pi / 5
        rr = 34 if i % 2 == 0 else 14
        star.append((cx + math.cos(a) * rr, 100 + math.sin(a) * rr))
    d.polygon(star, fill=GOLD)
    # Comic-Kontur leicht weichzeichnen, Bildschirm-Zeilen
    img = img.filter(ImageFilter.SMOOTH_MORE)
    lines = Image.new("L", (w, h), 0)
    ld = ImageDraw.Draw(lines)
    for y in range(0, h, 4):
        ld.line([(0, y), (w, y)], fill=40)
    img = Image.composite(Image.new("RGB", (w, h), (0, 0, 0)), img, lines)
    save(img, "Director.png")


# ---------- Plantafel: drei Felder mit Symbol und steigenden Balken ----------
def plan_board():
    w, h = 1536, 640
    img = Image.new("RGB", (w, h), (24, 20, 18))
    d = ImageDraw.Draw(img)
    pw = w // 3
    for i in range(3):
        x0 = i * pw + 20
        d.rectangle([x0, 20, x0 + pw - 40, h - 20], fill=(236, 226, 200))
        paper = noise_layer((pw - 40, h - 40), 6, 0.35)
        img.paste(Image.new("RGB", paper.size, (200, 185, 150)), (x0, 20), paper)
        d = ImageDraw.Draw(img)
        # Symbol
        ix, iy = x0 + 60, 330
        if i == 0:
            d.rectangle([ix - 25, iy - 40, ix + 25, iy + 70], fill=(250, 250, 245), outline=(80, 80, 80), width=4)
            d.rectangle([ix - 12, iy - 70, ix + 12, iy - 40], fill=(250, 250, 245), outline=(80, 80, 80), width=4)
        elif i == 1:
            d.ellipse([ix - 40, iy - 50, ix + 40, iy + 60], fill=(214, 160, 100), outline=(120, 80, 40), width=4)
        else:
            d.ellipse([ix - 55, iy - 35, ix + 55, iy + 45], fill=(176, 40, 40), outline=(100, 20, 20), width=4)
            d.ellipse([ix - 15, iy - 5, ix + 15, iy + 20], fill=(240, 220, 210))
        # Balken
        cols = [(60, 140, 60), (60, 140, 60), (60, 140, 60), (60, 140, 60), (60, 140, 60)]
        for b in range(5):
            bx = x0 + 160 + b * 58
            bh = 60 + b * 52 + random.randint(-10, 10)
            d.rectangle([bx, h - 90 - bh, bx + 40, h - 90], fill=cols[b])
        d.line([(x0 + 150, h - 140), (x0 + 450, h - 380)], fill=(50, 50, 50), width=6)
        d.polygon([(x0 + 450, h - 380), (x0 + 420, h - 375), (x0 + 445, h - 350)], fill=(50, 50, 50))
        d.line([(x0 + 140, h - 88), (x0 + pw - 70, h - 88)], fill=(60, 60, 60), width=4)
    save(img, "PlanBoard.png")


# ---------- Papier (Statustafel, Etiketten) ----------
def paper():
    s = 512
    img = Image.new("RGB", (s, s), (233, 223, 196))
    stains = noise_layer((s, s), 16, 0.6)
    img = Image.composite(Image.new("RGB", (s, s), (196, 176, 136)), img, stains)
    save(img, "Paper.png")


# ---------- Statustafel: Kästchen mit Haken (Text kommt aus der JSON) ----------
def checklist():
    w, h = 512, 640
    img = Image.new("RGB", (w, h), (233, 223, 196))
    stains = noise_layer((w, h), 16, 0.6)
    img = Image.composite(Image.new("RGB", (w, h), (196, 176, 136)), img, stains)
    d = ImageDraw.Draw(img)
    d.rectangle([12, 12, w - 12, h - 12], outline=(70, 60, 50), width=5)
    d.line([(40, 150), (w - 40, 150)], fill=(70, 60, 50), width=4)
    for i in range(4):
        y = 205 + i * 105
        d.rectangle([40, y, 100, y + 60], outline=(40, 40, 40), width=6)
        d.line([(50, y + 30), (68, y + 52), (105, y - 5)], fill=(30, 110, 40), width=10)
    save(img, "Checklist.png")


# ---------- Tank-Symbole (schwarz auf transparent) ----------
def icons():
    s = 256
    leaf = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(leaf)
    d.ellipse([50, 40, 210, 220], fill=(20, 20, 20, 255))
    d.polygon([(40, 230), (60, 210), (200, 50), (215, 60)], fill=(0, 0, 0, 0))
    d.line([(60, 205), (190, 70)], fill=(0, 0, 0, 0), width=10)
    save(leaf, "IconLeaf.png")
    drop = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(drop)
    d.polygon([(128, 25), (60, 150), (196, 150)], fill=(20, 20, 20, 255))
    d.ellipse([60, 100, 196, 236], fill=(20, 20, 20, 255))
    save(drop, "IconDrop.png")
    mol = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(mol)
    pts = [(128, 128), (128, 40), (50, 190), (206, 190)]
    for p in pts[1:]:
        d.line([pts[0], p], fill=(20, 20, 20, 255), width=14)
    for p in pts:
        d.ellipse([p[0] - 28, p[1] - 28, p[0] + 28, p[1] + 28], fill=(20, 20, 20, 255))
    save(mol, "IconMolecule.png")


# ---------- Weiche Verläufe ----------
def soft():
    s = 256
    img = Image.new("L", (s, s), 0)
    for y in range(s):
        for x in range(s):
            r = math.hypot(x - s / 2, y - s / 2) / (s / 2)
            img.putpixel((x, y), int(255 * max(0.0, 1 - r) ** 2))
    smoke = noise_layer((s, s), 12, 1.6).filter(ImageFilter.GaussianBlur(6))
    combined = Image.eval(Image.merge("RGB", (img, smoke, img)), lambda v: v)
    alpha = Image.composite(img, Image.eval(img, lambda v: int(v * 0.55)), smoke)
    out = Image.merge("RGBA", (Image.new("L", (s, s), 255),) * 3 + (alpha,))
    save(out, "SoftSmoke.png")
    cone = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    for y in range(256):
        for x in range(256):
            u = x / 255.0
            edge = 1.0
            fall = (1 - y / 255.0) ** 1.6
            a = int(255 * edge * fall)
            cone.putpixel((x, y), (255, 255, 255, a))
    save(cone, "ConeGradient.png")


hazard()
emblem(CREAM + (255,), "EmblemCream.png")
emblem(GOLD + (255,), "EmblemGold.png")
director()
plan_board()
paper()
checklist()
icons()
soft()
