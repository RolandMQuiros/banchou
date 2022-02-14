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
        [SerializeField, Tooltip("Float parameter to set the hitstun time")]
        private string _stunTimeFloat;
        [SerializeField, Tooltip("Float parameter to set the normalized hitstun time")]
        private string _stunTimeNormalizedFloat;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            
            var stunTimeHash = Animator.StringToHash(_stunTimeFloat);
            if (stunTimeHash != 0) {
                ObserveStateUpdate
                    .WithLatestFrom(state.ObserveHitsOn(pawnId), (_, hit) => hit.StunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(stunTimeHash, stunTime))
                    .AddTo(this);
            }

            var normalizedStunTimeHash = Animator.StringToHash(_stunTimeNormalizedFloat);
            if (normalizedStunTimeHash != 0) {
                ObserveStateUpdate
                    .WithLatestFrom(state.ObserveHitsOn(pawnId), (_, hit) => hit.NormalizedStunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(normalizedStunTimeHash, stunTime))
                    .AddTo(this);
            }

            var normalizedPauseTimeHash = Animator.StringToHash(_pauseTimeNormalizedFloat);
            if (normalizedPauseTimeHash != 0) {
                ObserveStateUpdate
                    .WithLatestFrom(state.ObserveHitsOn(pawnId), (_, hit) => hit.NormalizedStunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(pauseTime => animator.SetFloat(normalizedPauseTimeHash, pauseTime))
                    .AddTo(this);
            }
            
            var hitTriggerHash = Animator.StringToHash(_hitTrigger);
            if (hitTriggerHash != 0) {
                state.ObserveHitsOn(pawnId)
                    .Where(hit => hit.StunTime > 0)
                    .CatchIgnoreLog()
                    .Subscribe(hit => {
                        animator.SetTrigger(hitTriggerHash);

                        var now = state.GetTime();
                        if (normalizedPauseTimeHash != 0) {
                            animator.SetFloat(normalizedPauseTimeHash, hit.NormalizedPauseTimeAt(now));
                        }
                        
                        if (stunTimeHash != 0) {
                            animator.SetFloat(stunTimeHash, hit.StunTimeAt(now));
                        }

                        if (normalizedStunTimeHash != 0) {
                            animator.SetFloat(normalizedStunTimeHash, hit.NormalizedStunTimeAt(now));
                        }
                    })
                    .AddTo(this);
            }
        }
    }
}