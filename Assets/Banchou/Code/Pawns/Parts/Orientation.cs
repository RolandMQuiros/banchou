using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Orientation : MonoBehaviour {
        private PawnSpatial _spatial;
        private Rigidbody _rigidbody;

        public void Construct(
            PawnState pawn,
            Rigidbody rigidbody
        ) {
            _spatial = pawn.Spatial;
            _rigidbody = rigidbody;
        }

        private void FixedUpdate() {
            _rigidbody.rotation = Quaternion.LookRotation(_spatial.Forward, _spatial.Up);
        }
    }
}