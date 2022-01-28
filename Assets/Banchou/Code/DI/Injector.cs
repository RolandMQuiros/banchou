using UnityEngine;

namespace Banchou.DependencyInjection {
    public class GameObjectInjector : MonoBehaviour {
        private void Awake() {
            transform.ApplyBindings();
        }
    }
}