using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Macht aus einem SDF-Mesh eine Haut mit Knochen: Gewichte kommen aus den Abständen zu den Körperteilen
    /// (nächstes Teil voll, Nachbarteile weich auslaufend, höchstens vier Knochen), Blendshapes werden pro Punkt
    /// aus einer Verformungsfunktion berechnet. Gleiche Felder ergeben für Haut und Fleckenschale gleiche Gewichte.
    /// </summary>
    public static class SkinMesher
    {
        /// <summary>Verschiebung eines Punkts für einen Blendshape bei voller Stärke (w = Gewicht je Knochen).</summary>
        public delegate Vector3 ShapeDelta(Vector3 p, Vector3 n, float[] w);

        /// <summary>Setzt Knochengewichte, Bindposes und Blendshapes. Das Mesh liegt im Modellraum (Wurzel im Ursprung).</summary>
        public static Mesh Skin(Mesh mesh, SkinPart[] parts, Transform[] bones, float sigma, IList<(string name, ShapeDelta fn)> shapes)
        {
            var verts = mesh.vertices;
            var norms = mesh.normals;
            int nb = bones.Length;
            var weights = new BoneWeight[verts.Length];
            var allW = new float[verts.Length][];
            var d = new float[parts.Length];

            for (int v = 0; v < verts.Length; v++)
            {
                float dmin = float.MaxValue;
                for (int k = 0; k < parts.Length; k++)
                {
                    d[k] = parts[k].field(verts[v]);
                    dmin = Mathf.Min(dmin, d[k]);
                }
                var w = new float[nb];
                for (int k = 0; k < parts.Length; k++)
                    w[parts[k].bone] = Mathf.Max(w[parts[k].bone], Mathf.Exp(-(d[k] - dmin) / sigma));
                Normalize(w);
                allW[v] = w;
                weights[v] = Top4(w);
            }

            mesh.boneWeights = weights;
            var bind = new Matrix4x4[nb];
            for (int b = 0; b < nb; b++) bind[b] = bones[b].worldToLocalMatrix;
            mesh.bindposes = bind;

            mesh.ClearBlendShapes();
            if (shapes != null)
                foreach (var s in shapes)
                {
                    var delta = new Vector3[verts.Length];
                    for (int v = 0; v < verts.Length; v++) delta[v] = s.fn(verts[v], norms[v], allW[v]);
                    mesh.AddBlendShapeFrame(s.name, 100f, delta, null, null);
                }
            mesh.RecalculateBounds();
            return mesh;
        }

        static void Normalize(float[] w)
        {
            float sum = 0f;
            foreach (var x in w) sum += x;
            if (sum <= 0f) return;
            for (int i = 0; i < w.Length; i++) w[i] /= sum;
        }

        /// <summary>Die vier stärksten Knochen, neu normiert.</summary>
        static BoneWeight Top4(float[] w)
        {
            var idx = new int[] { -1, -1, -1, -1 };
            var val = new float[4];
            for (int b = 0; b < w.Length; b++)
            {
                if (w[b] <= 0f) continue;
                for (int s = 0; s < 4; s++)
                {
                    if (idx[s] >= 0 && w[b] <= val[s]) continue;
                    for (int m = 3; m > s; m--)
                    {
                        idx[m] = idx[m - 1];
                        val[m] = val[m - 1];
                    }
                    idx[s] = b;
                    val[s] = w[b];
                    break;
                }
            }
            float sum = val[0] + val[1] + val[2] + val[3];
            if (sum <= 0f) sum = 1f;
            return new BoneWeight
            {
                boneIndex0 = Mathf.Max(0, idx[0]), weight0 = val[0] / sum,
                boneIndex1 = Mathf.Max(0, idx[1]), weight1 = val[1] / sum,
                boneIndex2 = Mathf.Max(0, idx[2]), weight2 = val[2] / sum,
                boneIndex3 = Mathf.Max(0, idx[3]), weight3 = val[3] / sum
            };
        }

        /// <summary>Speichert das gehäutete Mesh neu (altes Asset wird ersetzt, da CopyInto keine Gewichte überträgt).</summary>
        public static Mesh Save(Mesh mesh, string path)
        {
            BlockoutKit.EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
            if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        /// <summary>Haut-Renderer an den Knochen; Bounds großzügig für Aufblähen und Wachsen.</summary>
        public static SkinnedMeshRenderer Renderer(Transform parent, string name, Mesh mesh, Transform[] bones, Transform rootBone, Material mat, bool shadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<SkinnedMeshRenderer>();
            r.sharedMesh = mesh;
            r.bones = bones;
            r.rootBone = rootBone;
            r.sharedMaterial = mat;
            r.quality = SkinQuality.Bone4;
            r.updateWhenOffscreen = false;
            r.skinnedMotionVectors = false;
            r.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            var b = mesh.bounds;
            r.localBounds = new Bounds(rootBone.InverseTransformPoint(b.center), b.size * 1.7f);
            return r;
        }
    }
}
