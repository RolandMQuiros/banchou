using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class SetInvincibility : FSMBehaviour {
        private enum RelativeTime {
            NormalizedStunTime,
            NormalizedTime
        };

        [SerializeField] private RelativeTime _relativeTo = RelativeTime.NormalizedStunTime;
        [SerializeField, Range(0, 1f)] private float _from = 0f;
        [SerializeField, Range(0, 1f)] private float _until = 0f;

        private GameState _state;
        private CombatantState _combatant;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObserveCombatant(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(combatant => _combatant = combatant)
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            float time = 0f;
            switch (_relativeTo) {
                case RelativeTime.NormalizedTime:
                    time = stateInfo.normalizedTime % 1f;
                    break;
                case RelativeTime.NormalizedStunTime:
                    time = _combatant.LastHit.NormalizedStunTimeAt(_state.GetTime());
                    break;
            }

            if (!_combatant.Defense.IsInvincible && time > _from && time < _until) {
                _combatant.Defense.SetInvincibility(true, _state.GetTime());
            } else if (_combatant.Defense.IsInvincible) {
                _combatant.Defense.SetInvincibility(false, _state.GetTime());
            }
        }
    }
}