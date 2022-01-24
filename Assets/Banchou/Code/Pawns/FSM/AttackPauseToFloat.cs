using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class AttackPauseToFloat : FSMBehaviour {
        [SerializeField] private string _outputParameter;

        private GameState _state;
        private AttackState _attack;
        private int _outputHash;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObserveLastAttack(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(attack => _attack = attack)
                .AddTo(this);
            _outputHash = Animator.StringToHash(_outputParameter);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_outputHash != default) {
                animator.SetFloat(_outputHash, _attack.NormalizedPauseTimeAt(_state.GetTime()));
            }
        }
    }
}