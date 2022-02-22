using Banchou.Pawn.Part;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class TogglePawnCollider : FSMBehaviour {
        private enum ApplyEvent { OnEnter, OnStateTime, OnExit }
        
        [SerializeField] private ApplyEvent _onEvent;
        [SerializeField] private bool _enableCollider;
        [SerializeField, Range(0f, 1f)] private float _stateTime;

        private Collider _collider;
        private bool _applied;

        public void Construct(GetPawnCollider getPawnCollider) {
            _collider = getPawnCollider();
        }

        private void Apply() {
            if (!_applied) {
                _collider.gameObject.SetActive(_enableCollider);
                _applied = true;
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _applied = false;
            
            if (_onEvent == ApplyEvent.OnEnter) {
                Apply();
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            if (_onEvent == ApplyEvent.OnStateTime && stateInfo.normalizedTime >= _stateTime) {
                Apply();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            if (_onEvent == ApplyEvent.OnExit) {
                Apply();
            }
        }
    }
}