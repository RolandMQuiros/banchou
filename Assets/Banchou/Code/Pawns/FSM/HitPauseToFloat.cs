using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitPauseToFloat : PawnFSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit pause time")]
        private FSMParameter _output = new(AnimatorControllerParameterType.Float);

        [SerializeField]
        private bool _normalized = true;
        
        private float _hitPauseTime;
        private float _hitTime;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            base.ConstructCommon(state, getPawnId);
            
            if (_output.IsSet) {
                State.ObserveHitsOn(PawnId)
                    .Where(_ => IsStateActive)
                    .CatchIgnoreLog()
                    .Subscribe(hit => {
                        _hitPauseTime = hit.PauseTime;
                        _hitTime = hit.LastUpdated;
                        animator.SetFloat(_output.Hash, 0f);
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            var timeElapsed = (State.GetTime() - _hitTime) * TimeScale;
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