using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class MoveFromKnockback : FSMBehaviour {
        private GameState _state;
        private PawnSpatial _spatial;
        private CombatantState _combatant;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();

            _state = state;
            _state.ObservePawnSpatial(pawnId)
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);

            _state.ObserveCombatant(pawnId)
                .Subscribe(combatant => _combatant = combatant)
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_spatial == null || _combatant == null) return;
            var now = _state.GetTime();
            var delta = _state.GetDeltaTime();
            var knockback = _combatant.KnockbackAt(now);
            if (knockback != Vector3.zero) {
                _spatial.Move(knockback * delta, now);   
            }
        }
    }
}