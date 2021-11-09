using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyKnockbackForce : FSMBehaviour {
        [SerializeField] private float _multiplier = 1f;

        private GameState _state;
        private CombatantState _combatant;
        
        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody) {
            _state = state;
            _state.ObserveLastHitChanges(getPawnId())
                .Where(_ => IsStateActive)
                .CatchIgnoreLog()
                .Subscribe(hit => {
                    Debug.Log($"Knockback force applied at {state.GetTime()}");
                    rigidbody.velocity = _multiplier * hit.Knockback;
                })
                .AddTo(this);
        }
    }
}