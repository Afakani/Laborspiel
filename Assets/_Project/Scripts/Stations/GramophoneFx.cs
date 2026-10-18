using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Grammophon sichtbar (Prototyp gramDisc/gramCrank): Platte und Kurbel drehen sich, Noten steigen aus dem Horn.</summary>
    public class GramophoneFx : MonoBehaviour
    {
        [SerializeField] Transform record;
        [SerializeField] Transform crank;
        [SerializeField] ParticleSystem notes;

        public void Setup(Transform recordT, Transform crankT, ParticleSystem notesFx)
        {
            record = recordT;
            crank = crankT;
            notes = notesFx;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var fx = gm.Config.machineFx;
            float dt = gm.Clock.DeltaTime;
            bool music = StationLogic.MusicPlaying(gm.State);
            if (music)
            {
                record.Rotate(0f, dt * fx.recordRadPerSec * Mathf.Rad2Deg, 0f, Space.Self);
                crank.Rotate(dt * fx.crankRadPerSec * Mathf.Rad2Deg, 0f, 0f, Space.Self);
            }
            var em = notes.emission;
            em.rateOverTime = music && dt > 0f ? fx.notesPerSec : 0f;
        }
    }
}
