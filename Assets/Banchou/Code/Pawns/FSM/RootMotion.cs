using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RootMotion : FSMBehaviour {
        private enum ApplyTarget { None, Spatial, AnimatorBody }
        [SerializeField] private ApplyTarget _rootPositionTarget;
        [SerializeField] private ApplyTarget _rootRotationTarget;

        private GameState _state;
        private PawnSpatial _spatial;
        private Animator _animator;
        private Rigidbody _rigidbody;

        private Vector3? _positionOffset;
        private Quaternion? _rotationOffset;

        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody) {
            _state = state;
            _state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => {
                    _spatial = spatial;
                })
                .AddTo(this);
            _rigidbody = rigidbody;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _positionOffset = null;
            _rotationOffset = null;
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            /*
               Animator does not let us directly set rootPosition and rootRotation, so we're stuck with bodyPosition and
               bodyRotation. Both deltaPosition and deltaRotation are in root-world space, not world space, which means
               they're transformed to world space under the assumption they're being applied to a world transform.

               We don't necessarily want that. Sometimes we want to only apply root motion to the root bone so that it
               only affects the visuals and not the Pawn. That's what ApplyTarget.AnimatorBody is for.

               So we need to boil the deltas down to their local changes, accumulate those changes over time,
               then apply them to bodyPosition|Rotation.
            */

            var now = _state.GetTime();
            var rootPosition = animator.rootPosition;
            var rootRotation = animator.rootRotation;
            var inverseRootRotation = Quaternion.Inverse(rootRotation);

            var bodyRotation = animator.bodyRotation;
            var localBodyRotation = inverseRootRotation * bodyRotation;

            var deltaPosition = animator.deltaPosition;
            var deltaRotation = animator.deltaRotation;

            var localDeltaPosition = inverseRootRotation * deltaPosition;

            _rotationOffset ??= rootRotation;
            _rotationOffset *= deltaRotation;

            var worldPositionDelta = _rotationOffset.Value * localDeltaPosition;
            _positionOffset ??= rootPosition;
            _positionOffset += worldPositionDelta;

            switch (_rootRotationTarget) {
                case ApplyTarget.Spatial:
                    _spatial.Rotate(deltaRotation * _spatial.Forward, now);
                    break;
                case ApplyTarget.AnimatorBody:
                    animator.bodyRotation = _rotationOffset.Value * localBodyRotation;
                    break;
            }

            switch (_rootPositionTarget) {
                case ApplyTarget.Spatial:
                    _spatial.Move(worldPositionDelta, now);
                    break;
                case ApplyTarget.AnimatorBody:
                    animator.bodyPosition += _positionOffset.Value;
                    break;
            }
        }
    }
}