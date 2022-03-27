using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyRecoilForce : FSMBehaviour {
        [SerializeField] private float _multiplier = 1f;
        
        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody, Animator animator) {
            var pawnId = getPawnId();
            state.ObserveAttacksBy(pawnId)
                .Where(_ => IsStateActive)
                .WithLatestFrom(state.ObservePawnTimeScale(pawnId), (attack, timeScale) => (attack, timeScale))
                .CatchIgnoreLog()
                .Subscribe(args => {
                    var (attack, timeScale) = args;
                    rigidbody.AddForce(_multiplier * timeScale * attack.Recoil, ForceMode.VelocityChange);
                })
                .AddTo(this);
        }
    }
}