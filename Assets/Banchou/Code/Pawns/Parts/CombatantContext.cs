using UniRx;
using UnityEngine;

using Banchou.Combatant;

namespace Banchou.Pawn.Part {
    public class CombatantContext : MonoBehaviour {
        [SerializeField] private CombatantState _combatant;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            state.ObserveCombatant(pawnId)
                .Subscribe(c => _combatant = c)
                .AddTo(this);
        }
    }
}