using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitStunToFloat : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit stun time")]
        private FloatFSMParameter _output;

        [SerializeField]
        private bool _normalized = true;

        private GameState _state;
        private float _hitPauseTime;
        private float _hitStunTime;
        private float _hitTotalTime;
        private float _whenHit;
        private float _timeScale;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            _state = state;
            if (_output.IsSet) {
                _state.ObserveHitsOn(pawnId)
                    .Where(_ => IsStateActive)
                    .CatchIgnoreLog()
                    .Subscribe(hit => {
                        _hitPauseTime = hit.PauseTime;
                        _hitStunTime = hit.StunTime;
                        _hitTotalTime = _hitPauseTime + _hitStunTime;
                        _whenHit = hit.LastUpdated;
                        animator.SetFloat(_output.Hash, 0f);
                    })
                    .AddTo(this);
                _state.ObservePawnTimeScale(pawnId)
                    .CatchIgnoreLog()
                    .Subscribe(timeScale => _timeScale = timeScale)
                    .AddTo(this);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            var timeElapsed = (_state.GetTime() - _whenHit) * _timeScale;
            if (timeElapsed < _hitTotalTime) {
                var stunTime = timeElapsed - _hitPauseTime;
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