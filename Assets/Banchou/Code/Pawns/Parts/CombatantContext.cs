using UnityEngine;
using Banchou.Combatant;

namespace Banchou.Pawn.Part {
    public class CombatantContext : MonoBehaviour {
        [SerializeField] private CombatantState _combatant;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            _combatant = state.GetCombatant(pawnId);
            if (_combatant == null) {
                state.SetCombatant(out _combatant, pawnId);
            }
        }
    }
}