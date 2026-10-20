# Erzeugt den Direktor als erfundene Comicfigur (dicker Funktionär: Glatze mit Haarkranz, runde Brille,
# Doppelkinn, buschige Brauen, Fliege, viele Orden) in Ebenen für die Animation im Fernseher:
#   Dir_Body.png (Hintergrund, Körper, Tisch, Mikrofon), Dir_Head.png (Kopf, Ohren, Haarkranz, Nase),
#   Dir_Eyes, Dir_Lids, Dir_BrowL, Dir_BrowR, Dir_Mouth0..3, Dir_Glasses, Dir_Sweat, Dir_Vein
# sowie DirectorRig.json (Lage jeder Ebene in Pixeln) und Director.png (Gesamtbild für Plakat, Porträt, Hauptmenü).
# Keine echte Person als Vorlage; keine Fremdquellen.
# Aufruf: python Tools/gen_director.py <Zielordner>
import json, math, os, random, sys
from PIL import Image, ImageDraw, ImageFilter

random.seed(7)
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

W, H = 1100, 1000            # Seitenverhältnis des TV-Bildschirms (3.4 × 3.1 m)
CX = W // 2
SKIN = (238, 188, 158)
SKIN_DARK = (214, 158, 128)
SKIN_LINE = (170, 108, 86)
HAIR = (128, 118, 108)
SUIT = (52, 58, 74)
SUIT_DARK = (36, 40, 52)
GOLD = (222, 172, 40)
RED = (190, 26, 30)
LINE = (40, 30, 26)


def layer():
    return Image.new("RGBA", (W, H), (0, 0, 0, 0))


def outline_ellipse(d, box, fill, width=6, outline=LINE):
    d.ellipse(box, fill=fill, outline=outline, width=width)


# ---------- Hintergrund und Körper ----------
def body():
    img = Image.new("RGBA", (W, H), (96, 12, 16, 255))
    d = ImageDraw.Draw(img)
    for x in range(0, W, 56):
        d.rectangle([x, 0, x + 26, H], fill=(132, 20, 26, 255))
    glow = Image.new("L", (W, H), 0)
    ImageDraw.Draw(glow).ellipse([CX - 420, 40, CX + 420, 860], fill=120)
    glow = glow.filter(ImageFilter.GaussianBlur(90))
    img = Image.composite(Image.new("RGBA", (W, H), (220, 80, 64, 255)), img, glow)
    d = ImageDraw.Draw(img)
    # Emblem-Andeutung oben links und rechts (goldene Kreise mit Stern)
    for ex in (150, W - 150):
        d.ellipse([ex - 70, 70, ex + 70, 210], outline=GOLD, width=8)
        star(d, ex, 140, 44, 18, GOLD)

    # Schultern und Bauch (breit, rund)
    d.ellipse([CX - 470, 640, CX + 470, 1300], fill=SUIT, outline=LINE, width=8)
    # Revers
    d.polygon([(CX - 150, 650), (CX - 40, 820), (CX - 230, 760)], fill=SUIT_DARK, outline=LINE)
    d.polygon([(CX + 150, 650), (CX + 40, 820), (CX + 230, 760)], fill=SUIT_DARK, outline=LINE)
    # Hemd
    d.polygon([(CX - 130, 650), (CX, 860), (CX + 130, 650)], fill=(240, 236, 226), outline=LINE)
    # Fliege
    d.polygon([(CX, 728), (CX - 85, 688), (CX - 85, 773)], fill=RED, outline=LINE)
    d.polygon([(CX, 728), (CX + 85, 688), (CX + 85, 773)], fill=RED, outline=LINE)
    d.ellipse([CX - 22, 710, CX + 22, 748], fill=(150, 18, 22), outline=LINE, width=4)
    # Orden: Ordensspangen und Medaillen auf beiden Seiten
    cols = [(200, 30, 30), GOLD, (40, 90, 170), (30, 130, 70), (240, 240, 240)]
    for side in (-1, 1):
        for row in range(3):
            for k in range(4):
                x = CX + side * (200 + k * 52) - 22
                y = 768 + row * 32
                c1, c2 = cols[(k + row) % 5], cols[(k + row + 2) % 5]
                d.rectangle([x, y, x + 44, y + 26], fill=c1, outline=LINE, width=2)
                d.rectangle([x + 16, y, x + 28, y + 26], fill=c2)
        for k in range(3):
            x = CX + side * (230 + k * 70)
            d.line([(x, 868), (x, 896)], fill=RED, width=10)
            d.ellipse([x - 24, 890, x + 24, 938], fill=GOLD, outline=LINE, width=3)
            star(d, x, 914, 15, 6, (180, 120, 20))
    # Tisch und Mikrofon
    d.rectangle([0, 950, W, H], fill=(70, 40, 26, 255), outline=LINE, width=6)
    d.line([(0, 960), (W, 960)], fill=(110, 66, 40), width=6)
    mx = CX - 90
    d.rectangle([mx - 8, 860, mx + 8, 955], fill=(60, 60, 60))
    d.rounded_rectangle([mx - 34, 790, mx + 34, 880], radius=30, fill=(150, 150, 150), outline=LINE, width=5)
    for y in range(805, 870, 12):
        d.line([(mx - 26, y), (mx + 26, y)], fill=(90, 90, 90), width=3)
    return img


