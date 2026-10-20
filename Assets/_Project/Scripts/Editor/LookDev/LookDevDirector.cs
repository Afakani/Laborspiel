using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Baut den Direktor im Fernseher aus den Ebenen von Tools/gen_director.py: Körper als Bildschirmfläche, darüber
    /// Kopfgruppe (Drehpunkt am Hals) mit Kopf, Augen, Lidern, Brauen, Mund (vier Formen), Brille, Schweiß und Ader.
    /// Lage und Größe jeder Ebene kommen aus DirectorRig.json (Pixel), umgerechnet auf die Bildschirmfläche.
    /// </summary>
    public static class LookDevDirector
    {
        const float Emission = 1.6f;

        /// <summary>Rig auf einer Bildschirmfläche (Mitte, Größe in Metern, lokal im TV) aufbauen.</summary>
        public static DirectorRig Build(Transform tv, Vector3 center, Vector2 size)
        {
            var json = JObject.Parse(File.ReadAllText(Path.Combine(LookDevKit.OwnTexDir, "DirectorRig.json")));
            var canvas = json["canvas"].ToObject<float[]>();
            var neckPx = json["neck"].ToObject<float[]>();
            var parts = (JObject)json["parts"];

            Vector3 ToLocal(float px, float py) => center + new Vector3((px / canvas[0] - 0.5f) * size.x, (0.5f - py / canvas[1]) * size.y, 0f);

            var root = BlockoutKit.Group(tv, "Director", Vector3.zero);
            var all = new List<Renderer>();
            var body = LookDevKit.Plane(root, "Body", center, size, Mat("Dir_Body", false), null, false);
            all.Add(body.GetComponent<Renderer>());

            var neck = ToLocal(neckPx[0], neckPx[1]);
            var head = BlockoutKit.Group(root, "Head", neck);
            int order = 0;
            GameObject Part(string name, string tex, float dz)
            {
                var r = parts[tex].ToObject<float[]>();
                var c = ToLocal((r[0] + r[2]) * 0.5f, (r[1] + r[3]) * 0.5f) - neck;
                var s = new Vector2((r[2] - r[0]) / canvas[0] * size.x, (r[3] - r[1]) / canvas[1] * size.y);
                var go = LookDevKit.Plane(head, name, c + Vector3.back * (0.02f + dz * 4f + order++ * 0.0005f), s, Mat(tex, true), null, false);
                all.Add(go.GetComponent<Renderer>());
                return go;
            }

            var headGo = Part("Face", "Dir_Head", 0f);
            Part("Eyes", "Dir_Eyes", 0.003f);
            var lids = Part("Lids", "Dir_Lids", 0.0065f);
            var browL = Pivot(Part("BrowL", "Dir_BrowL", 0.009f));
            var browR = Pivot(Part("BrowR", "Dir_BrowR", 0.009f));
            var mouth = Part("Mouth", "Dir_Mouth0", 0.006f);
            Part("Glasses", "Dir_Glasses", 0.012f);
            var vein = Pivot(Part("Vein", "Dir_Vein", 0.009f));
            var sweatA = Part("SweatA", "Dir_Sweat", 0.015f);
            // zweiter Tropfen gespiegelt auf der anderen Kopfseite
            var sweatB = Object.Instantiate(sweatA, head);
            sweatB.name = "SweatB";
            var pb = sweatA.transform.localPosition;
            float headCx = ToLocal(canvas[0] * 0.5f, 0f).x - neck.x;
            sweatB.transform.localPosition = new Vector3(2f * headCx - pb.x, pb.y - 0.08f, pb.z);
            all.Add(sweatB.GetComponent<Renderer>());

            var frames = new Material[4];
            for (int i = 0; i < 4; i++) frames[i] = Mat("Dir_Mouth" + i, true);

            var rig = root.gameObject.AddComponent<DirectorRig>();
            rig.Setup(all.ToArray(), new[] { headGo.GetComponent<Renderer>(), lids.GetComponent<Renderer>() }, head, browL, browR,
                mouth.GetComponent<Renderer>(), frames, lids, new[] { sweatA.transform, sweatB.transform }, vein);
            return rig;
        }

        /// <summary>Teil unter eine eigene Gruppe in seiner Mitte hängen (Drehen/Skalieren um die Mitte).</summary>
        static Transform Pivot(GameObject part)
        {
            var g = BlockoutKit.Group(part.transform.parent, part.name + "Pivot", part.transform.localPosition);
            part.transform.SetParent(g, true);
            return g;
        }

        static Material Mat(string tex, bool cutout) => LookDevKit.Textured("LD_" + tex, tex + ".png", Color.white, 0f, 0.4f, Emission, cutout);
    }
}
