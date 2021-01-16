using UnityEngine;

namespace Banchou.DependencyInjection {
    public class TreeInjector : MonoBehaviour {
        private void Awake() {
            transform.ApplyBindings();
        }
    }
}