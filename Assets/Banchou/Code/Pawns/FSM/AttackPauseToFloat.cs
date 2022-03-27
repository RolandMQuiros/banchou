using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class AttackPauseToFloat : FSMBehaviour {
        [SerializeField] private FSMParameter _output = new(AnimatorControllerParameterType.Float);

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            state.ObserveNormalizedAttackPause(getPawnId(), ObserveStateUpdate)
                .CatchIgnoreLog()
                .Subscribe(pauseTime => animator.SetFloat(_output.Hash, pauseTime))
                .AddTo(this);
        }
    }
}