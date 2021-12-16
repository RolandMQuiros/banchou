using UniRx;
using UnityEngine;

using Banchou.Combatant;

namespace Banchou.Pawn.FSM {
    public class LockedOnToBool : FSMBehaviour {
        [SerializeField] private string _outputParameter;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var hash = Animator.StringToHash(_outputParameter);
            
            state.ObserveCombatantChanges(getPawnId())
                .Where(_ => IsStateActive)
                .Subscribe(combatant => animator.SetBool(hash, combatant.LockOnTarget != default))
                .AddTo(this);
        }
    }
}