using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetPhysicsMaterialAtTime : FSMBehaviour {
        private enum ApplyEvent {
            OnEnter,
            AtNormalizedTime,
            OnExit
        }

        [SerializeField] private ApplyEvent _onEvent = ApplyEvent.AtNormalizedTime;
        [SerializeField] private PhysicMaterial _activeMaterial;
        [SerializeField, Min(0f)] private float _normalizedTime = 0f;
        [SerializeField] private string _normalizedTimeParameter;

        private Collider _worldCollider;
        private bool _applied;
        private int _inputHash;
        
        public void Construct(Collider worldCollider = null) {
            if (worldCollider != null) {
                _worldCollider = worldCollider;
            }
            _inputHash = Animator.StringToHash(_normalizedTimeParameter);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _applied = false;
            if (_onEvent == ApplyEvent.OnEnter && _worldCollider != null) {
                _worldCollider.material = _activeMaterial;
                _applied = false;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_worldCollider == null ||
                _onEvent != ApplyEvent.AtNormalizedTime) return;
            
            var stateTime = stateInfo.normalizedTime;
            if ( _inputHash != 0) {
                stateTime = animator.GetFloat(_inputHash);
            }
            
            if (stateTime >= _normalizedTime && !_applied) {
                _worldCollider.material = _activeMaterial;
                _applied = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent == ApplyEvent.OnExit && _worldCollider != null) {
                _worldCollider.material = _activeMaterial;
                _applied = true;
            }
        }
    }
}