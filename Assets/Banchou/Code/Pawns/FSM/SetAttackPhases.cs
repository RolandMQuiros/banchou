using Banchou.Combatant;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetAttackPhases : FSMBehaviour {
        private GameState _state;
        private AttackState _attackState;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _attackState = _state.GetCombatantAttack(getPawnId());
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _attackState.Start(_state.GetTime());
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _attackState.Finish(_state.GetTime());
        }
    }
}