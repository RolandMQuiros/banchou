using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyThrowForce : FSMBehaviour {
        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody, Animator animator) {
            var pawnId = getPawnId();
            state.ObserveGrabReleasesOn(pawnId)
                .Where(_ => IsStateActive)
                .WithLatestFrom(state.ObservePawnTimeScale(pawnId), (grab, timeScale) => (grab, timeScale))
                .CatchIgnoreLog()
                .Subscribe(args => {
                    var (grab, timeScale) = args;
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.AddForce(timeScale * grab.LaunchForce, ForceMode.VelocityChange);
                })
                .AddTo(this);
        }
    }
}