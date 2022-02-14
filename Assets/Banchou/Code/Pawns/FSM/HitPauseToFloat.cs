using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitPauseToFloat : FSMBehaviour {
        [SerializeField, Tooltip("Float parameter to set the hit pause time")]
        private FSMParameter _output = new(AnimatorControllerParameterType.Float);

        [SerializeField]
        private bool _normalized = true;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            var pawnId = getPawnId();
            if (_output.IsSet) {
                state.ObserveHitsOn(pawnId)
                    .Where(hit => IsStateActive && hit.StunTime > 0)
                    .CombineLatest(ObserveStateUpdate, (hit, _) => hit)
                    .Select(hit => _normalized ? hit.NormalizedPauseTimeAt(state.GetTime()) :
                        hit.PauseTimeAt(state.GetTime()))
                    .CatchIgnoreLog()
                    .Subscribe(stunTime => animator.SetFloat(_output.Hash, stunTime))
                    .AddTo(this);
            }
        }
    }
}