def star(d, x, y, r_out, r_in, col):
    pts = []
    for i in range(10):
        a = -math.pi / 2 + i * math.pi / 5
        rr = r_out if i % 2 == 0 else r_in
        pts.append((x + math.cos(a) * rr, y + math.sin(a) * rr))
    d.polygon(pts, fill=col)


# ---------- Kopf (ohne Augen, Brauen, Mund, Brille) ----------
def head():
    img = layer()
    d = ImageDraw.Draw(img)
    # Doppelkinn
    outline_ellipse(d, [CX - 170, 540, CX + 170, 715], SKIN_DARK)
    d.arc([CX - 120, 600, CX + 120, 700], 20, 160, fill=SKIN_LINE, width=5)
    # Ohren
    for side in (-1, 1):
        ex = CX + side * 228
        outline_ellipse(d, [ex - 40, 360, ex + 40, 480], SKIN_DARK)
        d.arc([ex - 22, 380, ex + 22, 460], 60 if side < 0 else -120, 300 if side < 0 else 120, fill=SKIN_LINE, width=4)
    # Gesicht: rund, unten breiter (Hängebacken)
    outline_ellipse(d, [CX - 230, 150, CX + 230, 650], SKIN)
    d.ellipse([CX - 236, 380, CX + 236, 640], fill=SKIN)
    d.arc([CX - 236, 380, CX + 236, 640], 0, 180, fill=LINE, width=6)
    # Glatze mit Glanzlicht
    d.ellipse([CX - 110, 175, CX + 10, 235], fill=(252, 222, 200))
    d.ellipse([CX - 80, 188, CX - 30, 210], fill=(255, 240, 228))
    # Haarkranz seitlich und hinten
    for side in (-1, 1):
        for k in range(9):
            hx = CX + side * (200 + math.sin(k * 0.6) * 18)
            hy = 285 + k * 18
            d.ellipse([hx - 30, hy - 22, hx + 30, hy + 22], fill=HAIR, outline=(90, 82, 76), width=3)
    # Stirnfalten
    for k in range(3):
        d.arc([CX - 110 + k * 10, 240 + k * 22, CX + 110 - k * 10, 300 + k * 22], 200, 340, fill=SKIN_LINE, width=4)
    # Wangen (rot) und Knollennase
    for side in (-1, 1):
        cheek = Image.new("L", (W, H), 0)
        ImageDraw.Draw(cheek).ellipse([CX + side * 140 - 55, 480, CX + side * 140 + 55, 560], fill=120)
        cheek = cheek.filter(ImageFilter.GaussianBlur(14))
        img = Image.composite(Image.new("RGBA", (W, H), (226, 120, 110, 255)), img, Image.composite(cheek, Image.new("L", (W, H), 0), img.split()[3]))
    d = ImageDraw.Draw(img)
    outline_ellipse(d, [CX - 46, 440, CX + 46, 520], (222, 140, 118), 5)
    d.ellipse([CX - 18, 452, CX + 4, 468], fill=(245, 190, 170))
    for side in (-1, 1):
        d.ellipse([CX + side * 30 - 12, 500, CX + side * 30 + 12, 514], fill=(150, 80, 66))
    # Falten von der Nase zum Mund
    for side in (-1, 1):
        start, end = (100, 170) if side < 0 else (10, 80)
        d.arc([CX + side * 40 - 70, 450, CX + side * 40 + 70, 590], start, end, fill=SKIN_LINE, width=4)
    return img


