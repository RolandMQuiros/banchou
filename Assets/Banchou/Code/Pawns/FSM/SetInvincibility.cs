using System;
using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class SetInvincibility : PawnFSMBehaviour {
        [Flags] private enum ApplyEvent { OnEnter = 1, AtTime = 2, OnExit = 4 }
        private enum ApplyMode { Set, Unset, Toggle }


        [SerializeField] private ApplyEvent _onEvent;
        [SerializeField] private ApplyMode _applyMode;
        [SerializeField, Min(0f)] private float _atTime;
        
        private CombatantState _combatant;
        private bool _applied;
        private float _stateStartTime;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            ConstructCommon(state, getPawnId);
            State.ObserveCombatant(PawnId)
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
            _combatant.Defense.SetInvincibility(value, State.GetTime());
        }
        

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) {
                Apply();
            }
            _applied = false;
            _stateStartTime = State.GetTime();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.AtTime)) {
                if (!_applied && _stateStartTime >= _atTime) {
                    Apply();
                    _applied = true;
                }
            }
        }
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) {
                Apply();
            }
            _applied = false;
            _stateStartTime = 0f;
        }
    }
}