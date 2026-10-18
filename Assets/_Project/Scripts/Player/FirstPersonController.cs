using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Ego-Steuerung wie im Prototyp (updatePlay): WASD laufen, Maus (gefangen) oder Q/E/Pfeile drehen,
    /// Kopfwippen und leichtes Neigen. Kollision über CharacterController (Kammer, Stationen),
    /// zusätzlich auf den Spielerbereich aus layout.playerArea begrenzt.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] Transform cameraRig;
        [SerializeField] Camera cam;

        CharacterController cc;
        PlayerConfig P;
        AreaLayout area;
        ChamberLayout chamber;
        float eyeHeight;
        float yaw, pitch;
        float bobPhase, bobAmp, roll, sideSpeed;
        float lastAspect = -1f;

        /// <summary>Faktor auf die Laufgeschwindigkeit (Infektion: schnell/bleischwer).</summary>
        public float SpeedMultiplier { get; set; } = 1f;
        /// <summary>Steuerung invertiert (Infektion).</summary>
        public bool InvertControls { get; set; }
        /// <summary>Schwanken der Kamera in Grad (Infektion „Der Boden schwankt“) und Frequenz.</summary>
        public float WobbleDeg { get; set; }
        public float WobbleFreq { get; set; } = 1f;
        /// <summary>Zusätzlicher Kameraversatz (Kamerawackeln bei Explosion, Bandriss …).</summary>
        public Vector3 ShakeOffset { get; set; }
        /// <summary>Läuft der Spieler gerade (für Leerlauf-Erkennung und Hände)?</summary>
        public bool IsMoving { get; private set; }
        public float BobPhase => bobPhase;
        public Camera Camera => cam;

        void Awake() => cc = GetComponent<CharacterController>();

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) { enabled = false; return; }
            P = gm.Config.player;
            area = gm.Config.layout.playerArea;
            chamber = gm.Config.layout.chamber;
            var start = gm.Config.layout.playerStart;
            eyeHeight = start.eyeHeight;
            Teleport(LayoutConfig.ToUnity(start.x, 0f, start.z), 0f, 0f);
        }

        /// <summary>Spieler setzen (Spielstart, Debug).</summary>
        public void Teleport(Vector3 pos, float yawDeg, float pitchDeg)
        {
            cc.enabled = false;
            transform.position = pos;
            cc.enabled = true;
            yaw = yawDeg;
            pitch = pitchDeg;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            float dt = gm.Clock.DeltaTime;
            bool control = gm.PlayerControlEnabled;

            UpdateCursor(control);
            if (!control || dt <= 0f)
            {
                IsMoving = false;
                sideSpeed = 0f;
                return;
            }

            float inv = InvertControls ? -1f : 1f;
            // Sprinten mit Shift (eigener Faktor aus player.sprintMultiplier), zusätzlich zu Infektionseffekten
            float mf = SpeedMultiplier * (InputReader.SprintHeld ? P.sprintMultiplier : 1f);

            // Drehen: Tasten (zeitbasiert) und Maus (pixelbasiert)
            yaw += InputReader.Turn * P.turnSpeedDeg * dt * inv * (mf > 1f ? P.fastTurnBoost : 1f);
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                var d = InputReader.LookDelta;
                yaw += d.x * P.mouseSensitivityDeg * GameSettings.Mouse * inv;
                pitch = Mathf.Clamp(pitch - d.y * P.mouseSensitivityDeg * GameSettings.Mouse, -P.lookUpMaxDeg, P.lookDownMaxDeg);
            }
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            // Laufen
            var m = InputReader.Move;
            m.x *= inv;
            if (m.sqrMagnitude > 1f) m.Normalize();
            var vel = (transform.forward * m.y + transform.right * m.x) * (P.walkSpeed * mf);
            cc.Move(vel * dt);
            ClampToArea();
            IsMoving = vel.sqrMagnitude > 0.01f;
            sideSpeed = m.x * P.walkSpeed * mf;
            if (IsMoving) bobPhase += dt * P.bob.phaseSpeed * mf;
        }

        void LateUpdate()
        {
            if (P == null) return;
            var gm = GameManager.Instance;
            float dt = gm.Clock.DeltaTime;
            float t = gm.Clock.Time;
            var b = P.bob;

            bobAmp = Mathf.Lerp(bobAmp, IsMoving ? 1f : 0f, Mathf.Min(1f, dt * b.fadeSpeed));
            roll = Mathf.Lerp(roll, -sideSpeed * b.rollPerSpeedDeg, Mathf.Min(1f, dt * b.rollSmoothing));
            cameraRig.localPosition = new Vector3(
                Mathf.Cos(bobPhase * 0.5f) * b.ampX * bobAmp,
                eyeHeight + Mathf.Sin(bobPhase) * b.ampY * bobAmp + Mathf.Sin(t * b.breatheFreq) * b.breatheAmp,
                0f) + ShakeOffset;
            float wobble = WobbleDeg * Mathf.Sin(t * WobbleFreq * Mathf.PI * 2f);
            cameraRig.localRotation = Quaternion.Euler(pitch + wobble * 0.3f, 0f, roll + wobble);

            // Sichtfeld wie im Prototyp: bei schmalen Fenstern weiter, höchstens fovMax
            if (!Mathf.Approximately(cam.aspect, lastAspect))
            {
                lastAspect = cam.aspect;
                cam.fieldOfView = Mathf.Min(P.fovMax, P.fov * Mathf.Max(1f, P.fovAspectRef / cam.aspect));
            }
        }

        void ClampToArea()
        {
            var p = transform.position;
            // Spielerbereich in Prototyp-Koordinaten: z_unity = -z_proto
            float x = Mathf.Clamp(p.x, area.xMin, area.xMax);
            float z = Mathf.Clamp(p.z, -area.zMax, -area.zMin);

            // Kammer: Mindestabstand zur Mitte wie im Prototyp (collide), genauer als der Kapsel-Kollider allein
            var c = chamber;
            var center = LayoutConfig.ToUnity(c.x, 0f, c.z);
            var off = new Vector2(x - center.x, z - center.z);
            float d = off.magnitude;
            if (d < c.playerCollisionRadius && d > 0.001f)
            {
                off *= c.playerCollisionRadius / d;
                x = center.x + off.x;
                z = center.z + off.y;
            }
            if (!Mathf.Approximately(x, p.x) || !Mathf.Approximately(z, p.z) || Mathf.Abs(p.y) > 0.001f)
            {
                cc.enabled = false;
                transform.position = new Vector3(x, 0f, z);
                cc.enabled = true;
            }
        }

        static void UpdateCursor(bool control)
        {
            if (!control)
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                return;
            }
            if (Cursor.lockState != CursorLockMode.Locked && InputReader.CapturePressed)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