# ---------- Teile ----------
def eyes():
    img = layer()
    d = ImageDraw.Draw(img)
    for side in (-1, 1):
        ex = CX + side * 88
        outline_ellipse(d, [ex - 34, 375, ex + 34, 425], (252, 250, 244), 4)
        d.ellipse([ex - 13, 386, ex + 13, 414], fill=(60, 44, 34))
        d.ellipse([ex - 5, 390, ex + 2, 398], fill=(255, 255, 255))
    return img


def lids():
    img = layer()
    d = ImageDraw.Draw(img)
    for side in (-1, 1):
        ex = CX + side * 88
        d.ellipse([ex - 38, 370, ex + 38, 430], fill=SKIN_DARK)
        d.arc([ex - 34, 380, ex + 34, 424], 10, 170, fill=LINE, width=5)
    return img


def brow(side):
    img = layer()
    d = ImageDraw.Draw(img)
    bx = CX + side * 92
    # Keil, innen dicker; Haarstriche
    pts = [(bx - side * 78, 340), (bx + side * 72, 322), (bx + side * 80, 345), (bx - side * 74, 362)]
    d.polygon(pts, fill=(78, 68, 62), outline=LINE)
    for k in range(18):
        x = bx - side * 70 + side * k * 8.5
        y = 330 + random.randint(-4, 6)
        d.line([(x, y + 22), (x + side * 10, y - 6)], fill=(56, 48, 44), width=4)
    return img


def mouths():
    frames = []
    # 0: zufrieden geschlossen
    img = layer(); d = ImageDraw.Draw(img)
    d.arc([CX - 70, 520, CX + 70, 600], 20, 160, fill=LINE, width=9)
    frames.append(img)
    # 1: streng geschlossen (Mundwinkel unten)
    img = layer(); d = ImageDraw.Draw(img)
    d.arc([CX - 70, 565, CX + 70, 625], 200, 340, fill=LINE, width=9)
    frames.append(img)
    # 2: sprechend (halb offen)
    img = layer(); d = ImageDraw.Draw(img)
    d.ellipse([CX - 52, 548, CX + 52, 602], fill=(90, 20, 22), outline=LINE, width=6)
    d.rectangle([CX - 34, 552, CX + 34, 562], fill=(250, 248, 240))
    d.ellipse([CX - 24, 580, CX + 24, 600], fill=(200, 80, 80))
    frames.append(img)
    # 3: brüllend (weit offen, eckig)
    img = layer(); d = ImageDraw.Draw(img)
    d.rounded_rectangle([CX - 78, 535, CX + 78, 630], radius=30, fill=(80, 14, 16), outline=LINE, width=7)
    d.rectangle([CX - 60, 540, CX + 60, 556], fill=(250, 248, 240))
    d.rectangle([CX - 50, 612, CX + 50, 624], fill=(250, 248, 240))
    d.ellipse([CX - 34, 590, CX + 34, 620], fill=(200, 76, 76))
    frames.append(img)
    return frames


def glasses():
    img = layer()
    d = ImageDraw.Draw(img)
    for side in (-1, 1):
        ex = CX + side * 88
        d.ellipse([ex - 64, 336, ex + 64, 464], outline=(34, 30, 28), width=11)
        d.arc([ex - 46, 352, ex + 30, 430], 200, 250, fill=(255, 255, 255), width=6)
        d.line([(ex + side * 64, 392), (CX + side * 214, 410)], fill=(34, 30, 28), width=9)
    d.arc([CX - 30, 372, CX + 30, 410], 200, 340, fill=(34, 30, 28), width=10)
    return img


