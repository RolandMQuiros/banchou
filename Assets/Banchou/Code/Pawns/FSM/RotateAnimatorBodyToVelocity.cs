using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RotateAnimatorBodyToVelocity : FSMBehaviour {
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private Vector3 _offsetAngles;
        
        private PawnSpatial _spatial;
        private Quaternion _targetRotation;
        private Quaternion _offsetRotation;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _spatial = state.GetPawnSpatial(getPawnId());
            _offsetRotation = Quaternion.Euler(_offsetAngles);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_spatial.AmbientVelocity != Vector3.zero) {
                _targetRotation = Quaternion.LookRotation(_spatial.AmbientVelocity.normalized);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_spatial.AmbientVelocity != Vector3.zero) {
                _targetRotation = Quaternion.RotateTowards(
                    _targetRotation,
                    Quaternion.LookRotation(_spatial.AmbientVelocity.normalized),
                    _rotationSpeed
                );
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            _targetRotation = Quaternion.identity;
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.bodyRotation = _targetRotation * _offsetRotation * animator.bodyRotation;
        }
    }
}