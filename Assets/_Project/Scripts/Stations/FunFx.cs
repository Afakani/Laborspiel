using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kuriositäten sichtbar (GDD 4.6, Prototyp emit): Noten über der Singenden Kuh, Milchtropfen beim Milchhuhn,
    /// eine Münze pro Ei beim Münzhuhn.
    /// </summary>
    public class FunFx : MonoBehaviour
    {
        [SerializeField] ParticleSystem notes;
        [SerializeField] ParticleSystem milk;
        [SerializeField] ParticleSystem coins;
        [SerializeField] Chamber chamber;
        [SerializeField] float notesHeight = 2.4f;
        [SerializeField] float dropHeight = 0.6f;

        public void Setup(Chamber c, ParticleSystem notesFx, ParticleSystem milkFx, ParticleSystem coinFx)
        {
            chamber = c;
            notes = notesFx;
            milk = milkFx;
            coins = coinFx;
        }

        void OnEnable() => EventBus.Station += OnStation;
        void OnDisable() => EventBus.Station -= OnStation;

        void OnStation(string id)
        {
            if (id != "coinEgg") return;
            Follow(coins, dropHeight);
            coins.Emit(1);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var fx = gm.Config.machineFx;
            var v = chamber.View;
            bool alive = v.ok && gm.Clock.DeltaTime > 0f;
            bool sing = alive && v.Singing;
            bool milkHen = alive && v.funId == UltimateIds.MilkHen;
            Follow(notes, notesHeight);
            Follow(milk, dropHeight);
            var en = notes.emission;
            en.rateOverTime = sing ? fx.singNotesPerSec : 0f;
            var em = milk.emission;
            em.rateOverTime = milkHen ? fx.milkDropsPerRate * chamber.Stats.currentRate : 0f;
        }

        /// <summary>Emitter über das Tier setzen.</summary>
        void Follow(ParticleSystem ps, float height)
        {
            var t = chamber.ModelTransform;
            if (t == null) return;
            ps.transform.position = t.position + Vector3.up * (height + chamber.Stats.floatHeight);
        }
    }
}
