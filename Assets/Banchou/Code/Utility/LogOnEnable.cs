using UnityEngine;

namespace Banchou.Utility {
    public class LogOnEnable : MonoBehaviour {
        private void OnEnable() {
            Debug.Log($"{name} Enabled");
        }
    }
}