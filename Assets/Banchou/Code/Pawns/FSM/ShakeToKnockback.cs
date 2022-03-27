using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ShakeToKnockback : FSMBehaviour {
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _multiplier = 1f;
        [SerializeField] private float _maximumOffset = 2f;
        
        private Vector3 _targetPosition;
        private GameState _state;
        private Vector3 _knockback;

        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            _state = state;
            _state.ObserveHitsOn(pawnId)
                .CatchIgnoreLog()
                .Subscribe(hit => { _knockback = hit.Knockback; })
                .AddTo(this);
            _state.ObserveNormalizedHitPause(pawnId, ObserveStateUpdate)
                .CatchIgnoreLog()
                .Subscribe(pauseTime => {
                    var magnitude = _multiplier * Mathf.Clamp01(_knockback.magnitude / _maximumOffset) *
                                    _curve.Evaluate(pauseTime);
                    _targetPosition = magnitude * _knockback.normalized;
                })
                .AddTo(this);
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