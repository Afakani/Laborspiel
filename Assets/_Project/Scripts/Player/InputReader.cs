using UnityEngine;
using UnityEngine.InputSystem;

namespace PlanErfuellt
{
    /// <summary>
    /// Liest die projektweiten Input Actions (Settings/PlanErfuellt_Input, Maps „Player“ und „Build“).
    /// Einziger Ort, an dem Aktionsnamen stehen.
    /// </summary>
    public static class InputReader
    {
        static InputAction move, turn, look, interact, pause, antidote, logbook, build, mute, confirm, capture;
        static InputAction point, place, deselect, sell;
        static readonly InputAction[] tools = new InputAction[4];
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
            point = a.FindAction("Build/Point", true);
            place = a.FindAction("Build/Place", true);
            deselect = a.FindAction("Build/Deselect", true);
            sell = a.FindAction("Build/Sell", true);
            for (int i = 0; i < tools.Length; i++) tools[i] = a.FindAction("Build/Tool" + (i + 1), true);
            a.FindActionMap("Build", true).Enable();
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

        // ---------- Baumodus ----------
        /// <summary>Mausposition in Bildschirmpixeln.</summary>
        public static Vector2 Point { get { Init(); return ready ? point.ReadValue<Vector2>() : Vector2.zero; } }
        public static bool PlacePressed { get { Init(); return ready && place.WasPressedThisFrame(); } }
        public static bool DeselectPressed { get { Init(); return ready && deselect.WasPressedThisFrame(); } }
        public static bool SellPressed { get { Init(); return ready && sell.WasPressedThisFrame(); } }
        /// <summary>Werkzeug-Taste 1..4 in diesem Frame gedrückt? Liefert den Index oder −1.</summary>
        public static int ToolPressed
        {
            get
            {
                Init();
                if (!ready) return -1;
                for (int i = 0; i < tools.Length; i++)
                    if (tools[i].WasPressedThisFrame()) return i;
                return -1;
            }
        }
    }
}
