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

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => {
                    _spatial = spatial;
                })
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_rootPositionTarget == ApplyTarget.Spatial) {
                _spatial.Move(animator.deltaPosition, _state.GetTime());
            }
        
            if (_rootRotationTarget == ApplyTarget.Spatial) {
                _spatial.Rotate(
                    animator.deltaRotation * _spatial.Forward,
                    _state.GetTime()
                );
            }
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_rootPositionTarget == ApplyTarget.AnimatorBody) {
                animator.bodyPosition += animator.rootPosition;
            }

            if (_rootRotationTarget == ApplyTarget.AnimatorBody) {
                animator.bodyRotation *= animator.rootRotation;
            }
        }
    }
}