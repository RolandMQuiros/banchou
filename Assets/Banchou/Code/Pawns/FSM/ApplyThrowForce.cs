using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyThrowForce : FSMBehaviour {
        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody) {
            state.ObserveGrabReleasesOn(getPawnId())
                .Where(_ => IsStateActive)
                .CatchIgnoreLog()
                .Subscribe(grab => {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.AddForce(grab.LaunchForce, ForceMode.VelocityChange);
                })
                .AddTo(this);
        }
    }
}