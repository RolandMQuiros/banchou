using UnityEngine;

namespace Banchou.Pawn.Part {
    public class CameraTarget : MonoBehaviour {
        [field: SerializeField] public float Radius { get; private set; } = 1f;
        [field: SerializeField] public float Weight { get; private set; } = 1f;
    }
}