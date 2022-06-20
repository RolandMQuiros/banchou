using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ShakeToKnockback : PawnFSMBehaviour {
        [SerializeField] private float _multiplier = 1f;
        [SerializeField] private float _maximumOffset = 2f;
        [SerializeField] private float _shakesPerSecond = 30f;

        private Vector3 _targetPosition;
        private float Envelope(float t) => -Mathf.Sin(t * Mathf.PI / 2f) + 1;
        private float Shake(float duration, float t) => Envelope(t) *
                                                        Mathf.Sin(2f * Mathf.PI * _shakesPerSecond * t * duration);

        public void Construct(GameState state, GetPawnId getPawnId) {
            ConstructCommon(state, getPawnId);
            var knockback = Vector3.zero;
            var pauseTime = 0f;
            
            State.ObserveHitsOn(PawnId)
                .CatchIgnoreLog()
                .Subscribe(hit => {
                    knockback = hit.Knockback;
                    pauseTime = hit.PauseTime;
                })
                .AddTo(this);
            State.ObserveNormalizedHitPause(PawnId, ObserveStateUpdate)
                .CatchIgnoreLog()
                .Subscribe(normalizedTime => {
                    if (pauseTime > 0f) {
                        var magnitude = _multiplier * Mathf.Clamp01(knockback.magnitude / _maximumOffset) *
                                        Shake(pauseTime, normalizedTime);
                        _targetPosition = magnitude * knockback.normalized;
                    }
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