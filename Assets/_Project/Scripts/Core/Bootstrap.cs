using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlanErfuellt
{
    /// <summary>Boot-Szene: lädt die Laborszene. Später Platz für Hauptmenü und Einstellungen.</summary>
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] string labScene = "Lab";

        void Start()
        {
            SceneManager.LoadScene(labScene, LoadSceneMode.Single);
        }
    }
}
