using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Krisen und Warnzeichen am Tier sichtbar machen (GDD 5.3, Prototyp emit-Aufrufe in updateSubject):
    /// Dampf bei drohender Überhitzung, Flammen bei Überhitzung, Blasen bei drohender Säure, Säurespritzer bei
    /// Säure-Milch, Schleim bei INSTABIL. Die Emitter folgen dem Tier.
    /// </summary>
    public class AnimalCrisisFx : MonoBehaviour
    {
        [SerializeField] Chamber chamber;
        [SerializeField] ParticleSystem steam;
        [SerializeField] ParticleSystem heatFire;
        [SerializeField] ParticleSystem bubbles;
        [SerializeField] ParticleSystem acidSpray;
        [SerializeField] ParticleSystem slime;
        [Header("Partikel pro Sekunde (Prototyp)")]
        [SerializeField] float steamRate = 14f;
        [SerializeField] float fireRate = 20f;
        [SerializeField] float bubbleRate = 10f;
        [SerializeField] float acidRate = 12f;
        [SerializeField] float slimeRate = 20f;

        public void Setup(Chamber c, ParticleSystem steamFx, ParticleSystem fireFx, ParticleSystem bubbleFx, ParticleSystem acidFx, ParticleSystem slimeFx)
        {
            chamber = c;
            steam = steamFx;
            heatFire = fireFx;
            bubbles = bubbleFx;
            acidSpray = acidFx;
            slime = slimeFx;
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var model = chamber.ModelTransform;
            bool alive = model != null && chamber.HasLivingAnimal && gm.Clock.DeltaTime > 0f;
            var s = chamber.Stats;
            var w = gm.Config.warnings;
            var r = gm.Config.risks;

            if (model != null) transform.position = model.position;
            Rate(steam, alive && s.heat > w.heat[0] && s.heat <= w.heat[1] ? steamRate : 0f);
            Rate(heatFire, alive && s.heat > r.heatCrisis ? fireRate : 0f);
            Rate(bubbles, alive && !s.acid && s.acidTimer > w.acidT ? bubbleRate : 0f);
            Rate(acidSpray, alive && s.acid ? acidRate : 0f);
            Rate(slime, alive && chamber.State == AnimalState.Unstable ? slimeRate : 0f);
        }

        static void Rate(ParticleSystem ps, float rate)
        {
            if (ps == null) return;
            var em = ps.emission;
            em.rateOverTime = rate;
            if (rate > 0f && !ps.isPlaying) ps.Play();
        }
    }
}
