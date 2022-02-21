using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class StartAttack : FSMBehaviour {
        private GameState _state;
        private AttackState _attack;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObserveCombatant(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(combatant => _attack = combatant.Attack)
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _attack?.Start(_state.GetTime());
        }
    }
}