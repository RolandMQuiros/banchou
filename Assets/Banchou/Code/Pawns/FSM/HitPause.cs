using Banchou.Combatant;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitPause : FSMBehaviour {
        [SerializeField, Tooltip("How fast the animator should run during hit pause"), Range(0.01f, 1f)]
        private float _animatorSpeed = 0.01f;

        [SerializeField, Tooltip("Whether or not to cancel momentum on hit confirm")]
        private bool _cancelMomentum = false;

        private GameState _state;
        private AttackState _attack;
        private float _originalSpeed;
        
        private Rigidbody _rigidbody;
        private Vector3? _originalVelocity;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator, Rigidbody rigidbody) {
            _state = state;
            _attack = _state.GetCombatantAttack(getPawnId());
            _originalSpeed = animator.speed;
            _rigidbody = rigidbody;
            _originalVelocity = null;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            var pauseTime = _attack.NormalizedPauseTimeAt(_state.GetTime());
            if (pauseTime < 1f) {
                animator.speed = _animatorSpeed;
                if (_cancelMomentum) {
                    _originalVelocity ??= _rigidbody.velocity;
                    _rigidbody.velocity = Vector3.zero;
                }
            } else {
                animator.speed = _originalSpeed;
                if (_originalVelocity != null) {
                    _rigidbody.velocity = _originalVelocity.Value;
                    _originalVelocity = null;
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.speed = _originalSpeed;
            _originalVelocity = null;
        }
    }
}