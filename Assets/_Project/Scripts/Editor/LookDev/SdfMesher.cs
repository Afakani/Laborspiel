using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Erzeugt glatte, organische Meshes aus Abstandsfeldern (Signed Distance Fields) per Surface Nets:
    /// Grundformen werden weich verschmolzen, Normalen kommen aus dem Feldgradienten (keine Kanten wie bei Grundkörpern).
    /// Optional nur ein Teil der Oberfläche (z. B. Flecken als leicht angehobene Schale).
    /// </summary>
    public static class SdfMesher
    {
        public delegate float Field(Vector3 p);

        /// <summary>
        /// Baut das Mesh der Fläche f = 0 im Bereich bounds. origin wird von allen Punkten abgezogen (Drehpunkt des Teils).
        /// keep entscheidet pro Viereck (Mittelpunkt) über Aufnahme; offset hebt die Fläche entlang der Normalen an.
        /// </summary>
        public static Mesh Build(string name, Field f, Bounds bounds, float cell, Vector3 origin, Func<Vector3, bool> keep = null, float offset = 0f)
        {
            int nx = Mathf.CeilToInt(bounds.size.x / cell) + 2;
            int ny = Mathf.CeilToInt(bounds.size.y / cell) + 2;
            int nz = Mathf.CeilToInt(bounds.size.z / cell) + 2;
            var min = bounds.min - Vector3.one * cell * 0.5f;
            var val = new float[nx * ny * nz];
            int Idx(int i, int j, int k) => (k * ny + j) * nx + i;
            Vector3 P(int i, int j, int k) => min + new Vector3(i, j, k) * cell;

            for (int k = 0; k < nz; k++)
            for (int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
                val[Idx(i, j, k)] = f(P(i, j, k));

            // Ein Punkt pro Zelle mit Vorzeichenwechsel
            var cellVert = new int[(nx - 1) * (ny - 1) * (nz - 1)];
            int CIdx(int i, int j, int k) => (k * (ny - 1) + j) * (nx - 1) + i;
            for (int c = 0; c < cellVert.Length; c++) cellVert[c] = -1;
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var corner = new Vector3Int[8];
            for (int c = 0; c < 8; c++) corner[c] = new Vector3Int(c & 1, (c >> 1) & 1, (c >> 2) & 1);
            int[,] edges = { { 0, 1 }, { 2, 3 }, { 4, 5 }, { 6, 7 }, { 0, 2 }, { 1, 3 }, { 4, 6 }, { 5, 7 }, { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 } };
            float eps = cell * 0.5f;

            for (int k = 0; k < nz - 1; k++)
            for (int j = 0; j < ny - 1; j++)
            for (int i = 0; i < nx - 1; i++)
            {
                int inside = 0;
                for (int c = 0; c < 8; c++)
                    if (val[Idx(i + corner[c].x, j + corner[c].y, k + corner[c].z)] < 0f) inside++;
                if (inside == 0 || inside == 8) continue;

                var sum = Vector3.zero;
                int n = 0;
                for (int e = 0; e < 12; e++)
                {
                    var a = corner[edges[e, 0]];
                    var b = corner[edges[e, 1]];
                    float va = val[Idx(i + a.x, j + a.y, k + a.z)], vb = val[Idx(i + b.x, j + b.y, k + b.z)];
                    if ((va < 0f) == (vb < 0f)) continue;
                    float t = va / (va - vb);
                    sum += Vector3.Lerp(P(i + a.x, j + a.y, k + a.z), P(i + b.x, j + b.y, k + b.z), t);
                    n++;
                }
                var avg = sum / n;
                var p = avg;
                // Auf die Fläche ziehen (zwei Newton-Schritte, je höchstens eine halbe Zelle), Normale aus dem Gradienten
                for (int it = 0; it < 2; it++)
                {
                    var g = Gradient(f, p, eps);
                    float gg = g.sqrMagnitude;
                    if (gg < 1e-8f) break;
                    p -= Vector3.ClampMagnitude(g * (f(p) / gg), cell * 0.5f);
                }
                if ((p - avg).sqrMagnitude > cell * cell) p = avg;
                var nrm = Gradient(f, p, eps).normalized;
                cellVert[CIdx(i, j, k)] = verts.Count;
                verts.Add(p + nrm * offset);
                norms.Add(nrm);
            }

            // Vierecke über jede Gitterkante mit Vorzeichenwechsel
            var tris = new List<int>();
            void Quad(int a, int b, int c, int d)
            {
                if (a < 0 || b < 0 || c < 0 || d < 0) return;
                if (keep != null && !keep((verts[a] + verts[b] + verts[c] + verts[d]) * 0.25f)) return;
                Tri(a, b, c);
                Tri(a, c, d);
            }
            void Tri(int a, int b, int c)
            {
                var cross = Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]);
                if (Vector3.Dot(cross, norms[a] + norms[b] + norms[c]) >= 0f) tris.AddRange(new[] { a, b, c });
                else tris.AddRange(new[] { a, c, b });
            }

            for (int k = 1; k < nz - 1; k++)
            for (int j = 1; j < ny - 1; j++)
            for (int i = 1; i < nx - 1; i++)
            {
                bool s0 = val[Idx(i, j, k)] < 0f;
                if (i < nx - 1 && s0 != val[Idx(i + 1, j, k)] < 0f)
                    Quad(cellVert[CIdx(i, j - 1, k - 1)], cellVert[CIdx(i, j, k - 1)], cellVert[CIdx(i, j, k)], cellVert[CIdx(i, j - 1, k)]);
                if (j < ny - 1 && s0 != val[Idx(i, j + 1, k)] < 0f)
                    Quad(cellVert[CIdx(i - 1, j, k - 1)], cellVert[CIdx(i, j, k - 1)], cellVert[CIdx(i, j, k)], cellVert[CIdx(i - 1, j, k)]);
                if (k < nz - 1 && s0 != val[Idx(i, j, k + 1)] < 0f)
                    Quad(cellVert[CIdx(i - 1, j - 1, k)], cellVert[CIdx(i, j - 1, k)], cellVert[CIdx(i, j, k)], cellVert[CIdx(i - 1, j, k)]);
            }

            // Nur benutzte Punkte übernehmen (wichtig für Teilflächen), relativ zum Drehpunkt
            var remap = new int[verts.Count];
            for (int r = 0; r < remap.Length; r++) remap[r] = -1;
            var outV = new List<Vector3>();
            var outN = new List<Vector3>();
            for (int t = 0; t < tris.Count; t++)
            {
                int v = tris[t];
                if (remap[v] < 0)
                {
                    remap[v] = outV.Count;
                    outV.Add(verts[v] - origin);
                    outN.Add(norms[v]);
                }
                tris[t] = remap[v];
            }

            var mesh = new Mesh { name = name };
            if (outV.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(outV);
            mesh.SetNormals(outN);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Gradient per zentraler Differenz.</summary>
        static Vector3 Gradient(Field f, Vector3 p, float e) => new Vector3(
            f(p + new Vector3(e, 0f, 0f)) - f(p - new Vector3(e, 0f, 0f)),
            f(p + new Vector3(0f, e, 0f)) - f(p - new Vector3(0f, e, 0f)),
            f(p + new Vector3(0f, 0f, e)) - f(p - new Vector3(0f, 0f, e))) / (2f * e);

        /// <summary>Speichert das Mesh als Asset (überschreibt ein vorhandenes, damit Verweise erhalten bleiben).</summary>
        public static Mesh Save(Mesh mesh, string path)
        {
            BlockoutKit.EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            CopyInto(mesh, existing);
            UnityEngine.Object.DestroyImmediate(mesh);
            return existing;
        }

        /// <summary>
        /// Überträgt die Daten über die Mesh-API (nicht CopySerialized), damit Unity die GPU-Puffer neu hochlädt.
        /// </summary>
        public static void CopyInto(Mesh src, Mesh dst)
        {
            dst.Clear();
            dst.indexFormat = src.indexFormat;
            dst.SetVertices(src.vertices);
            dst.SetNormals(src.normals);
            if (src.tangents.Length == src.vertexCount) dst.SetTangents(src.tangents);
            if (src.uv.Length == src.vertexCount) dst.SetUVs(0, src.uv);
            if (src.uv2.Length == src.vertexCount) dst.SetUVs(1, src.uv2);
            dst.subMeshCount = src.subMeshCount;
            for (int i = 0; i < src.subMeshCount; i++) dst.SetTriangles(src.GetTriangles(i), i);
            dst.RecalculateBounds();
            dst.UploadMeshData(false);
            EditorUtility.SetDirty(dst);
        }

        // ---------- Grundformen ----------

        public static float Sphere(Vector3 p, Vector3 c, float r) => (p - c).magnitude - r;

        /// <summary>Ellipsoid (Näherung nach Quilez), optional gedreht.</summary>
        public static float Ellipsoid(Vector3 p, Vector3 c, Vector3 r, Quaternion? rot = null)
        {
            var q = p - c;
            if (rot.HasValue) q = Quaternion.Inverse(rot.Value) * q;
            float k0 = new Vector3(q.x / r.x, q.y / r.y, q.z / r.z).magnitude;
            float k1 = new Vector3(q.x / (r.x * r.x), q.y / (r.y * r.y), q.z / (r.z * r.z)).magnitude;
            return k1 > 1e-6f ? k0 * (k0 - 1f) / k1 : -Mathf.Min(r.x, Mathf.Min(r.y, r.z));
        }

        /// <summary>Kapsel von a nach b mit Radius ra → rb.</summary>
        public static float Capsule(Vector3 p, Vector3 a, Vector3 b, float ra, float rb)
        {
            var pa = p - a;
            var ba = b - a;
            float h = Mathf.Clamp01(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba));
            return (pa - ba * h).magnitude - Mathf.Lerp(ra, rb, h);
        }

        /// <summary>Weiche Vereinigung (k = Rundungsbreite in m).</summary>
        public static float SMin(float a, float b, float k)
        {
            float h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
            return Mathf.Lerp(b, a, h) - k * h * (1f - h);
        }

        /// <summary>Weiches Abziehen von b aus a.</summary>
        public static float SSub(float a, float b, float k) => -SMin(-a, b, k);
    }
}
