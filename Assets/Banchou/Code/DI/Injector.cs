using UnityEngine;
using UnityEngine.SceneManagement;

namespace Banchou.DependencyInjection {
    public class GameObjectInjector : MonoBehaviour {
        private void Awake() {
            transform.ApplyBindings();
        }
    }
}