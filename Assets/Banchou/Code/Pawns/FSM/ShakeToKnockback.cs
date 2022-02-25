using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ShakeToKnockback : FSMBehaviour {
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _multiplier = 1f;
        [SerializeField] private float _maximumOffset = 2f;
        
        private GetDeltaTime _getDeltaTime;
        private Vector3 _targetPosition;

        private float _hitPauseTime;
        private float _timeElapsed;
        private Vector3 _knockback;

        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            _getDeltaTime = state.PawnDeltaTime(pawnId);
            state.ObserveHitsOn(pawnId)
                .CatchIgnoreLog()
                .Subscribe(hit => {
                    _hitPauseTime = hit.PauseTime;
                    _knockback = hit.Knockback;
                    _timeElapsed = 0f;
                })
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            _timeElapsed += _getDeltaTime();
            
            var stateTime = Mathf.Clamp01(_timeElapsed / _hitPauseTime);
            var magnitude = _multiplier * Mathf.Clamp01(_knockback.magnitude / _maximumOffset) *
                            _curve.Evaluate(stateTime);
            _targetPosition = magnitude * _knockback.normalized;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            _targetPosition = Vector3.zero;
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.bodyPosition += _targetPosition;
        }
    }
}