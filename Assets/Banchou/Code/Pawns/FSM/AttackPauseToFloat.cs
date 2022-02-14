using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class AttackPauseToFloat : FSMBehaviour {
        [SerializeField] private FSMParameter _output = new(AnimatorControllerParameterType.Float);
        private GameState _state;
        private AttackState _attack;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObserveAttack(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(attack => _attack = attack)
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_output.IsSet) {
                animator.SetFloat(_output.Hash, _attack.NormalizedPauseTimeAt(_state.GetTime()));
            }
        }
    }
}