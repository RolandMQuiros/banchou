using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetPhysicsMaterialAtTime : FSMBehaviour {
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

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_worldCollider == null) return;
            
            var stateTime = stateInfo.normalizedTime;
            if (_inputHash != 0) {
                stateTime = animator.GetFloat(_inputHash);
            }
            
            if (stateTime >= _normalizedTime && !_applied) {
                _worldCollider.material = _activeMaterial;
                _applied = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _applied = false;
        }
    }
}