using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitPauseToFloat : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit pause time")]
        private FSMParameter _output = new(AnimatorControllerParameterType.Float);

        [SerializeField]
        private bool _normalized = true;

        private GameState _state;
        private float _hitPauseTime;
        private float _hitTime;
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
                        _hitTime = hit.LastUpdated;
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
            var timeElapsed = (_state.GetTime() - _hitTime) * _timeScale;
            if (timeElapsed < _hitPauseTime) {
                if (_normalized) {
                    animator.SetFloat(_output.Hash, Mathf.Clamp01(timeElapsed / _hitPauseTime));
                } else {
                    animator.SetFloat(_output.Hash, Mathf.Clamp(timeElapsed, 0f, _hitPauseTime));
                }
            } else {
                if (_normalized) {
                    animator.SetFloat(_output.Hash, 1f);
                } else {
                    animator.SetFloat(_output.Hash, _hitPauseTime);
                }
            }
        }
    }
}