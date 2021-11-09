using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RotateToVelocity : FSMBehaviour {
        [SerializeField] private float _rotationSpeed = 100f;
        
        private PawnSpatial _spatial;

        public void Construct(GameState state, GetPawnId getPawnId) {
            state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_spatial.AmbientVelocity != Vector3.zero) {
                animator.transform.rotation = Quaternion.LookRotation(_spatial.AmbientVelocity.normalized);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_spatial.AmbientVelocity != Vector3.zero) {
                animator.transform.rotation = Quaternion.RotateTowards(
                    animator.transform.rotation,
                    Quaternion.LookRotation(_spatial.AmbientVelocity.normalized),
                    _rotationSpeed
                );
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.transform.localRotation = Quaternion.identity;
        }
    }
}