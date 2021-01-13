using UnityEngine;

namespace Banchou {
    public static class VectorExtensions {
        public static Vector3 CameraPlaneProject(this Vector2 input, Vector3 cameraForward, Vector3 cameraRight, Vector3 plane) {
            return Vector3.ProjectOnPlane(cameraForward, plane).normalized * input.y +
                Vector3.ProjectOnPlane(cameraRight, plane).normalized * input.x;
        }

        public static Vector3 CameraPlaneProject(this Vector2 input, Vector3 cameraForward, Vector3 cameraRight) {
            return input.CameraPlaneProject(cameraForward, cameraRight, Vector3.up);
        }

        public static Vector3 CameraPlaneProject(this Vector2 input, Transform transform) {
            return input.CameraPlaneProject(transform.forward, transform.right, Vector3.up);
        }
    }
}