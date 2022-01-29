using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitStunToFloat : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit stun time")]
        private FSMParameter _output = new(AnimatorControllerParameterType.Float);

        [SerializeField]
        private bool _normalized = true;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            if (_output.IsSet) {
                state.ObserveLastHitChanges(pawnId)
                    .Where(hit => IsStateActive && hit.StunTime > 0)
                    .CombineLatest(ObserveStateUpdate, (hit, _) => hit)
                    .Select(hit => _normalized ? hit.NormalizedStunTimeAt(state.GetTime()) :
                        hit.StunTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(_output.Hash, stunTime))
                    .AddTo(this);
            }
        }
    }
}