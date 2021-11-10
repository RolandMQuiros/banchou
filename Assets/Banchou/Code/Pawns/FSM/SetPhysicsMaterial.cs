using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetPhysicsMaterial : FSMBehaviour {
        [SerializeField] private PhysicMaterial _material;

        [SerializeField, Range(0f, 1f)] private float _fromTime = 0f;
        [SerializeField, Range(0f, 1f)] private float _untilTime = 1f;

        private Collider _worldCollider;
        private PhysicMaterial _defaultMaterial;
        private bool _applied;
        
        public void Construct(Collider worldCollider = null) {
            if (worldCollider != null) {
                _worldCollider = worldCollider;
                _defaultMaterial = worldCollider.sharedMaterial;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_worldCollider == null) return;
            
            var stateTime = stateInfo.normalizedTime % 1f;
            
            if (stateTime >= _fromTime && stateTime <= _untilTime && !_applied) {
                _worldCollider.material = _material;
                _applied = true;
            } else if (stateTime > _untilTime && _applied) {
                _worldCollider.sharedMaterial = _defaultMaterial;
                _applied = false;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_worldCollider != null) {
                _worldCollider.material = _defaultMaterial;
            }

            _applied = false;
        }
    }
}