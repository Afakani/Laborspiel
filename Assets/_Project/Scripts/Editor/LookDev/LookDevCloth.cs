using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Laken über der Kammer und Plane über dem Serum-Regal (GDD 6.8, 9): Leinen mit Falten, die nach unten
    /// stärker werden, gewölbte Haube über dem Kopfring, roter Saum. Drehpunkt oben, damit ClothVisual es
    /// von oben herablassen und schwingen lassen kann.
    /// </summary>
    public static class LookDevCloth
    {
        static Material canvas;

        /// <summary>Leinen-Material (beidseitig, Emission vorbereitet für das goldene Ω-Leuchten).</summary>
        public static Material Canvas()
        {
            if (canvas != null) return canvas;
            canvas = LookDevKit.Textured("LD_Canvas", "Canvas.png", new Color(0.74f, 0.70f, 0.62f), 0f, 0.08f, 0.0001f);
            canvas.SetFloat("_Cull", 0f);
            canvas.SetColor("_EmissionColor", Color.black);
            UnityEditor.EditorUtility.SetDirty(canvas);
            return canvas;
        }

        /// <summary>Laken-Objekt mit Drehpunkt oben (top = Höhe über dem Kammerboden).</summary>
        public static GameObject Build(Transform parent, float topRadius, float bottomRadius, float height, float top)
        {
            var pivot = BlockoutKit.Group(parent, "Cloth", new Vector3(0f, top, 0f));
            var mesh = Drape(topRadius, bottomRadius, height, 0.35f);
            var go = LookDevKit.Obj(pivot, "Drape", mesh, Vector3.zero, Canvas(), null, false, true);
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
            return pivot.gameObject;
        }

        /// <summary>Plane über dem Serum-Regal (Drehpunkt oben).</summary>
        public static Transform BuildRackCover(Transform parent, Vector3 topCenter, Vector3 size)
        {
            var pivot = BlockoutKit.Group(parent, "RackCover", topCenter);
            LookDevKit.Box(pivot, "Top", new Vector3(0f, 0.01f, 0f), new Vector3(size.x, 0.02f, size.z), Canvas(), null, false);
            LookDevKit.Box(pivot, "Front", new Vector3(-size.x * 0.5f, -size.y * 0.5f, 0f), new Vector3(0.015f, size.y, size.z), Canvas(), null, false);
            LookDevKit.Box(pivot, "Back", new Vector3(size.x * 0.5f, -size.y * 0.5f, 0f), new Vector3(0.015f, size.y, size.z), Canvas(), null, false);
            LookDevKit.Box(pivot, "SideA", new Vector3(0f, -size.y * 0.5f, -size.z * 0.5f), new Vector3(size.x, size.y, 0.015f), Canvas(), null, false);
            LookDevKit.Box(pivot, "SideB", new Vector3(0f, -size.y * 0.5f, size.z * 0.5f), new Vector3(size.x, size.y, 0.015f), Canvas(), null, false);
            return pivot;
        }

        /// <summary>Faltenwurf: Mantel von oben (y = 0) nach unten (y = −height) plus gewölbte Haube.</summary>
        static UnityEngine.Mesh Drape(float rTop, float rBottom, float height, float capRise)
        {
            const int seg = 96, rings = 18, capRings = 6;
            const float uRepeat = 6f;
            int cols = seg + 1;
            var verts = new System.Collections.Generic.List<Vector3>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            var tris = new System.Collections.Generic.List<int>();

            // Mantel
            for (int i = 0; i <= rings; i++)
            {
                float t = i / (float)rings;
                float amp = 0.3f + 0.7f * t;
                float r = Mathf.Lerp(rTop, rBottom, t * t);
                for (int j = 0; j <= seg; j++)
                {
                    float a = j / (float)seg * Mathf.PI * 2f;
                    float f = 1f + amp * (0.045f * Mathf.Sin(a * 14f) + 0.02f * Mathf.Sin(a * 31f + 1.3f) + 0.012f * Mathf.Sin(a * 5f + t * 3f));
                    verts.Add(new Vector3(Mathf.Cos(a) * r * f, -t * height, Mathf.Sin(a) * r * f));
                    uvs.Add(new Vector2(j / (float)seg * uRepeat, 1f - t));
                }
            }
            for (int i = 0; i < rings; i++)
                for (int j = 0; j < seg; j++)
                {
                    int a = i * cols + j, b = a + 1, c = a + cols, d = c + 1;
                    tris.Add(a); tris.Add(b); tris.Add(c);
                    tris.Add(b); tris.Add(d); tris.Add(c);
                }

            // Haube: Ringe von außen (Rand des Mantels) zur Mitte, leicht gewölbt
            int capStart = verts.Count;
            for (int k = 0; k < capRings; k++)
            {
                float t = k / (float)capRings;
                float r = rTop * (1f - t);
                float y = capRise * Mathf.Sin(t * Mathf.PI * 0.5f);
                for (int j = 0; j <= seg; j++)
                {
                    float a = j / (float)seg * Mathf.PI * 2f;
                    float f = 1f + (1f - t) * 0.3f * (0.045f * Mathf.Sin(a * 14f) + 0.02f * Mathf.Sin(a * 31f + 1.3f));
                    verts.Add(new Vector3(Mathf.Cos(a) * r * f, y, Mathf.Sin(a) * r * f));
                    uvs.Add(new Vector2(j / (float)seg * uRepeat, 1f + t * 0.2f));
                }
            }
            int center = verts.Count;
            verts.Add(new Vector3(0f, capRise, 0f));
            uvs.Add(new Vector2(uRepeat * 0.5f, 1.2f));
            for (int k = 0; k < capRings - 1; k++)
                for (int j = 0; j < seg; j++)
                {
                    int outer0 = capStart + k * cols + j, outer1 = outer0 + 1;
                    int inner0 = outer0 + cols, inner1 = inner0 + 1;
                    tris.Add(inner0); tris.Add(outer1); tris.Add(outer0);
                    tris.Add(inner0); tris.Add(inner1); tris.Add(outer1);
                }
            int last = capStart + (capRings - 1) * cols;
            for (int j = 0; j < seg; j++)
            {
                tris.Add(center); tris.Add(last + j + 1); tris.Add(last + j);
            }

            var m = new UnityEngine.Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            // Naht bei 0°/360°: Normalen beider Spalten mitteln
            var n = m.normals;
            for (int row = 0; row < verts.Count - 1; row += cols)
            {
                if (row + seg >= n.Length) break;
                var avg = (n[row] + n[row + seg]).normalized;
                n[row] = avg;
                n[row + seg] = avg;
            }
            m.normals = n;
            m.RecalculateTangents();
            m.RecalculateBounds();
            return LookDevKit.SaveMesh("ClothDrape", m, true);
        }
    }
}
