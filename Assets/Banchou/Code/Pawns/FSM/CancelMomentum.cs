using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class CancelMomentum : FSMBehaviour {
        private enum ApplyEvent { OnEnter, OnUpdate, OnExit }
        [SerializeField] private ApplyEvent _onEvent = ApplyEvent.OnEnter;
        private Rigidbody _rigidbody;
        
        public void Construct(Rigidbody rigidbody) {
            _rigidbody = rigidbody;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent == ApplyEvent.OnEnter) {
                _rigidbody.velocity = Vector3.zero;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent == ApplyEvent.OnUpdate) {
                _rigidbody.velocity = Vector3.zero;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent == ApplyEvent.OnExit) {
                _rigidbody.velocity = Vector3.zero;
            }
        }
    }
}