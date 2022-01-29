using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class InvincibilityToBool : FSMBehaviour {
        [SerializeField] private FSMParameter _output = new(AnimatorControllerParameterType.Bool);

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            if (_output.IsSet) {
                state.ObserveDefense(getPawnId())
                    .DistinctUntilChanged(defense => defense.IsInvincible)
                    .CatchIgnoreLog()
                    .Subscribe(defense => { animator.SetBool(_output.Hash, defense.IsInvincible); })
                    .AddTo(this);
            }
        }
    }
}