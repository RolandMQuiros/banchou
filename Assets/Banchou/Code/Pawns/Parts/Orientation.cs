using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Orientation : MonoBehaviour {
        private PawnSpatial _spatial;
        private Rigidbody _rigidbody;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Rigidbody body
        ) {
            state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            _rigidbody = body;
        }

        private void FixedUpdate() {
            _rigidbody.rotation = Quaternion.LookRotation(_spatial.Forward, _spatial.Up);
        }
    }
}