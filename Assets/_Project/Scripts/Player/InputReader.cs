using UnityEngine;
using UnityEngine.InputSystem;

namespace PlanErfuellt
{
    /// <summary>
    /// Liest die projektweiten Input Actions (Settings/PlanErfuellt_Input, Map „Player“).
    /// Einziger Ort, an dem Aktionsnamen stehen.
    /// </summary>
    public static class InputReader
    {
        static InputAction move, turn, look, interact, pause, antidote, logbook, build, mute, confirm, capture;
        static bool ready;

        static void Init()
        {
            if (ready) return;
            var a = InputSystem.actions;
            if (a == null)
            {
                Debug.LogError("InputReader: Keine projektweiten Input Actions gesetzt.");
                return;
            }
            move = a.FindAction("Player/Move", true);
            turn = a.FindAction("Player/Turn", true);
            look = a.FindAction("Player/Look", true);
            interact = a.FindAction("Player/Interact", true);
            pause = a.FindAction("Player/Pause", true);
            antidote = a.FindAction("Player/Antidote", true);
            logbook = a.FindAction("Player/Logbook", true);
            build = a.FindAction("Player/Build", true);
            mute = a.FindAction("Player/Mute", true);
            confirm = a.FindAction("Player/Confirm", true);
            capture = a.FindAction("Player/CaptureMouse", true);
            a.FindActionMap("Player", true).Enable();
            ready = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic()
        {
            ready = false;
            TestMove = null;
            TestInteract = null;
        }

        /// <summary>Nur für automatisierte Tests: ersetzt die Laufeingabe, solange gesetzt.</summary>
        public static Vector2? TestMove { get; set; }
        /// <summary>Nur für automatisierte Tests: ersetzt „Leertaste gehalten“, solange gesetzt.</summary>
        public static bool? TestInteract { get; set; }

        /// <summary>x = seitwärts, y = vorwärts.</summary>
        public static Vector2 Move { get { if (TestMove.HasValue) return TestMove.Value; Init(); return ready ? move.ReadValue<Vector2>() : Vector2.zero; } }
        /// <summary>-1 = links drehen (Q), +1 = rechts (E).</summary>
        public static float Turn { get { Init(); return ready ? turn.ReadValue<float>() : 0f; } }
        /// <summary>Mausbewegung in Pixeln seit dem letzten Frame.</summary>
        public static Vector2 LookDelta { get { Init(); return ready ? look.ReadValue<Vector2>() : Vector2.zero; } }

        public static bool InteractHeld { get { if (TestInteract.HasValue) return TestInteract.Value; Init(); return ready && interact.IsPressed(); } }
        public static bool InteractPressed { get { Init(); return ready && interact.WasPressedThisFrame(); } }
        public static bool PausePressed { get { Init(); return ready && pause.WasPressedThisFrame(); } }
        public static bool AntidotePressed { get { Init(); return ready && antidote.WasPressedThisFrame(); } }
        public static bool LogbookPressed { get { Init(); return ready && logbook.WasPressedThisFrame(); } }
        public static bool BuildPressed { get { Init(); return ready && build.WasPressedThisFrame(); } }
        public static bool MutePressed { get { Init(); return ready && mute.WasPressedThisFrame(); } }
        public static bool ConfirmPressed { get { Init(); return ready && confirm.WasPressedThisFrame(); } }
        public static bool CapturePressed { get { Init(); return ready && capture.WasPressedThisFrame(); } }
    }
}
