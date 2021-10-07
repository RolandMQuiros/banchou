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

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            
            if (!string.IsNullOrWhiteSpace(_hitTrigger)) {
                var hitTriggerHash = Animator.StringToHash(_hitTrigger);
                state.ObserveCombatant(pawnId)
                    .Where(combatant => combatant.StunTime > 0)
                    .CatchIgnoreLog()
                    .Subscribe(_ => {
                        animator.SetTrigger(hitTriggerHash);
                    })
                    .AddTo(this);
            }
            
            if (!string.IsNullOrWhiteSpace(_stunTimeFloat)) {
                var stunTimeHash = Animator.StringToHash(_stunTimeFloat);
                ObserveStateUpdate
                    .WithLatestFrom(state.ObserveCombatant(pawnId), (_, combatant) => combatant)
                    .Select(combatant => combatant.StunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(stunTimeHash, stunTime))
                    .AddTo(this);
            }
        }
    }
}