using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Banchou.Utility {
    [RequireComponent(typeof(Volume))]
    public class DepthOfFieldFollow : MonoBehaviour {
        [SerializeField] private Transform _camera;
        [SerializeField] private Transform _focusTarget;
        
        private DepthOfField _depthOfField;
        private bool CanUpdate => _camera != null && _focusTarget != null &&
                                  _depthOfField.mode == DepthOfFieldMode.Gaussian;
        
        private void Awake() {
            if (TryGetComponent<Volume>(out var volume)) {
                volume.profile.TryGet(out _depthOfField);
            }

            if (_camera == null && Camera.main != null) {
                _camera = Camera.main.transform;
            }
        }

        private void Update() {
            if (CanUpdate) {
                var distance = Vector3.Dot(_focusTarget.position - _camera.position, _camera.forward);
                _depthOfField.gaussianEnd.value =
                    _depthOfField.gaussianEnd.value - _depthOfField.gaussianStart.value + distance;
                _depthOfField.gaussianStart.value = distance;
            }
        }
    }
}