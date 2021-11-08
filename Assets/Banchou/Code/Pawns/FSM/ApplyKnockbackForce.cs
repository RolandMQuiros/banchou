using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyKnockbackForce : FSMBehaviour {
        [SerializeField] private float _multiplier = 100f;
        [SerializeField] private ForceMode _forceMode = ForceMode.Force;

        private GameState _state;
        private CombatantState _combatant;
        
        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody) {
            _state = state;
            _state.ObserveCombatant(getPawnId())
                .Where(_ => IsStateActive)
                .DistinctUntilChanged(combatant => combatant.KnockedBackWhen)
                .CatchIgnoreLog()
                .Subscribe(combatant => {
                    rigidbody.AddForce(_multiplier * combatant.Knockback, _forceMode);
                })
                .AddTo(this);
        }
    }
}