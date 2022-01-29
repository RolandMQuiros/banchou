using UniRx;
using UnityEngine;

using Banchou.Combatant;

namespace Banchou.Pawn.FSM {
    public class LockedOnToBool : FSMBehaviour {
        [SerializeField] private FSMParameter _output = new(AnimatorControllerParameterType.Bool);

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            if (_output.IsSet) {
                state.ObserveCombatantChanges(getPawnId())
                    .Where(_ => IsStateActive)
                    .Subscribe(combatant => animator.SetBool(_output.Hash, combatant.LockOnTarget != default))
                    .AddTo(this);
            }
        }
    }
}