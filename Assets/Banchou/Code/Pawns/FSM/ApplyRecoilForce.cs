using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyRecoilForce : FSMBehaviour {
        [SerializeField] private float _multiplier = 1f;
        
        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody) {
            state.ObserveConfirmedAttack(getPawnId())
                .Where(_ => IsStateActive)
                .CatchIgnoreLog()
                .Subscribe(attack => { rigidbody.velocity = _multiplier * attack.Recoil; })
                .AddTo(this);
        }
    }
}