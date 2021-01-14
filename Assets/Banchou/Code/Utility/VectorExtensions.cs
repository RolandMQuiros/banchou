using System.Collections.Generic;
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

        public static Vector3 ProjectOnContacts(this Vector3 vec, Vector3 up, IList<Vector3> contacts) {
            var projected = vec;

            foreach (var contact in contacts) {
                // If we're moving into a surface, we want to project the movement direction on it, so we don't cause physics jitters from
                // overlaps
                if (Vector3.Dot(contact, up) > 0.3f) {
                    // if (Vector3.Dot(velocity, contact) < 0f) {
                        // If surface is a floor, and we're moving into it, move along it at full movement speed
                        projected = Vector3.ProjectOnPlane(projected, contact).normalized * projected.magnitude;
                    // }
                    // If we're moving away from the surface, no need for projections
                } else if (Vector3.Dot(vec, contact) < 0f) {
                    // If the surface is a wall, and we're moving into it, move along it instead
                    projected = Vector3.ProjectOnPlane(projected, contact);
                }
            }

            return projected;
        }
    }
}