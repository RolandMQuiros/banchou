using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitStunToFloat : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit stun time")]
        private string _outputParameter;

        [SerializeField]
        private bool _normalized = true;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();

            var hash = Animator.StringToHash(_outputParameter);
            if (hash != 0) {
                state.ObserveLastHitChanges(pawnId)
                    .Where(hit => IsStateActive && hit.StunTime > 0)
                    .CombineLatest(ObserveStateUpdate, (hit, _) => hit)
                    .Select(hit => _normalized ? hit.NormalizedStunTimeAt(state.GetTime()) :
                        hit.StunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(hash, stunTime))
                    .AddTo(this);
            }
        }
    }
}