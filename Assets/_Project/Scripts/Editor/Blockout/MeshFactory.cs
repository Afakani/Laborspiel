using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Einfache Meshes, die Unity nicht als Grundkörper hat (Ring, Kegel). Werden als Assets gespeichert
    /// und bekommen Lightmap-UVs.
    /// </summary>
    public static class MeshFactory
    {
        const string Dir = "Assets/_Project/Art/Models/Blockout";

        /// <summary>Torus in der XZ-Ebene (Ring um die Y-Achse).</summary>
        public static Mesh Torus(float majorRadius, float minorRadius, int segments = 48, int sides = 10)
        {
            string name = $"Torus_{majorRadius:0.00}_{minorRadius:0.00}";
            var cached = Load(name);
            if (cached != null) return cached;

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float u = (float)i / segments * Mathf.PI * 2f;
                var center = new Vector3(Mathf.Cos(u) * majorRadius, 0f, Mathf.Sin(u) * majorRadius);
                var outward = new Vector3(Mathf.Cos(u), 0f, Mathf.Sin(u));
                for (int j = 0; j <= sides; j++)
                {
                    float v = (float)j / sides * Mathf.PI * 2f;
                    var n = outward * Mathf.Cos(v) + Vector3.up * Mathf.Sin(v);
                    verts.Add(center + n * minorRadius);
                    normals.Add(n);
                    uvs.Add(new Vector2((float)i / segments, (float)j / sides));
                }
            }
            int row = sides + 1;
            for (int i = 0; i < segments; i++)
            for (int j = 0; j < sides; j++)
            {
                int a = i * row + j, b = (i + 1) * row + j;
                tris.AddRange(new[] { a, a + 1, b, b, a + 1, b + 1 });
            }
            return Save(name, verts, normals, uvs, tris);
        }

        /// <summary>Kegel um die Y-Achse, Spitze oben, Boden bei y=0 (offen, beidseitig sichtbar).</summary>
        public static Mesh Cone(float radius, float height, int segments = 24)
        {
            string name = $"Cone_{radius:0.00}_{height:0.00}";
            var cached = Load(name);
            if (cached != null) return cached;

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int side = 0; side < 2; side++)
            {
                int start = verts.Count;
                float sign = side == 0 ? 1f : -1f;
                for (int i = 0; i <= segments; i++)
                {
                    float u = (float)i / segments * Mathf.PI * 2f;
                    var dir = new Vector3(Mathf.Cos(u), 0f, Mathf.Sin(u));
                    var n = (dir * height + Vector3.up * radius).normalized * sign;
                    verts.Add(dir * radius);
                    normals.Add(n);
                    uvs.Add(new Vector2((float)i / segments, 0f));
                    verts.Add(Vector3.up * height);
                    normals.Add(n);
                    uvs.Add(new Vector2((float)i / segments, 1f));
                }
                for (int i = 0; i < segments; i++)
                {
                    int a = start + i * 2, b = a + 2;
                    if (side == 0) tris.AddRange(new[] { a, a + 1, b, b, a + 1, b + 1 });
                    else tris.AddRange(new[] { a, b, a + 1, b, b + 1, a + 1 });
                }
            }
            return Save(name, verts, normals, uvs, tris);
        }

        static Mesh Load(string name) => AssetDatabase.LoadAssetAtPath<Mesh>($"{Dir}/{name}.asset");

        static Mesh Save(string name, List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> t)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(v);
            mesh.SetNormals(n);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(t, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            Unwrapping.GenerateSecondaryUVSet(mesh);
            BlockoutKit.EnsureFolder(Dir);
            AssetDatabase.CreateAsset(mesh, $"{Dir}/{name}.asset");
            return mesh;
        }
    }
}
