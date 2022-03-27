using System;
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
        private float _hitPauseTime;
        private float _hitTime;
        private float _timeScale;
        private Vector3 _knockback;

        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            _state = state;
            _state.ObserveHitsOn(pawnId)
                .CatchIgnoreLog()
                .Subscribe(hit => {
                    _hitPauseTime = hit.PauseTime;
                    _knockback = hit.Knockback;
                    _hitTime = hit.LastUpdated;
                })
                .AddTo(this);
            _state.ObservePawnTimeScale(pawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            var stateTime = Mathf.Clamp01((_state.GetTime() - _hitTime) * _timeScale / _hitPauseTime);
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