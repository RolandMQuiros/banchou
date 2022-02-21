using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class AttackTimeToFloat : FSMBehaviour {
        [SerializeField] private string _outputParameter;

        private int _outputHash;
        
        private GameState _state;
        private AttackState _attack;
        private float _attackTime;
        private float _stateLength;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();

            _state = state;
            _state.ObserveAttack(pawnId)
                .CatchIgnoreLog()
                .Subscribe(attack => _attack = attack)
                .AddTo(this);

            _outputHash = Animator.StringToHash(_outputParameter);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _attackTime = 0f;
            _stateLength = stateInfo.length;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_attack.NormalizedPauseTimeAt(_state.GetTime()) >= 1f) {
                _attackTime += _state.GetDeltaTime();
            }

            if (_outputHash != default) {
                animator.SetFloat(_outputHash, Mathf.Clamp01(_attackTime / stateInfo.length));
            }
        }
    }
}