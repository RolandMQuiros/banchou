using System.Collections;
using System.Collections.Generic;
using Banchou.Combatant;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class AttackPhases : MonoBehaviour {
        private GameState _state;
        private AttackState _attackState;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _attackState = state.GetCombatantAttack(getPawnId());
        }
        
        public void StartAttack() => _attackState.Start(_state.GetTime());
        public void ActivateAttack() => _attackState.Activate(_state.GetTime());
        public void RecoverAttack() => _attackState.Recover(_state.GetTime());
    }
}