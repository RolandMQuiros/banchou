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
            
            var hitTriggerHash = Animator.StringToHash(_hitTrigger);
            if (hitTriggerHash != 0) {
                state.ObserveCombatant(pawnId)
                    .Where(combatant => combatant.StunTime > 0)
                    .CatchIgnoreLog()
                    .Subscribe(_ => {
                        animator.SetTrigger(hitTriggerHash);
                    })
                    .AddTo(this);
            }
            
            var stunTimeHash = Animator.StringToHash(_stunTimeFloat);
            if (stunTimeHash != 0) {
                ObserveStateUpdate
                    .WithLatestFrom(state.ObserveCombatant(pawnId), (_, combatant) => combatant)
                    .Select(combatant => combatant.StunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(stunTimeHash, stunTime))
                    .AddTo(this);
            }

            var normalizedStunTimeHash = Animator.StringToHash(_stunTimeNormalizedFloat);
            if (normalizedStunTimeHash != 0) {
                ObserveStateUpdate
                    .WithLatestFrom(state.ObserveCombatant(pawnId), (_, combatant) => combatant)
                    .Where(combatant => combatant.StunTime > 0f)
                    .Select(combatant => (state.GetTime() - combatant.LastUpdated) / combatant.StunTime)
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(normalizedStunTimeHash, stunTime))
                    .AddTo(this);
            }
        }
    }
}