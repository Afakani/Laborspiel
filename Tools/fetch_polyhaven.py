# Lädt CC0-Texturen von Poly Haven und packt sie für URP Lit:
#   <id>_Albedo.jpg, <id>_Normal.png (OpenGL), <id>_Mask.png (R = Metallic, G = AO, A = Glätte)
# Aufruf: python Tools/fetch_polyhaven.py <Zielordner>
import io, json, os, sys, urllib.request
from PIL import Image

TEXTURES = [
    # (Poly-Haven-Id, Auflösung, Metallic-Wert falls keine Metal-Map)
    ("rusty_painted_metal", "2k", 0.0),
    ("blue_metal_plate", "2k", 0.0),
    ("green_metal_rust", "1k", 0.0),
    ("metal_plate", "1k", 0.5),
    ("metal_grate_rusty", "1k", 0.6),
    ("rusty_metal_02", "1k", 0.2),
    ("rusty_metal_03", "1k", 0.7),
    ("rusty_metal_sheet", "1k", 0.4),
    ("concrete_floor_worn_001", "2k", 0.0),
    ("concrete_wall_006", "1k", 0.0),
    ("painted_metal_shutter", "1k", 0.0),
    ("dark_wood", "1k", 0.0),
    ("old_planks_02", "1k", 0.0),
]

def get(url):
    req = urllib.request.Request(url, headers={"User-Agent": "PlanErfuellt-Unity/1.0 (texture import)"})
    with urllib.request.urlopen(req) as r:
        return r.read()

def pick(files, key, res, fmt):
    try:
        return files[key][res][fmt]["url"]
    except KeyError:
        return None

def main(out_root):
    for tid, res, metal_const in TEXTURES:
        out = os.path.join(out_root, tid)
        os.makedirs(out, exist_ok=True)
        if os.path.exists(os.path.join(out, tid + "_Mask.png")):
            print("vorhanden:", tid)
            continue
        files = json.loads(get("https://api.polyhaven.com/files/" + tid))
        albedo = get(pick(files, "Diffuse", res, "jpg"))
        open(os.path.join(out, tid + "_Albedo.jpg"), "wb").write(albedo)
        normal = Image.open(io.BytesIO(get(pick(files, "nor_gl", res, "jpg")))).convert("RGB")
        normal.save(os.path.join(out, tid + "_Normal.png"))
        rough = Image.open(io.BytesIO(get(pick(files, "Rough", res, "jpg")))).convert("L")
        size = rough.size
        ao_url = pick(files, "AO", res, "jpg")
        ao = Image.open(io.BytesIO(get(ao_url))).convert("L").resize(size) if ao_url else Image.new("L", size, 255)
        metal_url = pick(files, "Metal", res, "jpg")
        metal = Image.open(io.BytesIO(get(metal_url))).convert("L").resize(size) if metal_url else Image.new("L", size, int(metal_const * 255))
        smooth = rough.point(lambda v: 255 - v)
        Image.merge("RGBA", (metal, ao, Image.new("L", size, 0), smooth)).save(os.path.join(out, tid + "_Mask.png"))
        print("geladen:", tid, res, "Metal-Map" if metal_url else "Metal=%.1f" % metal_const)

if __name__ == "__main__":
    main(sys.argv[1])
