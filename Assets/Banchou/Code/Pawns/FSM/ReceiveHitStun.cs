using UnityEngine;
using UniRx;

using Banchou.Combatant;

namespace Banchou.Pawn.FSM {
    public class ReceiveHitstun : FSMBehaviour {
        [Header("Output Parameters")]
        [SerializeField, Tooltip("Trigger parameter to set when hitstun is applied")]
        private string _hitTrigger;
        [SerializeField, Tooltip("Float parameter to set the hitstun time")]
        private string _stunTimeFloat;
        [SerializeField, Tooltip("Float parameter to set the normalized hitstun time")]
        private string _stunTimeNormalizedFloat;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            
            var stunTimeHash = Animator.StringToHash(_stunTimeFloat);
            if (stunTimeHash != 0) {
                ObserveStateUpdate
                    .Select(_ => state.GetCombatantLastHit(pawnId)
                        .StunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(stunTimeHash, stunTime))
                    .AddTo(this);
            }

            var normalizedStunTimeHash = Animator.StringToHash(_stunTimeNormalizedFloat);
            if (normalizedStunTimeHash != 0) {
                ObserveStateUpdate
                    .Select(_ => state.GetCombatantLastHit(pawnId)
                        .NormalizedStunTimeAt(state.GetTime()))
                    .Where(stunTime => stunTime <= 1f)
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(normalizedStunTimeHash, stunTime))
                    .AddTo(this);
            }
            
            var hitTriggerHash = Animator.StringToHash(_hitTrigger);
            if (hitTriggerHash != 0) {
                state.ObserveLastHitChanges(pawnId)
                    .Where(hit => hit.StunTime > 0)
                    .CatchIgnoreLog()
                    .Subscribe(hit => {
                        animator.SetTrigger(hitTriggerHash);
                        if (stunTimeHash != 0) {
                            animator.SetFloat(stunTimeHash, hit.StunTimeAt(state.GetTime()));
                        }

                        if (normalizedStunTimeHash != 0) {
                            animator.SetFloat(normalizedStunTimeHash, hit.NormalizedStunTimeAt(state.GetTime()));
                        }
                    })
                    .AddTo(this);
            }
        }
    }
}