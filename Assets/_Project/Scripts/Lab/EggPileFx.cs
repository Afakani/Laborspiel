using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Sichtbare Eier (D161): Legt das Huhn ein Ei, geht es kurz in die Hocke, das Ei fällt hinten heraus, springt auf
    /// und bleibt auf dem Kammerboden liegen, bis die Schicht vorbei ist. Höchstens eggs.maxOnFloor Eier, darüber
    /// verschwindet das älteste. Goldene Eier beim Münzhuhn und beim Planerfüller Ω. Werte aus eggs.
    /// </summary>
    public class EggPileFx : MonoBehaviour
    {
        class Egg
        {
            public Transform t;
            public MeshRenderer r;
            public Vector3 v;
            public bool moving;
            public Quaternion rest;
            public Vector3 spinAxis;
            public float spin;
        }

        [SerializeField] Material normal;
        [SerializeField] Material gold;

        readonly List<Egg> eggs = new List<Egg>();
        int next, moving;
        bool any;
        Mesh mesh;
        Chamber chamber;

        public void Setup(Material normalMat, Material goldMat)
        {
            normal = normalMat;
            gold = goldMat;
        }

        void Awake()
        {
            mesh = BuildMesh();
            chamber = FindAnyObjectByType<Chamber>();
        }

        void OnEnable() => EventBus.Station += OnStation;
        void OnDisable() => EventBus.Station -= OnStation;

        void OnStation(string id)
        {
            if (id == Chamber.EggLaidEvent) Lay();
        }

        void Lay()
        {
            var gm = GameManager.Instance;
            var root = chamber != null ? chamber.ModelTransform : null;
            if (gm == null || root == null) return;
            var e = gm.Config.eggs;
            var vis = root.GetComponent<AnimalVisuals>();
            if (vis != null) vis.LayEgg(e);

            float s = root.lossyScale.x;
            var back = -root.right;
            back.y = 0f;
            back.Normalize();
            var egg = Take(e);
            egg.t.position = root.position + back * (e.backOffset * s) + Vector3.up * (e.dropHeight * s);
            egg.t.rotation = Random.rotation;
            var side = Random.insideUnitCircle * e.sideScatter;
            var dir = (back + new Vector3(side.x, 0f, side.y)).normalized;
            egg.v = dir * (e.scatterSpeed * Random.Range(0.5f, 1f)) + Vector3.up * e.upSpeed;
            egg.spinAxis = Random.onUnitSphere;
            egg.spin = Random.Range(e.spinDegMin, e.spinDegMax);
            // liegt auf der Seite (lange Achse waagerecht), Richtung zufällig
            egg.rest = Quaternion.Euler(0f, Random.Range(0f, 360f), 90f);
            var st = chamber.Stats;
            bool isGold = (st.fun != null && st.fun.coinsPerEgg > 0f) || (st.ultimate != null && st.ultimate.id == UltimateIds.Omega);
            egg.r.sharedMaterial = isGold ? gold : normal;
            float k = Random.Range(0.92f, 1.08f);
            egg.t.localScale = new Vector3(e.width, e.length, e.width) * k;
            if (!egg.moving) moving++;
            egg.moving = true;
            egg.t.gameObject.SetActive(true);
            any = true;
        }

        Egg Take(EggsConfig e)
        {
            if (eggs.Count < e.maxOnFloor)
            {
                var go = new GameObject("Egg" + eggs.Count);
                go.transform.SetParent(transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var egg = new Egg { t = go.transform, r = r };
                eggs.Add(egg);
                return egg;
            }
            var old = eggs[next];
            next = (next + 1) % eggs.Count;
            return old;
        }

        void Update()
        {
            if (!any) return;
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null) return;
            // Schicht vorbei: Boden wird gefegt
            if (dc.Mode != DayMode.Play && dc.Mode != DayMode.DayOver)
            {
                Clear();
                return;
            }
            if (moving == 0) return;
            float dt = gm.Clock.DeltaTime;
            if (dt <= 0f) return;

            var cfg = gm.Config;
            var e = cfg.eggs;
            var c = cfg.layout.chamber;
            var center = LayoutConfig.ToUnity(c.x, 0f, c.z);
            float floor = c.baseY + e.width * 0.5f;
            float maxR = c.radius - e.wallMargin;
            foreach (var egg in eggs)
            {
                if (!egg.moving) continue;
                var p = egg.t.position;
                egg.v.y -= e.gravity * dt;
                p += egg.v * dt;

                // Glaswand der Kammer: nach innen zurückprallen
                var flat = new Vector3(p.x - center.x, 0f, p.z - center.z);
                if (flat.magnitude > maxR)
                {
                    var n = flat.normalized;
                    p = new Vector3(center.x + n.x * maxR, p.y, center.z + n.z * maxR);
                    float vn = Vector3.Dot(egg.v, n);
                    if (vn > 0f) egg.v -= n * (vn * (1f + e.bounce));
                }

                bool onFloor = p.y <= floor;
                if (onFloor)
                {
                    p.y = floor;
                    if (egg.v.y < -e.settleSpeed)
                    {
                        egg.v.y = -egg.v.y * e.bounce;
                        egg.v.x *= e.bounceKeep;
                        egg.v.z *= e.bounceKeep;
                    }
                    else
                    {
                        egg.v.y = 0f;
                        float f = Mathf.Max(0f, 1f - e.rollFriction * dt);
                        egg.v.x *= f;
                        egg.v.z *= f;
                    }
                }
                egg.t.position = p;

                if (onFloor && egg.v.y == 0f)
                {
                    egg.t.rotation = Quaternion.Slerp(egg.t.rotation, egg.rest, Mathf.Min(1f, dt * e.restTurnSpeed));
                    if (new Vector2(egg.v.x, egg.v.z).sqrMagnitude < e.stopSpeed * e.stopSpeed && Quaternion.Angle(egg.t.rotation, egg.rest) < 2f)
                    {
                        egg.t.rotation = egg.rest;
                        egg.moving = false;
                        moving--;
                    }
                }
                else egg.t.Rotate(egg.spinAxis, egg.spin * dt, Space.World);
            }
        }

        void Clear()
        {
            foreach (var egg in eggs)
            {
                egg.moving = false;
                egg.t.gameObject.SetActive(false);
            }
            next = 0;
            moving = 0;
            any = false;
        }

        /// <summary>Eiform als Drehkörper um Y (Höhe 1, Breite 1): oben spitzer, unten runder.</summary>
        static Mesh BuildMesh()
        {
            const int rings = 12, segs = 16;
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int i = 0; i <= rings; i++)
            {
                float th = Mathf.PI * i / rings;
                float y = -Mathf.Cos(th) * 0.5f;
                float r = Mathf.Sin(th) * 0.5f * (1f - 0.14f * Mathf.Cos(th) * -1f);
                for (int j = 0; j <= segs; j++)
                {
                    float ph = 2f * Mathf.PI * j / segs;
                    verts.Add(new Vector3(Mathf.Cos(ph) * r, y, Mathf.Sin(ph) * r));
                }
            }
            for (int i = 0; i < rings; i++)
            for (int j = 0; j < segs; j++)
            {
                int a = i * (segs + 1) + j, b = a + segs + 1;
                tris.Add(a); tris.Add(b); tris.Add(a + 1);
                tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
            }
            var m = new Mesh { name = "Egg" };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }
}
