using UnityEngine;
using UniRx;

using Banchou.Combatant;

namespace Banchou.Pawn.FSM {
    public class ReceiveHitStun : FSMBehaviour {
        [Header("Output Parameters")]
        [SerializeField, Tooltip("Trigger parameter to set when hitstun is applied")]
        private string _hitTrigger;
        [SerializeField, Tooltip("Float parameter to set the normalized hit pause time")]
        private string _pauseTimeNormalizedFloat;
        [SerializeField, Tooltip("Float parameter to set the normalized hitstun time")]
        private string _stunTimeNormalizedFloat;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();

            var normalizedStunTimeHash = Animator.StringToHash(_stunTimeNormalizedFloat);
            if (normalizedStunTimeHash != 0) {
                state.ObserveNormalizedHitStun(pawnId, ObserveStateUpdate)
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(normalizedStunTimeHash, stunTime))
                    .AddTo(this);
            }

            var normalizedPauseTimeHash = Animator.StringToHash(_pauseTimeNormalizedFloat);
            if (normalizedPauseTimeHash != 0) {
                state.ObserveNormalizedHitPause(pawnId, ObserveStateUpdate)
                    .CatchIgnoreLog()
                    .Subscribe(pauseTime => animator.SetFloat(normalizedPauseTimeHash, pauseTime))
                    .AddTo(this);
            }
            
            var hitTriggerHash = Animator.StringToHash(_hitTrigger);
            if (hitTriggerHash != 0) {
                state.ObserveHitsOn(pawnId)
                    .Where(hit => hit.StunTime > 0)
                    .CatchIgnoreLog()
                    .Subscribe(_ => { animator.SetTrigger(hitTriggerHash); })
                    .AddTo(this);
            }
        }
    }
}