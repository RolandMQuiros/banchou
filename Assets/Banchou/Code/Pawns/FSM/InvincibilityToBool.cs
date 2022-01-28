using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class InvincibilityToBool : FSMBehaviour {
        [SerializeField] private string _outputParameter;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var hash = Animator.StringToHash(_outputParameter);
            if (hash != default) {
                state.ObserveDefense(getPawnId())
                    .DistinctUntilChanged(defense => defense.IsInvincible)
                    .CatchIgnoreLog()
                    .Subscribe(defense => { animator.SetBool(hash, defense.IsInvincible); })
                    .AddTo(this);
            }
        }
    }
}