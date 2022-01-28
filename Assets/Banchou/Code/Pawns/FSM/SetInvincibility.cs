using System;
using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class SetInvincibility : FSMBehaviour {
        [Flags] private enum ApplyEvent { OnEnter = 1, AtTime = 2, OnExit = 4 }
        private enum ApplyMode { Set, Unset, Toggle }


        [SerializeField] private ApplyEvent _onEvent;
        [SerializeField] private ApplyMode _applyMode;
        [SerializeField, Min(0f)] private float _atTime;

        private GameState _state;
        private CombatantState _combatant;
        private bool _applied;
        private float _stateTime;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            _state.ObserveCombatant(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(combatant => _combatant = combatant)
                .AddTo(this);
        }

        private void Apply() {
            bool value = false;
            switch (_applyMode) {
                case ApplyMode.Set:
                    value = true;
                    break;
                case ApplyMode.Unset:
                    value = false;
                    break;
                case ApplyMode.Toggle:
                    value = !_combatant.Defense.IsInvincible;
                    break;
            }
            _combatant.Defense.SetInvincibility(value, _state.GetTime());
        }
        

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) {
                Apply();
            }
            _applied = false;
            _stateTime = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.AtTime)) {
                _stateTime += _state.GetDeltaTime();
                if (!_applied && _stateTime >= _atTime) {
                    Apply();
                    _applied = true;
                }
            }
        }
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) {
                Apply();
            }
            _applied = false;
            _stateTime = 0f;
        }
    }
}