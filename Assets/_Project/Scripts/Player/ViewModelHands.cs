using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Ego-Hände mit Spritze (Prototyp animVM): wippen beim Laufen, atmen im Stand, beugen sich beim Halten
    /// einer Aktion vor und stoßen beim Spritzen nach vorn (Thrust, ab Phase 5 für das Gegenmittel).
    /// </summary>
    public class ViewModelHands : MonoBehaviour
    {
        [SerializeField] FirstPersonController controller;
        [SerializeField] InteractionSystem interaction;
        [SerializeField] Transform rightHand;
        [SerializeField] Transform leftHand;
        [SerializeField] Transform plunger;

        Vector3 rightBase, leftBase, plungerBase;
        Vector3 rightEulerBase;
        float hold, thrust;

        /// <summary>Spritzbewegung auslösen (Dauer in Sekunden).</summary>
        public void Thrust(float seconds) => thrust = Mathf.Max(thrust, seconds);

        void Start()
        {
            rightBase = rightHand.localPosition;
            leftBase = leftHand.localPosition;
            plungerBase = plunger.localPosition;
            rightEulerBase = rightHand.localEulerAngles;
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var h = gm.Config.player.hands;
            float dt = gm.Clock.DeltaTime;
            float t = gm.Clock.Time;
            float k = Mathf.Min(1f, dt * h.smoothing);

            thrust = Mathf.Max(0f, thrust - dt);
            float th = thrust > 0f ? 1f : 0f;
            hold = interaction.IsHoldingActive ? interaction.Progress : 0f;

            bool moving = controller.IsMoving;
            float phase = controller.BobPhase;
            float bob = moving ? Mathf.Sin(phase) * h.bob : 0f;
            float sway = moving ? Mathf.Cos(phase * 0.5f) * h.sway : 0f;

            // z ist in Unity nach vorn positiv, im Prototyp negativ
            var rTarget = rightBase + new Vector3(-hold * h.holdX + sway, hold * h.holdY + bob + Mathf.Sin(t * 1.4f) * h.idleAmp, th * h.thrustZ + hold * h.holdZ);
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rTarget, k);
            var rEuler = rightEulerBase + new Vector3(-(hold * h.holdTiltDeg - th * h.thrustTiltDeg), 0f, 0f);
            rightHand.localRotation = Quaternion.Slerp(rightHand.localRotation, Quaternion.Euler(rEuler), k);

            var lTarget = leftBase + new Vector3(-sway, -bob + Mathf.Sin(t * 1.2f + 1f) * h.idleAmp, 0f);
            leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, lTarget, k);

            plunger.localPosition = Vector3.Lerp(plunger.localPosition, plungerBase + new Vector3(0f, 0f, th * h.thrustZ * 0.27f), k);
        }
    }
}
