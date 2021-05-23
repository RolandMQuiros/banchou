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

            for (int c = 0; c < contacts.Count; c++) {
                var contact = contacts[c];
                var vecDotContact = Vector3.Dot(projected, contact);

                var isFloor = Vector3.Dot(contact, up) > 0.6f;
                var movingIntoContact = vecDotContact < 0f;

                // If we're moving into a surface, we want to project the movement direction on it, so we don't cause physics jitters from
                // overlaps
                if (isFloor) {
                    projected = Vector3.Normalize(Vector3.ProjectOnPlane(projected, contact)) * projected.magnitude;
                } else if (movingIntoContact) {
                    // If the surface is a wall, and we're moving into it, move along it instead
                    projected = Vector3.ProjectOnPlane(projected, contact);
                }
            }

            return projected;
        }

        public static int DirectionSector(this Vector2 vec, float sectorAngle, float start) {
            var sectorCount = Mathf.Ceil(2f * Mathf.PI / sectorAngle);
            var sector = -1;
            if (vec != Vector2.zero) {
                var vectorAngle = Mathf.Atan2(vec.y, vec.x);
                for (var a = start; a < 2f * Mathf.PI && a < vectorAngle; a += sectorAngle) {
                    sector++;
                }
            }
            return sector;
        }
    }
}