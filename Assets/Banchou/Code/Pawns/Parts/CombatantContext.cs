using UniRx;
using UnityEngine;

using Banchou.Combatant;

namespace Banchou.Pawn.Part {
    public class CombatantContext : MonoBehaviour {
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private CombatantState _combatant;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            var combatant = state.GetCombatant(pawnId);
            
            if (combatant == null) {
                state.SetCombatant(out _combatant, pawnId, _maxHealth);
            }

            state.ObserveCombatant(pawnId)
                .Subscribe(combatant => _combatant = combatant)
                .AddTo(this);
        }
    }
}