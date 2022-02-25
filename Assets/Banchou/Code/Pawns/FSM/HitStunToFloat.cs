using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitStunToFloat : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit stun time")]
        private FSMParameter _output = new(AnimatorControllerParameterType.Float);

        [SerializeField]
        private bool _normalized = true;

        private GetDeltaTime _getDeltaTime;
        private float _hitPauseTime;
        private float _hitStunTime;
        private float _hitTotalTime;
        private float _timeElapsed;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            _getDeltaTime = state.PawnDeltaTime(pawnId);
            if (_output.IsSet) {
                state.ObserveHitsOn(pawnId)
                    .Where(_ => IsStateActive)
                    .CatchIgnoreLog()
                    .Subscribe(hit => {
                        _hitPauseTime = hit.PauseTime;
                        _hitStunTime = hit.StunTime;
                        _hitTotalTime = _hitPauseTime + _hitStunTime;
                        _timeElapsed = 0f;
                        animator.SetFloat(_output.Hash, 0f);
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_timeElapsed < _hitTotalTime) {
                _timeElapsed += _getDeltaTime();
                
                var stunTime = _timeElapsed - _hitPauseTime;
                if (stunTime < 0f) {
                    animator.SetFloat(_output.Hash, 0f);
                } else if (_normalized) {
                    animator.SetFloat(_output.Hash, Mathf.Clamp01(stunTime / _hitStunTime));
                } else {
                    animator.SetFloat(_output.Hash, Mathf.Clamp(stunTime, 0f, _hitStunTime));
                }
            } else {
                if (_normalized) {
                    animator.SetFloat(_output.Hash, 1f);
                } else {
                    animator.SetFloat(_output.Hash, _hitStunTime);
                }
            }
        }
    }
}