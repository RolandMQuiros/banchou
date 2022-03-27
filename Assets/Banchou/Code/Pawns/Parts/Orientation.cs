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
            var pawnId = getPawnId();
            _rigidbody = body;
            state.ObservePawnSpatial(pawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            state.ObservePawnSpatialChanges(pawnId)
                .DistinctUntilChanged(spatial => spatial.IsSync)
                .Where(spatial => spatial.IsSync)
                .CatchIgnoreLog()
                .Subscribe(Apply)
                .AddTo(this);
        }

        private void Apply(PawnSpatial spatial) {
            _rigidbody.rotation = Quaternion.LookRotation(spatial.Forward, spatial.Up);
        }

        private void FixedUpdate() {
            Apply(_spatial);
        }
    }
}