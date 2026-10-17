using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt
{
    /// <summary>
    /// Zwei globale Post-Processing-Volumes für die Infektion (grüne Sicht, verschwommene Sicht).
    /// Liegt in der Szene; InfectionSystem blendet ihre Gewichte ein und aus.
    /// </summary>
    public class InfectionVolumes : MonoBehaviour
    {
        public static InfectionVolumes Instance { get; private set; }

        [SerializeField] Volume green;
        [SerializeField] Volume blur;

        public void Setup(Volume greenVolume, Volume blurVolume)
        {
            green = greenVolume;
            blur = blurVolume;
        }

        void OnEnable()
        {
            Instance = this;
            Set(0f, 0f);
        }

        void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        public void Set(float greenWeight, float blurWeight)
        {
            if (green != null) green.weight = greenWeight;
            if (blur != null) blur.weight = blurWeight;
        }
    }
}
