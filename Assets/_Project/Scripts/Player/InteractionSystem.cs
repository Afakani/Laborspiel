using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Wählt das Ziel mit der kleinsten Wertung in Reichweite (wie Prototyp pickTarget) und führt
    /// die Aktion aus, wenn die Leertaste lange genug gehalten wurde. Danach muss man loslassen.
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        IInteractable current;
        string holdKey;
        float held;
        bool holding, consumed;

        /// <summary>Aktuelles Ziel oder null.</summary>
        public IInteractable Current => current;
        public string PromptText { get; private set; }
        public bool CanUse { get; private set; }
        /// <summary>Fortschritt des Haltebalkens 0..1.</summary>
        public float Progress { get; private set; }
        /// <summary>Wird gerade eine mögliche Aktion gehalten (für die Hände)?</summary>
        public bool IsHoldingActive => holding && !consumed && current != null && CanUse;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.PlayerControlEnabled)
            {
                Clear();
                holding = false;
                return;
            }

            var pos = transform.position;
            current = PickTarget(new Vector2(pos.x, -pos.z));
            if (current == null)
            {
                Clear();
            }
            else
            {
                current.GetPrompt(out var text, out var ok, out var hold);
                PromptText = text;
                CanUse = ok;

                // Halten wie im Prototyp (act.down): gilt bis zum Loslassen; nach einer Aktion erst wieder nach Loslassen
                if (InputReader.InteractHeld) holding = true;
                else
                {
                    holding = false;
                    consumed = false;
                }

                if (holding && ok && !consumed)
                {
                    if (holdKey != current.Key)
                    {
                        holdKey = current.Key;
                        held = 0f;
                    }
                    held += gm.Clock.DeltaTime;
                    if (held >= hold)
                    {
                        current.Execute();
                        consumed = true;
                        held = 0f;
                    }
                }
                else held = 0f;

                Progress = ok && holding && !consumed && hold > 0f ? Mathf.Clamp01(held / hold) : 0f;
            }
        }

        static IInteractable PickTarget(Vector2 p)
        {
            IInteractable best = null;
            float bestScore = float.MaxValue;
            var list = Interactable.All;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetScore(p, out var s) && s < bestScore)
                {
                    bestScore = s;
                    best = list[i];
                }
            }
            return best;
        }

        void Clear()
        {
            current = null;
            PromptText = null;
            CanUse = false;
            Progress = 0f;
            held = 0f;
        }
    }
}
