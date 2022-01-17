using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitToTrigger : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit pause time")]
        private string _outputParameter;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();

            var hitTriggerHash = Animator.StringToHash(_outputParameter);
            if (hitTriggerHash != 0) {
                state.ObserveLastHitChanges(pawnId)
                    .Where(hit => IsStateActive && hit.StunTime > 0)
                    .CatchIgnoreLog()
                    .Subscribe(_ => { animator.SetTrigger(hitTriggerHash); })
                    .AddTo(this);
            }
        }
    }
}