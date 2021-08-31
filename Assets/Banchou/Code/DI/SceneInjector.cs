using UnityEngine;
using UnityEngine.SceneManagement;

namespace Banchou.DependencyInjection {
    public class SceneInjector : MonoBehaviour {
        private DiContainer _container;
        
        private void Awake() {
            _container = new DiContainer();
            foreach (var context in transform.FindContexts()) {
                context.InstallBindings(_container);
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            foreach (var root in scene.GetRootGameObjects()) {
                root.transform.ApplyBindings(_container);
            }
        }
    }
}