def sweat():
    img = layer()
    d = ImageDraw.Draw(img)
    x, y = CX + 175, 300
    d.polygon([(x, y - 34), (x - 16, y), (x + 16, y)], fill=(170, 220, 255), outline=(40, 80, 120))
    d.ellipse([x - 17, y - 16, x + 17, y + 18], fill=(170, 220, 255), outline=(40, 80, 120), width=3)
    d.ellipse([x - 8, y - 8, x - 2, y], fill=(255, 255, 255))
    return img


def vein():
    img = layer()
    d = ImageDraw.Draw(img)
    x, y = CX - 120, 250
    for a in (0, 90, 180, 270):
        r = math.radians(a + 45)
        d.line([(x + math.cos(r) * 6, y + math.sin(r) * 6), (x + math.cos(r) * 30, y + math.sin(r) * 30)], fill=(200, 20, 30), width=12)
    return img


# ---------- Ausgabe ----------
rig = {"canvas": [W, H], "neck": [CX, 700], "parts": {}}


def save_full(img, name):
    img.save(os.path.join(OUT, name))
    print("geschrieben:", name)


def save_part(img, name, bbox=None, pad=6):
    bb = bbox or img.getbbox()
    x0, y0, x1, y1 = max(0, bb[0] - pad), max(0, bb[1] - pad), min(W, bb[2] + pad), min(H, bb[3] + pad)
    img.crop((x0, y0, x1, y1)).save(os.path.join(OUT, name + ".png"))
    rig["parts"][name] = [x0, y0, x1, y1]
    print("geschrieben:", name, (x0, y0, x1, y1))


def union(images):
    boxes = [i.getbbox() for i in images]
    return (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))


b = body()
h = head()
e = eyes()
l = lids()
bl, br = brow(-1), brow(1)
ms = mouths()
g = glasses()
s = sweat()
v = vein()

save_full(b.convert("RGB"), "Dir_Body.png")
save_part(h, "Dir_Head")
save_part(e, "Dir_Eyes")
save_part(l, "Dir_Lids")
save_part(bl, "Dir_BrowL")
save_part(br, "Dir_BrowR")
mb = union(ms)
for i, m in enumerate(ms):
    save_part(m, f"Dir_Mouth{i}", mb)
save_part(g, "Dir_Glasses")
save_part(s, "Dir_Sweat")
save_part(v, "Dir_Vein")
with open(os.path.join(OUT, "DirectorRig.json"), "w", encoding="utf-8", newline="\n") as fh:
    json.dump(rig, fh, indent=2)
print("geschrieben: DirectorRig.json")

# Gesamtbild (ruhige Miene) mit Bildschirmzeilen; Format wie bisher 1024 × 640, Figur mittig
full = b.copy()
for part in (h, e, bl, br, ms[0], g):
    full.alpha_composite(part)
full = full.convert("RGB").filter(ImageFilter.SMOOTH_MORE)
poster = Image.new("RGB", (1024, 640), (96, 12, 16))
scaled = full.resize((int(W * 0.64), int(H * 0.64)), Image.LANCZOS)
poster.paste(scaled, ((1024 - scaled.width) // 2, 0))
# Seitenränder mit Vorhang füllen
pd = ImageDraw.Draw(poster)
for x in list(range(0, (1024 - scaled.width) // 2, 56)) + list(range((1024 + scaled.width) // 2, 1024, 56)):
    pd.rectangle([x, 0, x + 26, 640], fill=(132, 20, 26))
lines = Image.new("L", poster.size, 0)
ld = ImageDraw.Draw(lines)
for y in range(0, 640, 4):
    ld.line([(0, y), (1024, y)], fill=36)
poster = Image.composite(Image.new("RGB", poster.size, (0, 0, 0)), poster, lines)
save_full(poster, "Director.png")
