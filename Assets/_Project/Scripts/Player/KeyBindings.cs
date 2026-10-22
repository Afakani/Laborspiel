using System;
using UnityEngine.InputSystem;

namespace PlanErfuellt
{
    /// <summary>
    /// Tastenbelegung (auf Wunsch): findet die Tastatur-Bindung einer Aktion (bei Laufen den Teil up/down/left/right),
    /// zeigt sie an, belegt sie interaktiv neu (Esc bricht ab) und speichert alle Abweichungen in GameSettings.Bindings.
    /// </summary>
    public static class KeyBindings
    {
        static InputActionRebindingExtensions.RebindingOperation running;

        public static bool Rebinding => running != null;

        static InputAction Action(string path) => InputSystem.actions != null ? InputSystem.actions.FindAction(path) : null;

        /// <summary>Index der ersten Tastatur-Bindung (ohne Teil: keine zusammengesetzte; mit Teil: dieser Teil). −1 = keine.</summary>
        public static int BindingIndex(InputAction a, string part)
        {
            if (a == null) return -1;
            for (int i = 0; i < a.bindings.Count; i++)
            {
                var b = a.bindings[i];
                if (!b.path.StartsWith("<Keyboard>")) continue;
                bool wanted = string.IsNullOrEmpty(part) ? !b.isPartOfComposite : b.isPartOfComposite && b.name == part;
                if (wanted) return i;
            }
            return -1;
        }

        /// <summary>Anzeigename der Taste, z. B. „F“.</summary>
        public static string Display(string action, string part = null)
        {
            var a = Action(action);
            int i = BindingIndex(a, part);
            return i < 0 ? "?" : a.GetBindingDisplayString(i);
        }

        /// <summary>Neue Taste abfragen. done(true) = belegt, done(false) = abgebrochen.</summary>
        public static void Rebind(string action, string part, Action<bool> done)
        {
            var a = Action(action);
            int i = BindingIndex(a, part);
            if (a == null || i < 0 || running != null)
            {
                done?.Invoke(false);
                return;
            }
            var map = a.actionMap;
            map.Disable();
            running = a.PerformInteractiveRebinding(i)
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op => Finish(map, op, true, done))
                .OnCancel(op => Finish(map, op, false, done))
                .Start();
        }

        static void Finish(InputActionMap map, InputActionRebindingExtensions.RebindingOperation op, bool ok, Action<bool> done)
        {
            op.Dispose();
            running = null;
            map.Enable();
            if (ok) Save();
            done?.Invoke(ok);
        }

        /// <summary>Alle Tasten auf Standard.</summary>
        public static void ResetAll()
        {
            if (InputSystem.actions == null) return;
            InputSystem.actions.RemoveAllBindingOverrides();
            Save();
        }

        static void Save() => GameSettings.Bindings = InputSystem.actions.SaveBindingOverridesAsJson();
    }
}
