using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetAttackPhases : FSMBehaviour {
        private GameState _state;
        private AttackState _attackState;

        private int _attackId;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObserveAttack(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(attack => _attackState = attack)
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _attackId = _attackState.Start(_state.GetTime()).AttackId;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_attackId == _attackState.AttackId) {
                _attackState.Finish(_state.GetTime());
            }
        }
    }
}