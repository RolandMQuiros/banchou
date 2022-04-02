using UnityEngine;
using UnityEngine.SceneManagement;

namespace Banchou.DependencyInjection {
    public class SceneInjector : MonoBehaviour {
        private void Awake() {
            transform.ApplyBindings();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            var container = transform.FindContexts().ToDiContainer();
            foreach (var root in scene.GetRootGameObjects()) {
                if (root != gameObject) {
                    root.transform.ApplyBindings(container);
                }
            }
        }
    }
}