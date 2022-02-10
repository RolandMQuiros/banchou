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

        private Vector3 _positionOffset;
        private Quaternion _rotationOffset;
        private Quaternion? _startingRootRotation;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => {
                    _spatial = spatial;
                })
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _positionOffset = Vector3.zero;
            _rotationOffset = Quaternion.identity;
            _startingRootRotation = null;
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            /* The problem:
               Animator does not let us directly set rootPosition and rootRotation, so we're stuck bodyPosition and
               bodyRotation. Both deltaPosition and deltaRotation are in root-world space, not world space. So we need
               to boil them down to their local changes, accumulate them, then apply them to bodyPosition|Rotation.
            */
            
            var now = _state.GetTime();
            var rootRotation = animator.rootRotation;
            var inverseRootRotation = Quaternion.Inverse(rootRotation);
            var deltaRotation = animator.deltaRotation;

            _startingRootRotation ??= animator.rootRotation;
            _rotationOffset *= deltaRotation;

            // Convert root space position delta to local space
            var rootSpaceDelta = inverseRootRotation * animator.deltaPosition;
            // Convert local space position delta to world space, accounting for accumulated rotation deltas 
            var worldSpaceDelta = rootRotation * _rotationOffset * rootSpaceDelta;
            _positionOffset += worldSpaceDelta;
            
            switch (_rootRotationTarget) {
                case ApplyTarget.Spatial:
                    _spatial.Rotate(deltaRotation * _spatial.Forward, now);
                    break;
                case ApplyTarget.AnimatorBody:
                    // Get body rotation in local space
                    var localSpaceBodyRotation = inverseRootRotation * animator.bodyRotation;
                    
                    // 
                    animator.bodyRotation = _startingRootRotation.Value * _rotationOffset * localSpaceBodyRotation;
                    break;
            }

            switch (_rootPositionTarget) {
                case ApplyTarget.Spatial:
                    _spatial.Move(worldSpaceDelta, now);
                    break;
                case ApplyTarget.AnimatorBody:
                    animator.bodyPosition += _positionOffset;
                    break;
            }
        }
    }
}