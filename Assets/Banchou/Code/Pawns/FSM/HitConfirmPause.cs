using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitConfirmPause : FSMBehaviour {
        [SerializeField, Tooltip("How fast the animator should run during hit pause"), Range(0.01f, 1f)]
        private float _animatorSpeed = 0.01f;

        private GameState _state;
        private AttackState _attack;
        private float _originalSpeed;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            _attack = _state.GetCombatantAttack(getPawnId());
            _originalSpeed = animator.speed;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            var pauseTime = _attack.NormalizedPauseTimeAt(_state.GetTime());
            if (pauseTime < 1f) {
                animator.speed = _animatorSpeed;
            } else {
                animator.speed = _originalSpeed;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.speed = _originalSpeed;
        }
    }
}