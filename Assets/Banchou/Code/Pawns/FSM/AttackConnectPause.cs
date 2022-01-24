using Banchou.Combatant;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class AttackConnectPause : FSMBehaviour {
        [SerializeField, Tooltip("Whether or not to stop the rigid body's movement during the pause")]
        private bool _freezeOnHit = true;

        [SerializeField, Tooltip("Whether or not to restore momentum after freezing")]
        private bool _preserveMomentum = true;

        private GameState _state;
        private AttackState _attack;
        private float _originalSpeed;
        
        private Rigidbody _rigidbody;
        private RigidbodyConstraints _originalConstraints;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator, Rigidbody rigidbody) {
            _state = state;
            _attack = _state.GetCombatantAttack(getPawnId());
            _originalSpeed = animator.speed;
            _rigidbody = rigidbody;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _originalConstraints = _rigidbody.constraints;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            var pauseTime = _attack.NormalizedPauseTimeAt(_state.GetTime());
            
            if (pauseTime < 1f) {
                animator.speed = 0f;
                if (_freezeOnHit) {
                    if (!_preserveMomentum) {
                        _rigidbody.velocity = Vector3.zero;
                    }
                    // This is messing up on weird transitions
                    _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                }
            } else {
                animator.speed = _originalSpeed;
                _rigidbody.constraints = _originalConstraints;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.speed = _originalSpeed;
            _rigidbody.constraints = _originalConstraints;
        }
    }
}