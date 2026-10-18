using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlanErfuellt
{
    /// <summary>Lädt das Staats-TV-Studio als eigene Szene zusätzlich zum Labor (einmal beim Start).</summary>
    public class NewsStudioLoader : MonoBehaviour
    {
        [SerializeField] string sceneName = "NewsStudio";

        public void Setup(string scene) => sceneName = scene;

        void Start()
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
    }
}
