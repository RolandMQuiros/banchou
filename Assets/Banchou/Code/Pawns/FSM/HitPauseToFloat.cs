using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitPauseToFloat : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit pause time")]
        private FSMParameter _output = new(AnimatorControllerParameterType.Float);

        [SerializeField]
        private bool _normalized = true;

        private GetDeltaTime _getDeltaTime;
        private float _hitPauseTime;
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
                        _timeElapsed = 0f;
                        animator.SetFloat(_output.Hash, 0f);
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_timeElapsed < _hitPauseTime) {
                _timeElapsed += _getDeltaTime();
                if (_normalized) {
                    animator.SetFloat(_output.Hash, Mathf.Clamp01(_timeElapsed / _hitPauseTime));
                } else {
                    animator.SetFloat(_output.Hash, Mathf.Clamp(_timeElapsed, 0f, _hitPauseTime));
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