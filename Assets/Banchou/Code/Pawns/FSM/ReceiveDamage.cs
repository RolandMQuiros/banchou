using UnityEngine;
using UniRx;

using Banchou.Combatant;

namespace Banchou.Pawn.FSM {
    public class ReceiveDamage : FSMBehaviour {
        [SerializeField] private string _hitTriggerParameter;
        [SerializeField] private string _stunTimeFloatParameter;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            
            if (!string.IsNullOrWhiteSpace(_hitTriggerParameter)) {
                var hitTriggerHash = Animator.StringToHash(_hitTriggerParameter);
                state.ObserveCombatant(pawnId)
                    .Where(combatant => combatant.StunTime > 0)
                    .CatchIgnoreLog()
                    .Subscribe(_ => {
                        animator.SetTrigger(hitTriggerHash);
                    })
                    .AddTo(this);
            }
            
            if (!string.IsNullOrWhiteSpace(_stunTimeFloatParameter)) {
                var stunTimeHash = Animator.StringToHash(_stunTimeFloatParameter);
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